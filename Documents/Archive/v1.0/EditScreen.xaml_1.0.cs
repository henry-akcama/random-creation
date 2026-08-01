using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CreatureCrafter
{
    public partial class EditScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        // Currently selected category
        private CreatureCategory? _selectedModel;

        // ── Drag state for categories ────────────────────────────────────────
        private Point _catDragStart;
        private CategoryViewModel? _catDragSource;
        private bool _catDragging;

        // ── Drag state for options ───────────────────────────────────────────
        private Point _optDragStart;
        private OptionViewModel? _optDragSource;
        private bool _optDragging;

        public EditScreen() => InitializeComponent();

        // ── Refresh helpers ──────────────────────────────────────────────────

        public void RefreshCategories()
        {
            var vms = Main.Data.Categories
                .Select(c => new CategoryViewModel(c, c == _selectedModel))
                .ToList();

            CategoryList.ItemsSource = vms;
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            if (_selectedModel == null)
            {
                RightPanelTitle.Text = "Select a category";
                AddOptionButton.Visibility = Visibility.Collapsed;
                OptionsList.ItemsSource = null;
                return;
            }

            RightPanelTitle.Text = $"{_selectedModel.Name} — Options";
            AddOptionButton.Visibility = Visibility.Visible;
            OptionsList.ItemsSource = _selectedModel.Options
                .Select(o => new OptionViewModel(o, _selectedModel))
                .ToList();
        }

        // ── Category interactions ────────────────────────────────────────────

        private void CategoryRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                _selectedModel = (_selectedModel == vm.Model) ? null : vm.Model;
                RefreshCategories();
            }
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
            => Main.SaveData();

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Add Category", "Enter category name:", "") { Owner = Main };
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
            {
                var cat = new CreatureCategory { Name = dlg.Result.Trim() };
                Main.Data.Categories.Add(cat);
                _selectedModel = cat;
                Main.SaveData();
                RefreshCategories();
            }
        }

        private void RenameCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                var dlg = new InputDialog("Rename Category", "Enter new name:", vm.Name) { Owner = Main };
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
                {
                    vm.Model.Name = dlg.Result.Trim();
                    Main.SaveData();
                    RefreshCategories();
                }
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                string msg = vm.Model.Options.Count > 0
                    ? $"Delete \"{vm.Name}\" and all {vm.Model.Options.Count} option(s) inside? This cannot be undone."
                    : $"Delete category \"{vm.Name}\"? This cannot be undone.";

                if (new ConfirmDialog("Confirm Delete", msg) { Owner = Main }.ShowDialog() == true)
                {
                    Main.Data.Categories.Remove(vm.Model);
                    if (_selectedModel == vm.Model) _selectedModel = null;
                    Main.SaveData();
                    RefreshCategories();
                }
            }
        }

        // ── Option interactions ──────────────────────────────────────────────

        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModel == null) return;
            var dlg = new InputDialog("Add Option", "Enter option name:", "") { Owner = Main };
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
            {
                _selectedModel.Options.Add(new CreatureOption { Name = dlg.Result.Trim() });
                Main.SaveData();
                RefreshOptions();
            }
        }

        private void EditOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                var dlg = new InputDialog("Edit Option", "Enter new name:", vm.Name) { Owner = Main };
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Result))
                {
                    vm.Model.Name = dlg.Result.Trim();
                    Main.SaveData();
                    RefreshOptions();
                }
            }
        }

        private void DeleteOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                if (new ConfirmDialog("Confirm Delete", $"Delete option \"{vm.Name}\"? This cannot be undone.") { Owner = Main }.ShowDialog() == true)
                {
                    vm.Category.Options.Remove(vm.Model);
                    Main.SaveData();
                    RefreshOptions();
                }
            }
        }

        // Cycle weight: Normal → Low → Rare → Normal
        private void WeightBadge_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                vm.Model.Weight = vm.Model.Weight switch
                {
                    WeightTier.Normal => WeightTier.Low,
                    WeightTier.Low    => WeightTier.Rare,
                    WeightTier.Rare   => WeightTier.Normal,
                    _                 => WeightTier.Normal
                };
                Main.SaveData();
                RefreshOptions();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => Main.NavigateToMain();

        // ── Category drag-and-drop ───────────────────────────────────────────

        private void CategoryList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _catDragStart = e.GetPosition(null);
            _catDragSource = null;
            _catDragging = false;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
                _catDragSource = vm;
        }

        private void CategoryList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _catDragSource == null) return;
            var pos = e.GetPosition(null);
            if (!_catDragging &&
                (System.Math.Abs(pos.X - _catDragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 System.Math.Abs(pos.Y - _catDragStart.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                _catDragging = true;
                DragDrop.DoDragDrop(CategoryList, _catDragSource, DragDropEffects.Move);
                _catDragging = false;
                _catDragSource = null;
            }
        }

        private void CategoryList_Drop(object sender, DragEventArgs e)
        {
            if (_catDragSource == null) return;

            var target = GetCategoryViewModelAtPoint(e.GetPosition(CategoryList));
            if (target == null || target.Model == _catDragSource.Model) return;

            var cats = Main.Data.Categories;
            int fromIdx = cats.IndexOf(_catDragSource.Model);
            int toIdx   = cats.IndexOf(target.Model);
            if (fromIdx < 0 || toIdx < 0) return;

            cats.RemoveAt(fromIdx);
            cats.Insert(toIdx, _catDragSource.Model);

            Main.SaveData();
            RefreshCategories();
        }

        private CategoryViewModel? GetCategoryViewModelAtPoint(Point pt)
        {
            var hit = VisualTreeHelper.HitTest(CategoryList, pt);
            if (hit == null) return null;
            DependencyObject? el = hit.VisualHit;
            while (el != null)
            {
                if (el is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
                    return vm;
                el = VisualTreeHelper.GetParent(el);
            }
            return null;
        }

        // ── Option drag-and-drop ─────────────────────────────────────────────

        private void OptionsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _optDragStart = e.GetPosition(null);
            _optDragSource = null;
            _optDragging = false;
        }

        private void OptionDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
                _optDragSource = vm;
        }

        private void OptionsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _optDragSource == null) return;
            var pos = e.GetPosition(null);
            if (!_optDragging &&
                (System.Math.Abs(pos.X - _optDragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 System.Math.Abs(pos.Y - _optDragStart.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                _optDragging = true;
                DragDrop.DoDragDrop(OptionsList, _optDragSource, DragDropEffects.Move);
                _optDragging = false;
                _optDragSource = null;
            }
        }

        private void OptionsList_Drop(object sender, DragEventArgs e)
        {
            if (_optDragSource == null || _selectedModel == null) return;

            var target = GetOptionViewModelAtPoint(e.GetPosition(OptionsList));
            if (target == null || target.Model == _optDragSource.Model) return;

            var opts = _selectedModel.Options;
            int fromIdx = opts.IndexOf(_optDragSource.Model);
            int toIdx   = opts.IndexOf(target.Model);
            if (fromIdx < 0 || toIdx < 0) return;

            opts.RemoveAt(fromIdx);
            opts.Insert(toIdx, _optDragSource.Model);

            Main.SaveData();
            RefreshOptions();
        }

        private OptionViewModel? GetOptionViewModelAtPoint(Point pt)
        {
            var hit = VisualTreeHelper.HitTest(OptionsList, pt);
            if (hit == null) return null;
            DependencyObject? el = hit.VisualHit;
            while (el != null)
            {
                if (el is FrameworkElement fe && fe.Tag is OptionViewModel vm)
                    return vm;
                el = VisualTreeHelper.GetParent(el);
            }
            return null;
        }
    }
}