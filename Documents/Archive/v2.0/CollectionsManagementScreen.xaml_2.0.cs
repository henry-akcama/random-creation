using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RandomCreation
{
    public partial class CollectionsManagementScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);
        public Action? OnClosed { get; set; }

        // ── Drag state ───────────────────────────────────────────────────────
        private Point _dragStartPos;
        private CollectionManagementViewModel? _dragSource;
        private bool   _dragActive;
        private int    _dropIndex = -1;
        private Popup? _ghostPopup;
        private Popup? _linePopup;
        private Border? _ghostBorder;

        public CollectionsManagementScreen() => InitializeComponent();

        public void Refresh()
        {
            var collections = DataService.Categories.Collections;
            if (collections.Count == 0)
            {
                CollectionsList.ItemsSource = null;
                EmptyText.Visibility        = Visibility.Visible;
                return;
            }
            EmptyText.Visibility        = Visibility.Collapsed;
            CollectionsList.ItemsSource = collections
                .Select(c => new CollectionManagementViewModel(c))
                .ToList();
        }

        private void AddCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Add Collection", "Enter collection name:", "",
                name => NameValidator.IsDuplicateCollection(
                    name, DataService.Categories.Collections))
            { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                DataService.Categories.Collections.Add(new RandomCollection
                    { Name = dlg.Result, IsEnabled = true });
                DataService.SaveCategories();
                Refresh();
                Main.RefreshGenerateButtonState();
            }
        }

        private void RenameCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.Tag is CollectionManagementViewModel vm)
            {
                var dlg = new InputDialog("Rename Collection", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicateCollection(
                        name, DataService.Categories.Collections, vm.Name))
                { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    DataService.SaveCategories();
                    Refresh();
                }
            }
        }

        private void DeleteCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.Tag is CollectionManagementViewModel vm)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    int catCount = vm.Model.Categories.Count;
                    int optCount = vm.OptionCount;
                    string msg = catCount > 0
                        ? $"Delete \"{vm.Name}\" and all {catCount} categories and {optCount} options inside? This cannot be undone."
                        : $"Delete collection \"{vm.Name}\"? This cannot be undone.";
                    if (new ConfirmDialog("Confirm Delete", msg)
                        { Owner = Main }.ShowDialog() != true) return;
                }
                DataService.Categories.Collections.Remove(vm.Model);
                DataService.SaveCategories();
                Refresh();
                Main.RefreshGenerateButtonState();
            }
        }

        private void CollectionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            Main.RefreshMainScreen();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnClosed?.Invoke();
            Main.HideOverlay();
        }

        // ════════════════════════════════════════════════════════════════════
        // DRAG AND DROP
        // ════════════════════════════════════════════════════════════════════

        private void CollectionsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPos = e.GetPosition(null);
            _dragSource   = null;
            _dragActive   = false;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe &&
                fe.Tag is CollectionManagementViewModel vm)
                _dragSource = vm;
        }

        private void CollectionsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||
                _dragSource == null || _dragActive) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pos.Y - _dragStartPos.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragSource.Name);
        }

        private void CollectionsList_Drop(object sender, DragEventArgs e) { }

        private void BeginDrag(string name)
        {
            _dragActive = true;

            double scale = FontScaleHelper.GetScale(DataService.Settings.FontSize);
            double width = CollectionsList.ActualWidth * scale;

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
                Child              = _ghostBorder,
                IsOpen             = false,
                AllowsTransparency = true,
                Placement          = PlacementMode.AbsolutePoint,
                IsHitTestVisible   = false
            };

            _linePopup = new Popup
            {
                Child              = BuildLineCanvas(width),
                IsOpen             = false,
                AllowsTransparency = true,
                Placement          = PlacementMode.AbsolutePoint,
                IsHitTestVisible   = false
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
            var line = new Line
            {
                X1 = 4, X2 = Math.Max(width - 4, 6), Y1 = 5, Y2 = 5,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                StrokeThickness = 2
            };
            var dotL = new Ellipse { Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)) };
            var dotR = new Ellipse { Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)) };
            Canvas.SetLeft(dotL, 0);              Canvas.SetTop(dotL, 2);
            Canvas.SetLeft(dotR, Math.Max(width - 6, 0)); Canvas.SetTop(dotR, 2);
            canvas.Children.Add(line);
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

            var listPos = Mouse.GetPosition(CollectionsList);
            bool isOver = listPos.X >= 0 && listPos.Y >= 0 &&
                          listPos.X <= CollectionsList.ActualWidth &&
                          listPos.Y <= CollectionsList.ActualHeight;

            if (isOver)
            {
                SetGhostValid(true);
                _dropIndex = GetInsertionIndex(listPos);
                double lineY = GetInsertionY(_dropIndex);
                if (lineY >= 0 && _linePopup != null)
                {
                    var screenPt = CollectionsList.PointToScreen(new Point(0, lineY));
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

            var listPos = e.GetPosition(CollectionsList);
            bool isOver = listPos.X >= 0 && listPos.Y >= 0 &&
                          listPos.X <= CollectionsList.ActualWidth &&
                          listPos.Y <= CollectionsList.ActualHeight;

            if (isOver && _dropIndex >= 0)
            {
                var cols    = DataService.Categories.Collections;
                int fromIdx = cols.IndexOf(_dragSource.Model);
                int toIdx   = _dropIndex;
                if (fromIdx >= 0 && toIdx >= 0 && fromIdx != toIdx)
                {
                    if (toIdx > fromIdx) toIdx--;
                    cols.RemoveAt(fromIdx);
                    cols.Insert(Math.Min(toIdx, cols.Count), _dragSource.Model);
                    DataService.SaveCategories();
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
            int count = CollectionsList.Items.Count;
            if (count == 0) return 0;
            for (int i = 0; i < count; i++)
            {
                var c = CollectionsList.ItemContainerGenerator
                    .ContainerFromIndex(i) as FrameworkElement;
                if (c == null) continue;
                var bounds = new Rect(c.TranslatePoint(new Point(), CollectionsList), c.RenderSize);
                if (pt.Y < bounds.Top + bounds.Height / 2) return i;
            }
            return count;
        }

        private double GetInsertionY(int index)
        {
            int count = CollectionsList.Items.Count;
            if (count == 0) return -1;

            if (index >= count)
            {
                var last = CollectionsList.ItemContainerGenerator
                    .ContainerFromIndex(count - 1) as FrameworkElement;
                if (last == null) return -1;
                return last.TranslatePoint(new Point(0, last.ActualHeight), CollectionsList).Y;
            }

            if (index == 0)
            {
                var first = CollectionsList.ItemContainerGenerator
                    .ContainerFromIndex(0) as FrameworkElement;
                if (first == null) return -1;
                return first.TranslatePoint(new Point(), CollectionsList).Y;
            }

            var above = CollectionsList.ItemContainerGenerator
                .ContainerFromIndex(index - 1) as FrameworkElement;
            var below = CollectionsList.ItemContainerGenerator
                .ContainerFromIndex(index) as FrameworkElement;
            if (above == null || below == null) return -1;

            double bottomOfAbove = above.TranslatePoint(
                new Point(0, above.ActualHeight), CollectionsList).Y;
            double topOfBelow = below.TranslatePoint(
                new Point(), CollectionsList).Y;

            return (bottomOfAbove + topOfBelow) / 2.0;
        }
    }
}
