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
    public partial class ManageContentScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        private RandomCollection? _selectedCollection;
        private RandomCategory?   _selectedCategory;

        // ── Drag state ───────────────────────────────────────────────────────
        private enum DragMode { None, Category, Option }
        private DragMode           _dragMode  = DragMode.None;
        private Point              _dragStartScreen;
        private bool               _dragActive;
        private CategoryViewModel? _dragCatSource;
        private OptionViewModel?   _dragOptSource;
        private int                _dropIndex = -1;
        private Point              _lastDragPt;

        // Stored cross-container drop targets — set during mouse move, used at commit
        private CollectionViewModel? _pendingCollectionTarget;
        private CategoryViewModel?   _pendingCategoryTarget;

        // Canvas overlay elements
        private Border? _ghostEl;
        private Line?   _lineEl;

        // Currently highlighted target row
        private Border? _highlightedBorder;
        private Brush?  _highlightedOrigBrush;
        private Brush?  _highlightedOrigBg;

        public ManageContentScreen() => InitializeComponent();

        public void Refresh()
        {
            RestoreSidebarWidth();
            RefreshCollections();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_dragActive) CancelDrag();
                else Main.NavigateToMain();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void RestoreSidebarWidth()
        {
            double w = DataService.Settings.SidebarWidth;
            if (w >= 280 && w <= 400)
                SidebarColumn.Width = new GridLength(w);
        }

        private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
            => DataService.SaveSidebarWidth(SidebarColumn.ActualWidth);

        // ════════════════════════════════════════════════════════════════════
        // COLLECTIONS
        // ════════════════════════════════════════════════════════════════════

        private void RefreshCollections()
        {
            var enabled = DataService.Categories.Collections
                .Where(c => c.IsEnabled).ToList();

            if (enabled.Count == 0)
            {
                CollectionList.ItemsSource      = null;
                CollectionsEmptyText.Visibility = Visibility.Visible;
                _selectedCollection             = null;
                _selectedCategory               = null;
            }
            else
            {
                CollectionsEmptyText.Visibility = Visibility.Collapsed;
                if (_selectedCollection == null || !enabled.Contains(_selectedCollection))
                    _selectedCollection = enabled[0];
                CollectionList.ItemsSource = enabled
                    .Select(c => new CollectionViewModel(c, c == _selectedCollection))
                    .ToList();
            }
            RefreshCategoriesZone();
        }

        private void CollectionRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CollectionViewModel vm)
            {
                _selectedCollection = vm.Model;
                _selectedCategory   = null;
                RefreshCollections();
            }
        }

        private void ManageCollectionsButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new CollectionsManagementScreen();
            screen.OnClosed = () =>
            {
                if (_selectedCollection != null &&
                    !DataService.Categories.Collections.Contains(_selectedCollection))
                    _selectedCollection = null;
                RefreshCollections();
                Main.RefreshGenerateButtonState();
            };
            Main.ShowOverlay(screen);
        }

        // ════════════════════════════════════════════════════════════════════
        // CATEGORIES
        // ════════════════════════════════════════════════════════════════════

        private void RefreshCategoriesZone()
        {
            bool has = _selectedCollection != null;
            CategoriesZoneLabel.Text = has
                ? $"CATEGORIES — {_selectedCollection!.Name.ToUpper()}" : "CATEGORIES";

            AddCategoryButton.IsEnabled = has &&
                DataService.Categories.Collections.Any(c => c.IsEnabled);
            AddCategoryButtonText.Text = has
                ? $"＋ Add Category to {_selectedCollection!.Name}" : "＋ Add Category";

            CategoryBulkControls.Visibility = has && _selectedCollection!.Categories.Count > 0
                ? Visibility.Visible : Visibility.Collapsed;

            if (!has)
            {
                CategoryList.ItemsSource       = null;
                CategoriesEmptyText.Visibility = Visibility.Collapsed;
            }
            else if (_selectedCollection!.Categories.Count == 0)
            {
                CategoryList.ItemsSource       = null;
                CategoriesEmptyText.Text       = "No categories yet. Add one above.";
                CategoriesEmptyText.Visibility = Visibility.Visible;
            }
            else
            {
                CategoriesEmptyText.Visibility = Visibility.Collapsed;
                CategoryList.ItemsSource = _selectedCollection!.Categories
                    .Select(c => new CategoryViewModel(c, _selectedCollection, c == _selectedCategory))
                    .ToList();
            }
            RefreshOptionsPanel();
        }

        private void CategoryRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                _selectedCategory = (_selectedCategory == vm.Model) ? null : vm.Model;
                RefreshCategoriesZone();
            }
        }

        private void CategoryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshCategoriesZone();
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCollection == null) return;
            var col = _selectedCollection;
            var dlg = new InputDialog("Add Category",
                $"Enter name for new category in {col.Name}:", "",
                name => NameValidator.IsDuplicateCategory(name, col.Categories)) { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                var cat = new RandomCategory { Name = dlg.Result };
                col.Categories.Add(cat);
                _selectedCategory = cat;
                DataService.SaveCategories();
                RefreshCategoriesZone();
            }
        }

        private void RenameCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm &&
                _selectedCollection != null)
            {
                var dlg = new InputDialog("Rename Category", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicateCategory(
                        name, _selectedCollection.Categories, vm.Name)) { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    DataService.SaveCategories();
                    RefreshCategoriesZone();
                }
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm &&
                _selectedCollection != null)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    string msg = vm.Model.Options.Count > 0
                        ? $"Delete \"{vm.Name}\" and all {vm.Model.Options.Count} option(s)? This cannot be undone."
                        : $"Delete category \"{vm.Name}\"? This cannot be undone.";
                    if (new ConfirmDialog("Confirm Delete", msg) { Owner = Main }
                        .ShowDialog() != true) return;
                }
                _selectedCollection.Categories.Remove(vm.Model);
                if (_selectedCategory == vm.Model) _selectedCategory = null;
                DataService.SaveCategories();
                RefreshCategoriesZone();
                Main.RefreshGenerateButtonState();
            }
        }

        private void BulkEnableCategories_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCollection == null) return;
            var dlg = new ConfirmDialog("Enable All Categories",
                $"Enable all {_selectedCollection.Categories.Count} categories in {_selectedCollection.Name}?",
                "Cancel", "Enable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            foreach (var cat in _selectedCollection.Categories) cat.IsEnabled = true;
            DataService.SaveCategories();
            RefreshCategoriesZone();
            Main.RefreshGenerateButtonState();
        }

        private void BulkDisableCategories_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCollection == null) return;
            var dlg = new ConfirmDialog("Disable All Categories",
                $"Disable all {_selectedCollection.Categories.Count} categories in {_selectedCollection.Name}?",
                "Cancel", "Disable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            foreach (var cat in _selectedCollection.Categories) cat.IsEnabled = false;
            DataService.SaveCategories();
            RefreshCategoriesZone();
            Main.RefreshGenerateButtonState();
        }

        // ════════════════════════════════════════════════════════════════════
        // OPTIONS
        // ════════════════════════════════════════════════════════════════════

        private void RefreshOptionsPanel()
        {
            if (_selectedCategory == null)
            {
                RightPanelTitle.Text           = "Select a category";
                AddOptionButton.Visibility     = Visibility.Collapsed;
                OptionBulkControls.Visibility  = Visibility.Collapsed;
                OptionsScrollViewer.Visibility = Visibility.Collapsed;
                OptionsEmptyText.Visibility    = Visibility.Collapsed;
                SelectCategoryText.Visibility  = Visibility.Visible;
                return;
            }
            SelectCategoryText.Visibility = Visibility.Collapsed;
            RightPanelTitle.Text          = $"{_selectedCategory.Name} — Options";
            AddOptionButton.Visibility    = Visibility.Visible;

            if (_selectedCategory.Options.Count == 0)
            {
                OptionsScrollViewer.Visibility = Visibility.Collapsed;
                OptionsEmptyText.Visibility    = Visibility.Visible;
                OptionBulkControls.Visibility  = Visibility.Collapsed;
            }
            else
            {
                OptionsEmptyText.Visibility    = Visibility.Collapsed;
                OptionsScrollViewer.Visibility = Visibility.Visible;
                OptionBulkControls.Visibility  = Visibility.Visible;
                OptionsList.ItemsSource = _selectedCategory.Options
                    .Select(o => new OptionViewModel(o, _selectedCategory)).ToList();
            }
        }

        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory == null) return;
            var cat = _selectedCategory;
            var dlg = new InputDialog("Add Option", "Enter option name:", "",
                name => NameValidator.IsDuplicateOption(name, cat.Options)) { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                cat.Options.Add(new RandomOption { Name = dlg.Result });
                DataService.SaveCategories();
                RefreshOptionsPanel();
                Main.RefreshGenerateButtonState();
            }
        }

        private void EditOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm &&
                _selectedCategory != null)
            {
                var dlg = new InputDialog("Edit Option", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicateOption(
                        name, _selectedCategory.Options, vm.Name)) { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    DataService.SaveCategories();
                    RefreshOptionsPanel();
                }
            }
        }

        private void DeleteOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm &&
                _selectedCategory != null)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    if (new ConfirmDialog("Confirm Delete",
                        $"Delete option \"{vm.Name}\"? This cannot be undone.")
                    { Owner = Main }.ShowDialog() != true) return;
                }
                _selectedCategory.Options.Remove(vm.Model);
                DataService.SaveCategories();
                RefreshOptionsPanel();
                Main.RefreshGenerateButtonState();
            }
        }

        private void OptionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshOptionsPanel();
        }

        private void WeightBadge_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                vm.Model.Weight = vm.Model.Weight switch
                {
                    WeightTier.Normal => WeightTier.Low,
                    WeightTier.Low    => WeightTier.Rare,
                    WeightTier.Rare   => WeightTier.Normal,
                    _ => WeightTier.Normal
                };
                DataService.SaveCategories();
                RefreshOptionsPanel();
            }
        }

        private void BulkEnableOptions_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategory == null) return;
            var dlg = new ConfirmDialog("Enable All Options",
                $"Enable all {_selectedCategory.Options.Count} options in {_selectedCategory.Name}?",
                "Cancel", "Enable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            foreach (var opt in _selectedCategory.Options) opt.IsEnabled = true;
            DataService.SaveCategories();
            RefreshOptionsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void BulkDisableOptions_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategory == null) return;
            var dlg = new ConfirmDialog("Disable All Options",
                $"Disable all {_selectedCategory.Options.Count} options in {_selectedCategory.Name}?",
                "Cancel", "Disable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            foreach (var opt in _selectedCategory.Options) opt.IsEnabled = false;
            DataService.SaveCategories();
            RefreshOptionsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void PresetsButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new PresetsScreen();
            screen.OnClosed = () => Main.RefreshMainScreen();
            Main.ShowOverlay(screen);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Main.RefreshMainScreen();
            Main.NavigateToMain();
        }

        // ════════════════════════════════════════════════════════════════════
        // DRAG AND DROP — Canvas overlay approach
        //
        // All coordinates are in ManageContentScreen's own space using
        // e.GetPosition(this). The Canvas overlay sits on top at the same
        // coordinate level so no transform math is needed.
        //
        // Same-list reorder: blue insertion line between items
        // Cross-container:   target row highlights, no line
        // Invalid zone:      ghost border turns red
        // ════════════════════════════════════════════════════════════════════

        // ── Category drag initiation ─────────────────────────────────────────

        private void CategoryList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartScreen = PointToScreen(e.GetPosition(this));
            _dragCatSource   = null;
            _dragMode        = DragMode.None;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                _dragCatSource = vm;
                _dragMode      = DragMode.Category;
            }
        }

        private void CategoryList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||
                _dragCatSource == null || _dragActive) return;
            var screen = PointToScreen(e.GetPosition(this));
            if (Math.Abs(screen.X - _dragStartScreen.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(screen.Y - _dragStartScreen.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragCatSource.Name);
        }

        private void CategoryList_Drop(object sender, DragEventArgs e) { }

        // ── Option drag initiation ───────────────────────────────────────────

        private void OptionsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartScreen = PointToScreen(e.GetPosition(this));
            _dragOptSource   = null;
            _dragMode        = DragMode.None;
        }

        private void OptionDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                _dragOptSource = vm;
                _dragMode      = DragMode.Option;
            }
        }

        private void OptionsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||
                _dragOptSource == null || _dragActive) return;
            var screen = PointToScreen(e.GetPosition(this));
            if (Math.Abs(screen.X - _dragStartScreen.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(screen.Y - _dragStartScreen.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragOptSource.Name);
        }

        private void OptionsList_Drop(object sender, DragEventArgs e) { }

        // ── Begin drag ───────────────────────────────────────────────────────

        private void BeginDrag(string name)
        {
            _dragActive = true;

            // Ghost — shown on the canvas overlay
            _ghostEl = new Border
            {
                Background      = (Brush)Application.Current.Resources["BackgroundCardBrush"],
                BorderBrush     = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                BorderThickness = new Thickness(1.5),
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(8, 7, 8, 7),
                Opacity         = 0.80,
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
                            FontSize  = 14, Margin = new Thickness(0,0,8,0),
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

            // Insertion line — drawn on canvas
            _lineEl = new Line
            {
                Stroke          = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                StrokeThickness = 2,
                IsHitTestVisible = false
            };

            DragOverlay.Children.Clear();
            DragOverlay.Children.Add(_ghostEl);
            DragOverlay.Children.Add(_lineEl);
            DragOverlay.Visibility = Visibility.Visible;

            // Hook window mouse events
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         += OnDragMouseMove;
                win.MouseLeftButtonUp += OnDragMouseUp;
                win.LostMouseCapture  += OnDragCancelled;
                win.PreviewKeyDown    += OnDragKeyDown;
            }
        }

        // ── Mouse move ───────────────────────────────────────────────────────

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragActive || _ghostEl == null) return;

            var pt = e.GetPosition(this);
            _lastDragPt = pt;

            UpdateGhostPosition(pt);
            UpdateGhostWidth(pt);
            UpdateDragFeedback(pt);
        }

        private void UpdateGhostPosition(Point pt)
        {
            if (_ghostEl == null) return;
            Canvas.SetLeft(_ghostEl, pt.X + 10);
            Canvas.SetTop(_ghostEl,  pt.Y - 12);
        }

        private void UpdateGhostWidth(Point pt)
        {
            // Width is set per-zone inside UpdateDragFeedback
            // This method kept as no-op to avoid breaking the call chain
        }

        private void UpdateDragFeedback(Point pt)
        {
            ClearHighlight();
            HideInsertionLine();
            _pendingCollectionTarget = null;
            _pendingCategoryTarget   = null;

            if (_dragMode == DragMode.Category)
            {
                if (IsOverCollectionZone(pt))
                {
                    var colPt = this.TranslatePoint(pt, CollectionList);
                    var colVm = GetViewModelAtPoint<CollectionViewModel>(CollectionList, colPt);
                    if (colVm != null && colVm.Model != _dragCatSource?.Collection)
                    {
                        SetGhostValid(true);
                        HighlightBorderAtPoint(CollectionList, colPt);
                        _ghostEl!.Width          = CollectionsZone.ActualWidth;
                        _pendingCollectionTarget = colVm;
                    }
                    else SetGhostValid(false);
                    return;
                }

                if (IsOverCategoryScrollViewer(pt))
                {
                    var catPt = this.TranslatePoint(pt, CategoryList);
                    SetGhostValid(true);
                    _ghostEl!.Width = CategoryScrollViewer.ActualWidth;
                    ShowInsertionLine(CategoryList, catPt, pt);
                    return;
                }
            }
            else
            {
                if (IsOverCategoryScrollViewer(pt))
                {
                    var catPt = this.TranslatePoint(pt, CategoryList);
                    var catVm = GetViewModelAtPoint<CategoryViewModel>(CategoryList, catPt);
                    if (catVm != null && catVm.Model != _dragOptSource?.Category)
                    {
                        SetGhostValid(true);
                        HighlightBorderAtPoint(CategoryList, catPt);
                        _ghostEl!.Width        = CategoryScrollViewer.ActualWidth;
                        _pendingCategoryTarget = catVm;
                    }
                    else SetGhostValid(false);
                    return;
                }

                if (IsOverOptionsList(pt))
                {
                    var optPt = this.TranslatePoint(pt, OptionsList);
                    SetGhostValid(true);
                    _ghostEl!.Width = OptionsScrollViewer.ActualWidth;
                    ShowInsertionLine(OptionsList, optPt, pt);
                    return;
                }
            }

            SetGhostValid(false);
        }

        // ── Mouse up ─────────────────────────────────────────────────────────

        private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragActive) return;

            // Hide overlay FIRST so ghost doesn't block VisualTreeHelper hit testing
            DragOverlay.Visibility = Visibility.Collapsed;

            // Use _lastDragPt (from last MouseMove) — more reliable than
            // e.GetPosition(this) at mouse-up time
            CommitDrop(_lastDragPt);
            EndDrag();
        }

        private void CommitDrop(Point pt)
        {
            if (_dragMode == DragMode.Category)
                CommitCategoryDrop(pt);
            else if (_dragMode == DragMode.Option)
                CommitOptionDrop(pt);
        }

        private void CommitCategoryDrop(Point pt)
        {
            if (_dragCatSource == null) return;

            // Cross-collection drop — use target stored during last mouse-move
            if (_pendingCollectionTarget != null &&
                _pendingCollectionTarget.Model != _dragCatSource.Collection &&
                DataService.Categories.Collections.Contains(_pendingCollectionTarget.Model))
            {
                _dragCatSource.Collection.Categories.Remove(_dragCatSource.Model);
                _pendingCollectionTarget.Model.Categories.Add(_dragCatSource.Model);
                if (_selectedCollection == _pendingCollectionTarget.Model)
                    _selectedCategory = _dragCatSource.Model;
                DataService.SaveCategories();
                RefreshCollections();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Same-collection reorder
            if (_selectedCollection == null) return;
            var cats    = _selectedCollection.Categories;
            int fromIdx = cats.IndexOf(_dragCatSource.Model);
            if (fromIdx < 0 || _dropIndex < 0 || fromIdx == _dropIndex) return;
            int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
            cats.RemoveAt(fromIdx);
            cats.Insert(Math.Min(toIdx, cats.Count), _dragCatSource.Model);
            DataService.SaveCategories();
            RefreshCategoriesZone();
        }

        private void CommitOptionDrop(Point pt)
        {
            if (_dragOptSource == null) return;

            // Cross-category drop — use stored target from last mouse-move
            if (_pendingCategoryTarget != null &&
                _pendingCategoryTarget.Model != _dragOptSource.Category &&
                _selectedCollection != null &&
                _selectedCollection.Categories.Contains(_pendingCategoryTarget.Model))
            {
                _dragOptSource.Category.Options.Remove(_dragOptSource.Model);
                _pendingCategoryTarget.Model.Options.Add(_dragOptSource.Model);
                DataService.SaveCategories();
                RefreshOptionsPanel();
                RefreshCategoriesZone();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Same-category reorder
            var opts    = _dragOptSource.Category.Options;
            int fromIdx = opts.IndexOf(_dragOptSource.Model);
            if (fromIdx < 0 || _dropIndex < 0 || fromIdx == _dropIndex) return;
            int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
            opts.RemoveAt(fromIdx);
            opts.Insert(Math.Min(toIdx, opts.Count), _dragOptSource.Model);
            DataService.SaveCategories();
            RefreshOptionsPanel();
        }

        private void OnDragCancelled(object sender, MouseEventArgs e) => CancelDrag();

        private void OnDragKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) CancelDrag();
        }

        private void CancelDrag() => EndDrag();

        private void EndDrag()
        {
            _dragActive              = false;
            _dragMode                = DragMode.None;
            _dragCatSource           = null;
            _dragOptSource           = null;
            _dropIndex               = -1;
            _pendingCollectionTarget = null;
            _pendingCategoryTarget   = null;

            ClearHighlight();
            DragOverlay.Children.Clear();
            DragOverlay.Visibility = Visibility.Collapsed;
            _ghostEl = null;
            _lineEl  = null;

            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         -= OnDragMouseMove;
                win.MouseLeftButtonUp -= OnDragMouseUp;
                win.LostMouseCapture  -= OnDragCancelled;
                win.PreviewKeyDown    -= OnDragKeyDown;
            }
        }

        // ── Visual feedback ──────────────────────────────────────────────────

        private void SetGhostValid(bool valid)
        {
            if (_ghostEl == null) return;
            _ghostEl.BorderBrush = valid
                ? new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff))
                : new SolidColorBrush(Color.FromRgb(0xe0, 0x50, 0x50));
        }

        /// <summary>
        /// Draws the insertion line on the DragOverlay canvas.
        /// listPt = point in list's local space (for index calculation)
        /// overlayPt = point in ManageContentScreen space (for Canvas positioning)
        /// </summary>
        private void ShowInsertionLine(ItemsControl list, Point listPt, Point overlayPt)
        {
            if (_lineEl == null) return;

            _dropIndex = GetInsertionIndex(list, listPt);
            double lineY = GetInsertionY(list, _dropIndex);
            if (lineY < 0) { HideInsertionLine(); return; }

            // lineY is in the ItemsControl's coordinate space.
            // TranslatePoint from ItemsControl to ManageContentScreen (this)
            // correctly handles the ScrollViewer offset.
            var lineInOverlay = list.TranslatePoint(new Point(0, lineY), this);
            var listLeft      = list.TranslatePoint(new Point(0, 0), this);
            double lineWidth  = list.ActualWidth;

            _lineEl.X1 = listLeft.X + 4;
            _lineEl.X2 = listLeft.X + lineWidth - 4;
            _lineEl.Y1 = lineInOverlay.Y;
            _lineEl.Y2 = lineInOverlay.Y;
            _lineEl.Visibility = Visibility.Visible;
        }

        private void HideInsertionLine()
        {
            if (_lineEl != null) _lineEl.Visibility = Visibility.Collapsed;
            _dropIndex = -1;
        }

        private void HighlightBorderAtPoint(ItemsControl list, Point listPt)
        {
            var b = FindRowBorderAtPoint(list, listPt);
            if (b == null || b == _highlightedBorder) return;
            ClearHighlight();
            _highlightedBorder    = b;
            _highlightedOrigBrush = b.BorderBrush;
            _highlightedOrigBg    = b.Background;
            b.BorderBrush   = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff));
            b.BorderThickness = new Thickness(1.5);
            b.Background    = new SolidColorBrush(Color.FromArgb(30, 0x0a, 0x84, 0xff));
        }

        private void ClearHighlight()
        {
            if (_highlightedBorder == null) return;
            _highlightedBorder.BorderBrush    = _highlightedOrigBrush;
            _highlightedBorder.Background     = _highlightedOrigBg;
            _highlightedBorder.BorderThickness = new Thickness(1);
            _highlightedBorder    = null;
            _highlightedOrigBrush = null;
            _highlightedOrigBg    = null;
        }

        // ── Coordinate helpers ────────────────────────────────────────────────

        /// <summary>
        /// Translates a point from ManageContentScreen space to an element's local space.
        /// For elements inside a ScrollViewer, accounts for scroll offset.
        /// </summary>
        private Point TranslateToElement(Point ptInThis, UIElement el)
            => this.TranslatePoint(ptInThis, el);

        /// <summary>
        /// Checks if a point (in ManageContentScreen space) is over a named zone element.
        /// Uses the zone element rather than ItemsControl since zones have reliable heights.
        /// </summary>
        private bool IsOverCollectionZone(Point pt)
        {
            var p = this.TranslatePoint(pt, CollectionsZone);
return p.X >= 0 && p.Y >= 0 &&
                   p.X <= CollectionsZone.ActualWidth &&
                   p.Y <= CollectionsZone.ActualHeight;
        }

        private bool IsOverCategoryScrollViewer(Point pt)
        {
            var p = this.TranslatePoint(pt, CategoryScrollViewer);
return p.X >= 0 && p.Y >= 0 &&
                   p.X <= CategoryScrollViewer.ActualWidth &&
                   p.Y <= CategoryScrollViewer.ActualHeight;
        }

        private bool IsOverOptionsList(Point pt)
        {
            var p = this.TranslatePoint(pt, OptionsScrollViewer);
            return p.X >= 0 && p.Y >= 0 &&
                   p.X <= OptionsScrollViewer.ActualWidth &&
                   p.Y <= OptionsScrollViewer.ActualHeight;
        }

        private static bool IsOverElement(Point pt, FrameworkElement el)
            => pt.X >= 0 && pt.Y >= 0 && pt.X <= el.ActualWidth && pt.Y <= el.ActualHeight;

        private static int GetInsertionIndex(ItemsControl list, Point pt)
        {
            int count = list.Items.Count;
            if (count == 0) return 0;
            for (int i = 0; i < count; i++)
            {
                var c = list.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (c == null) continue;
                var top    = c.TranslatePoint(new Point(), list).Y;
                var bottom = top + c.ActualHeight;
                if (pt.Y < top + (bottom - top) / 2.0) return i;
            }
            return count;
        }

        private static double GetInsertionY(ItemsControl list, int index)
        {
            int count = list.Items.Count;
            if (count == 0) return -1;

            if (index >= count)
            {
                var last = list.ItemContainerGenerator
                    .ContainerFromIndex(count - 1) as FrameworkElement;
                if (last == null) return -1;
                return last.TranslatePoint(new Point(0, last.ActualHeight), list).Y;
            }
            if (index == 0)
            {
                var first = list.ItemContainerGenerator
                    .ContainerFromIndex(0) as FrameworkElement;
                if (first == null) return -1;
                return first.TranslatePoint(new Point(), list).Y;
            }

            var above = list.ItemContainerGenerator
                .ContainerFromIndex(index - 1) as FrameworkElement;
            var below = list.ItemContainerGenerator
                .ContainerFromIndex(index) as FrameworkElement;
            if (above == null || below == null) return -1;

            double bot  = above.TranslatePoint(new Point(0, above.ActualHeight), list).Y;
            double top2 = below.TranslatePoint(new Point(), list).Y;
            return (bot + top2) / 2.0;
        }

        /// <summary>
        /// Finds a ViewModel of type T at a point in an ItemsControl by checking
        /// item container bounds directly — avoids VisualTreeHelper.HitTest issues.
        /// pt is in the ItemsControl's own coordinate space.
        /// </summary>
        private static T? GetViewModelAtPoint<T>(ItemsControl list, Point pt) where T : class
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                // ContainerFromIndex gives us the ContentPresenter wrapper
                var container = list.ItemContainerGenerator
                    .ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;

                // Walk into the container to find the actual content element
                // which may have a non-zero size even if the wrapper reports 0
                var content = FindFirstChild<FrameworkElement>(container)
                              ?? container;

                var origin = content.TranslatePoint(new Point(), list);
                double h = content.ActualHeight > 0
                    ? content.ActualHeight : container.ActualHeight;
                double w = content.ActualWidth > 0
                    ? content.ActualWidth : container.ActualWidth;

                if (w <= 0 || h <= 0) continue;

                var bounds = new Rect(origin.X, origin.Y, w, h);
                if (bounds.Contains(pt))
                {
                    var vm = FindTagInSubtree<T>(container);
                    if (vm != null) return vm;
                }
            }
            return null;
        }

        private static FrameworkElement? FindFirstChild<T2>(DependencyObject root)
            where T2 : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T2 fe && fe.ActualHeight > 0) return fe;
                var found = FindFirstChild<T2>(child);
                if (found != null) return found;
            }
            return null;
        }

        private static T? FindTagInSubtree<T>(DependencyObject root) where T : class
        {
            if (root is FrameworkElement fe && fe.Tag is T vm) return vm;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var found = FindTagInSubtree<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        private static Border? FindRowBorderAtPoint(ItemsControl list, Point pt)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                var container = list.ItemContainerGenerator
                    .ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;

                var origin = container.TranslatePoint(new Point(), list);
                var bounds = new Rect(origin, container.RenderSize);

                if (bounds.Contains(pt))
                {
                    // Find the outermost Border with a Tag in this container
                    return FindTaggedBorder(container);
                }
            }
            return null;
        }

        private static Border? FindTaggedBorder(DependencyObject root)
        {
            if (root is Border b && b.Tag != null) return b;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var found = FindTaggedBorder(VisualTreeHelper.GetChild(root, i));
                if (found != null) return found;
            }
            return null;
        }
    }
}
