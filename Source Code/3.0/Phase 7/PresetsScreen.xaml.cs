using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RandomCreation
{
    public partial class PresetsScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);
        public Action? OnClosed { get; set; }

        // ── Selection + edit state ────────────────────────────────────────────
        private Preset?        _selectedPreset;
        private PresetViewModel? _editingPreset;
        private readonly HashSet<string> _expandedPresets = new();

        // ── Drag state ───────────────────────────────────────────────────────
        private Point   _dragStartPos;
        private PresetViewModel? _dragSource;
        private bool    _dragActive;
        private int     _dropIndex = -1;
        private Popup?  _ghostPopup;
        private Popup?  _linePopup;
        private Border? _ghostBorder;

        public PresetsScreen() => InitializeComponent();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BackButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var desc = UndoService.Undo();
                if (desc != null) ToastService.Show($"Undone: {desc}");
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (_selectedPreset != null)
                    DeletePreset(_selectedPreset);
                e.Handled = true;
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_selectedPreset != null)
                    DuplicatePreset(_selectedPreset);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        public void Refresh()
        {
            var presets = DataService.Presets.Presets;
            if (presets.Count == 0)
            {
                PresetsList.ItemsSource = null;
                EmptyText.Visibility    = Visibility.Visible;
                _selectedPreset         = null;
                return;
            }
            EmptyText.Visibility = Visibility.Collapsed;
            PresetsList.ItemsSource = presets.Select(p =>
            {
                var vm = new PresetViewModel(p, p == _selectedPreset);
                if (_expandedPresets.Contains(p.Name)) vm.ToggleExpanded();
                return vm;
            }).ToList();
        }


        // ── Selection ─────────────────────────────────────────────────────────

        private void PresetRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement src && IsChildOfButton(src)) return;
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                _selectedPreset = _selectedPreset == vm.Model ? null : vm.Model;
                Refresh();
            }
        }


        // ── Inline edit ───────────────────────────────────────────────────────

        private void PresetNameBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock tb) return;
            var rowBorder = FindParentOfType<Border>(tb);
            if (rowBorder?.Tag is not PresetViewModel vm) return;
            if (_selectedPreset != vm.Model) return;
            e.Handled = true;

            var box = FindChildByName<TextBox>(rowBorder, "PresetNameBox");
            if (box == null) return;
            _editingPreset = vm;
            box.Text       = vm.Name;
            box.Visibility = Visibility.Visible;
            tb.Visibility  = Visibility.Collapsed;
            box.Focus(); box.SelectAll();
        }

        private void PresetNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            if (e.Key == Key.Enter)       { CommitPresetEdit(box); e.Handled = true; }
            else if (e.Key == Key.Escape) { CancelPresetEdit(box); e.Handled = true; }
        }

        private void PresetNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox box) CommitPresetEdit(box);
        }

        private void CommitPresetEdit(TextBox box)
        {
            if (_editingPreset == null) return;
            var vm         = _editingPreset;
            _editingPreset = null;
            string newName = box.Text.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != vm.Name)
            {
                if (DataService.Presets.Presets.Any(p => p != vm.Model && p.Name == newName))
                {
                    ToastService.Show($"A preset named \"{newName}\" already exists");
                    _editingPreset = vm;
                    box.Focus(); box.SelectAll(); return;
                }
                string oldName = vm.Name;
                _expandedPresets.Remove(vm.Name);
                vm.Model.Name = newName;
                if (_expandedPresets.Contains(vm.Name))
                    _expandedPresets.Add(newName);

                UndoService.Push($"Rename preset to {newName}", () =>
                {
                    vm.Model.Name = oldName;
                    DataService.SavePresets();
                    Refresh();
                });

                DataService.SavePresets();
            }

            box.Visibility = Visibility.Collapsed;
            Refresh();
        }

        private void CancelPresetEdit(TextBox box)
        {
            _editingPreset = null;
            box.Visibility = Visibility.Collapsed;
            Refresh();
        }


        // ── Duplicate ─────────────────────────────────────────────────────────

        private void DuplicatePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
                DuplicatePreset(vm.Model);
        }

        private void DuplicatePreset(Preset src)
        {
            var presets  = DataService.Presets.Presets;
            string baseName = StripCounterSuffix(src.Name);
            string newName  = baseName;
            if (presets.Any(p => p.Name == newName))
            {
                int counter = 2;
                while (presets.Any(p => p.Name == $"{baseName} ({counter})")) counter++;
                newName = $"{baseName} ({counter})";
            }

            var newPreset = new Preset
            {
                Name        = newName,
                Collections = src.Collections
                    .Select(c => new PresetCollectionState
                    {
                        CollectionName = c.CollectionName,
                        IsEnabled      = c.IsEnabled,
                        Groups         = c.Groups
                            .Select(g => new PresetGroupState
                            {
                                GroupName  = g.GroupName,
                                IsEnabled  = g.IsEnabled,
                                Categories = g.Categories
                                    .Select(cat => new PresetCategoryState
                                    {
                                        CategoryName = cat.CategoryName,
                                        IsEnabled    = cat.IsEnabled,
                                        Options      = cat.Options
                                            .Select(o => new PresetOptionState
                                            {
                                                OptionName = o.OptionName,
                                                IsEnabled  = o.IsEnabled
                                            }).ToList()
                                    }).ToList()
                            }).ToList()
                    }).ToList()
            };

            int insertAt = presets.IndexOf(src) + 1;
            presets.Insert(insertAt, newPreset);
            _selectedPreset = newPreset;

            UndoService.Push($"Duplicate preset {src.Name}", () =>
            {
                presets.Remove(newPreset);
                if (_selectedPreset == newPreset) _selectedPreset = src;
                DataService.SavePresets();
                Refresh();
            });

            DataService.SavePresets();
            Refresh();
            ToastService.Show($"Duplicated \"{src.Name}\" → \"{newName}\"");
        }

        private static string StripCounterSuffix(string name)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(.*?) \(\d+\)$");
            string current = name;
            while (true)
            {
                var m = regex.Match(current);
                if (!m.Success) return current;
                current = m.Groups[1].Value;
            }
        }


        // ── Delete ────────────────────────────────────────────────────────────

        private void DeletePreset(Preset preset)
        {
            if (DataService.Settings.ConfirmOnDelete)
            {
                if (new ConfirmDialog("Confirm Delete",
                    $"Delete preset \"{preset.Name}\"? This cannot be undone.")
                { Owner = Main }.ShowDialog() != true) return;
            }

            var presets  = DataService.Presets.Presets;
            var snapshot = presets.ToList();
            _expandedPresets.Remove(preset.Name);
            presets.Remove(preset);
            if (_selectedPreset == preset) _selectedPreset = null;

            UndoService.Push($"Delete preset {preset.Name}", () =>
            {
                DataService.Presets.Presets.Clear();
                foreach (var p in snapshot) DataService.Presets.Presets.Add(p);
                DataService.SavePresets();
                Refresh();
            });

            DataService.SavePresets();
            Refresh();
        }


        // ── Load ──────────────────────────────────────────────────────────────

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                var dlg = new ConfirmDialog("Load Preset",
                    $"Load \"{vm.Name}\"? This will replace your current enabled states.",
                    "Cancel", "Load") { Owner = Main };
                if (dlg.ShowDialog() != true) return;
                DataService.ApplyPreset(vm.Model);
                Main.RefreshGenerateButtonState();
                Main.RefreshMainScreen();
                BackButton_Click(this, new RoutedEventArgs());
            }
        }

        // ── Expand row (click on non-name area) ───────────────────────────────

        private void PresetHeader_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement src && IsChildOfButton(src)) return;
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                vm.ToggleExpanded();
                if (vm.IsExpanded) _expandedPresets.Add(vm.Name);
                else               _expandedPresets.Remove(vm.Name);
                Refresh();
            }
        }


        // ── Save current ──────────────────────────────────────────────────────

        private void SaveCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SavePresetDialog() { Owner = Main };
            if (dialog.ShowDialog() == true)
            {
                if (dialog.OverwriteTarget != null)
                {
                    var confirm = new ConfirmDialog("Replace Preset",
                        $"Replace \"{dialog.OverwriteTarget.Name}\" with your current state?",
                        "Cancel", "Replace") { Owner = Main };
                    if (confirm.ShowDialog() != true) return;
                    int idx = DataService.Presets.Presets.IndexOf(dialog.OverwriteTarget);
                    DataService.Presets.Presets[idx] =
                        DataService.CaptureCurrentStateAsPreset(dialog.OverwriteTarget.Name);
                }
                else
                {
                    DataService.Presets.Presets.Add(
                        DataService.CaptureCurrentStateAsPreset(dialog.PresetName));
                }
                DataService.SavePresets();
                Refresh();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnClosed?.Invoke();
            Main.HideOverlay();
        }


        // ── Drag and drop ─────────────────────────────────────────────────────

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                _dragSource    = vm;
                _dragStartPos  = e.GetPosition(this);
            }
        }

        private void PresetsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPos = e.GetPosition(this);
            _dragSource   = null;
        }

        private void PresetsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragSource == null || _dragActive) return;
            var pos = e.GetPosition(this);
            if (Math.Abs(pos.X - _dragStartPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pos.Y - _dragStartPos.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragSource.Name);
        }

        private void BeginDrag(string name)
        {
            _dragActive = true;
            _ghostBorder = new Border
            {
                Background      = (Brush)Application.Current.Resources["BackgroundCardBrush"],
                BorderBrush     = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0a, 0x84, 0xff)),
                BorderThickness = new Thickness(1.5, 1.5, 1.5, 1.5),
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(12, 6, 12, 6),
                Child           = new TextBlock
                {
                    Text       = name,
                    Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                    FontSize   = 13
                }
            };
            _ghostPopup = new Popup
            {
                Child           = _ghostBorder,
                IsOpen          = true,
                AllowsTransparency = true,
                Placement       = PlacementMode.Absolute
            };
            _linePopup = new Popup
            {
                Child = new Line
                {
                    Stroke          = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0a, 0x84, 0xff)),
                    StrokeThickness = 2,
                    X1 = 0, X2 = PresetsList.ActualWidth,
                    Y1 = 0, Y2 = 0
                },
                IsOpen             = true,
                AllowsTransparency = true,
                Placement          = PlacementMode.Absolute
            };

            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         += OnDragMouseMove;
                win.MouseLeftButtonUp += OnDragMouseUp;
            }
        }

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragActive || _ghostBorder == null) return;
            var pt = e.GetPosition(null);
            if (_ghostPopup != null)
            {
                _ghostPopup.HorizontalOffset = pt.X + 10;
                _ghostPopup.VerticalOffset   = pt.Y - 12;
            }
            UpdateDropIndex(e.GetPosition(PresetsList));
        }

        private void UpdateDropIndex(Point listPt)
        {
            int count = PresetsList.Items.Count;
            _dropIndex = count;
            for (int i = 0; i < count; i++)
            {
                var container = PresetsList.ItemContainerGenerator
                    .ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                var top = container.TranslatePoint(new Point(), PresetsList).Y;
                if (listPt.Y < top + container.ActualHeight / 2.0)
                {
                    _dropIndex = i;
                    break;
                }
            }
            if (_linePopup?.Child is Line line && _dropIndex >= 0)
            {
                double lineY = GetDropLineY(_dropIndex);
                var screen = PresetsList.PointToScreen(new Point(0, lineY));
                _linePopup.HorizontalOffset = screen.X;
                _linePopup.VerticalOffset   = screen.Y;
            }
        }

        private double GetDropLineY(int index)
        {
            int count = PresetsList.Items.Count;
            if (count == 0) return 0;
            if (index >= count)
            {
                var last = PresetsList.ItemContainerGenerator
                    .ContainerFromIndex(count - 1) as FrameworkElement;
                if (last != null)
                    return last.TranslatePoint(new Point(0, last.ActualHeight), PresetsList).Y;
            }
            var c = PresetsList.ItemContainerGenerator
                .ContainerFromIndex(Math.Min(index, count - 1)) as FrameworkElement;
            return c?.TranslatePoint(new Point(), PresetsList).Y ?? 0;
        }

        private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragActive) return;
            CommitDrop();
            EndDrag();
        }

        private void CommitDrop()
        {
            if (_dragSource == null || _dropIndex < 0) return;
            var presets  = DataService.Presets.Presets;
            var src      = _dragSource.Model;
            int fromIdx  = presets.IndexOf(src);
            if (fromIdx < 0 || _dropIndex == fromIdx || _dropIndex == fromIdx + 1) return;
            int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
            toIdx = Math.Max(0, Math.Min(toIdx, presets.Count - 1));
            presets.RemoveAt(fromIdx);
            presets.Insert(toIdx, src);
            DataService.SavePresets();
            Refresh();
        }

        private void EndDrag()
        {
            _dragActive = false;
            _dragSource = null;
            _dropIndex  = -1;
            if (_ghostPopup != null) { _ghostPopup.IsOpen = false; _ghostPopup = null; }
            if (_linePopup  != null) { _linePopup.IsOpen  = false; _linePopup  = null; }
            _ghostBorder = null;
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         -= OnDragMouseMove;
                win.MouseLeftButtonUp -= OnDragMouseUp;
            }
        }

        private void PresetsList_Drop(object sender, DragEventArgs e) { }


        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsChildOfButton(DependencyObject element)
        {
            var current = element;
            while (current != null)
            {
                if (current is Button) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private static T? FindParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            var current = VisualTreeHelper.GetParent(element);
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static T? FindChildByName<T>(DependencyObject root, string name)
            where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T fe && fe.Name == name) return fe;
                var found = FindChildByName<T>(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
