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

        private readonly HashSet<string> _expandedPresets = new();

        // ── Drag state ───────────────────────────────────────────────────────
        private Point _dragStartPos;
        private PresetViewModel? _dragSource;
        private bool   _dragActive;
        private int    _dropIndex = -1;
        private Popup? _ghostPopup;
        private Popup? _linePopup;
        private Border? _ghostBorder;

        public PresetsScreen() => InitializeComponent();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BackButton_Click(this, new RoutedEventArgs());
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
                return;
            }
            EmptyText.Visibility = Visibility.Collapsed;
            PresetsList.ItemsSource = presets.Select(p =>
            {
                var vm = new PresetViewModel(p);
                if (_expandedPresets.Contains(p.Name)) vm.ToggleExpanded();
                return vm;
            }).ToList();
        }

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

        private void RenamePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                var dlg = new InputDialog("Rename Preset", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicatePreset(
                        name, DataService.Presets.Presets, vm.Name))
                { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    DataService.SavePresets();
                    Refresh();
                }
            }
        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    if (new ConfirmDialog("Confirm Delete",
                        $"Delete preset \"{vm.Name}\"? This cannot be undone.")
                    { Owner = Main }.ShowDialog() != true) return;
                }
                DataService.Presets.Presets.Remove(vm.Model);
                _expandedPresets.Remove(vm.Name);
                DataService.SavePresets();
                Refresh();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnClosed?.Invoke();
            Main.HideOverlay();
        }

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

        // ════════════════════════════════════════════════════════════════════
        // DRAG AND DROP
        // ════════════════════════════════════════════════════════════════════

        private void PresetsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPos = e.GetPosition(null);
            _dragSource   = null;
            _dragActive   = false;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is PresetViewModel vm)
                _dragSource = vm;
        }

        private void PresetsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||
                _dragSource == null || _dragActive) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pos.Y - _dragStartPos.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragSource.Name);
        }

        private void PresetsList_Drop(object sender, DragEventArgs e) { }

        private void BeginDrag(string name)
        {
            _dragActive = true;
            double scale = FontScaleHelper.GetScale(DataService.Settings.FontSize);
            double width = PresetsList.ActualWidth * scale;

            _ghostBorder = new Border
            {
                Width           = width,
                Background      = (Brush)Application.Current.Resources["BackgroundCardBrush"],
                BorderBrush     = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                BorderThickness = new Thickness(1.5),
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(8, 7, 8, 7),
                Opacity         = 0.75,
                IsHitTestVisible = false,
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text      = "⠿",
                            Foreground = (Brush)Application.Current.Resources["BorderSelectedBrush"],
                            FontSize  = 14,
                            Margin    = new Thickness(0, 0, 8, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text      = name,
                            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                            FontSize  = 13,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            };

            _ghostPopup = new Popup
            {
                Child = _ghostBorder, IsOpen = false,
                AllowsTransparency = true,
                Placement = PlacementMode.AbsolutePoint,
                IsHitTestVisible = false
            };

            _linePopup = new Popup
            {
                Child = BuildLineCanvas(width), IsOpen = false,
                AllowsTransparency = true,
                Placement = PlacementMode.AbsolutePoint,
                IsHitTestVisible = false
            };

            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         += OnDragMouseMove;
                win.MouseLeftButtonUp += OnDragMouseUp;
            }

            _ghostPopup.IsOpen = true;
        }

        private static Canvas BuildLineCanvas(double width)
        {
            var canvas = new Canvas
                { Width = Math.Max(width, 10), Height = 10, IsHitTestVisible = false };
            canvas.Children.Add(new Line
            {
                X1 = 4, X2 = Math.Max(width - 4, 6), Y1 = 5, Y2 = 5,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                StrokeThickness = 2
            });
            var dotL = new Ellipse { Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)) };
            var dotR = new Ellipse { Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)) };
            Canvas.SetLeft(dotL, 0);              Canvas.SetTop(dotL, 2);
            Canvas.SetLeft(dotR, Math.Max(width - 6, 0)); Canvas.SetTop(dotR, 2);
            canvas.Children.Add(dotL);
            canvas.Children.Add(dotR);
            return canvas;
        }

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragActive || _ghostPopup == null) return;

            var screenPos = PointToScreen(e.GetPosition(this));
            _ghostPopup.HorizontalOffset = screenPos.X + 14;
            _ghostPopup.VerticalOffset   = screenPos.Y - 10;

            var listPos = Mouse.GetPosition(PresetsList);
            bool isOver = listPos.X >= 0 && listPos.Y >= 0 &&
                          listPos.X <= PresetsList.ActualWidth &&
                          listPos.Y <= PresetsList.ActualHeight;

            if (isOver)
            {
                SetGhostValid(true);
                _dropIndex = GetInsertionIndex(listPos);
                double lineY = GetInsertionY(_dropIndex);
                if (lineY >= 0 && _linePopup != null)
                {
                    var screenPt = PresetsList.PointToScreen(new Point(0, lineY));
                    _linePopup.HorizontalOffset = screenPt.X;
                    _linePopup.VerticalOffset   = screenPt.Y - 5;
                    _linePopup.IsOpen = true;
                }
            }
            else
            {
                SetGhostValid(false);
                if (_linePopup != null) _linePopup.IsOpen = false;
                _dropIndex = -1;
            }
        }

        private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragActive || _dragSource == null) return;

            var listPos = Mouse.GetPosition(PresetsList);
            bool isOver = listPos.X >= 0 && listPos.Y >= 0 &&
                          listPos.X <= PresetsList.ActualWidth &&
                          listPos.Y <= PresetsList.ActualHeight;

            if (isOver && _dropIndex >= 0)
            {
                var presets = DataService.Presets.Presets;
                int fromIdx = presets.IndexOf(_dragSource.Model);
                int toIdx   = _dropIndex;
                if (fromIdx >= 0 && toIdx >= 0 && fromIdx != toIdx)
                {
                    if (toIdx > fromIdx) toIdx--;
                    presets.RemoveAt(fromIdx);
                    presets.Insert(Math.Min(toIdx, presets.Count), _dragSource.Model);
                    DataService.SavePresets();
                    Refresh();
                }
            }

            EndDrag();
        }

        private void EndDrag()
        {
            _dragActive = false;
            _dropIndex  = -1;
            _dragSource = null;

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

        private void SetGhostValid(bool valid)
        {
            if (_ghostBorder == null) return;
            _ghostBorder.BorderBrush = valid
                ? new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff))
                : new SolidColorBrush(Color.FromRgb(0xe0, 0x50, 0x50));
        }

        private int GetInsertionIndex(Point pt)
        {
            int count = PresetsList.Items.Count;
            if (count == 0) return 0;
            for (int i = 0; i < count; i++)
            {
                var c = PresetsList.ItemContainerGenerator
                    .ContainerFromIndex(i) as FrameworkElement;
                if (c == null) continue;
                var bounds = new Rect(c.TranslatePoint(new Point(), PresetsList), c.RenderSize);
                if (pt.Y < bounds.Top + bounds.Height / 2) return i;
            }
            return count;
        }

        private double GetInsertionY(int index)
        {
            int count = PresetsList.Items.Count;
            if (count == 0) return -1;

            if (index >= count)
            {
                var last = PresetsList.ItemContainerGenerator
                    .ContainerFromIndex(count - 1) as FrameworkElement;
                if (last == null) return -1;
                return last.TranslatePoint(new Point(0, last.ActualHeight), PresetsList).Y;
            }

            if (index == 0)
            {
                var first = PresetsList.ItemContainerGenerator
                    .ContainerFromIndex(0) as FrameworkElement;
                if (first == null) return -1;
                return first.TranslatePoint(new Point(), PresetsList).Y;
            }

            var above = PresetsList.ItemContainerGenerator
                .ContainerFromIndex(index - 1) as FrameworkElement;
            var below = PresetsList.ItemContainerGenerator
                .ContainerFromIndex(index) as FrameworkElement;
            if (above == null || below == null) return -1;

            double bottomOfAbove = above.TranslatePoint(
                new Point(0, above.ActualHeight), PresetsList).Y;
            double topOfBelow = below.TranslatePoint(
                new Point(), PresetsList).Y;

            return (bottomOfAbove + topOfBelow) / 2.0;
        }
    }
}
