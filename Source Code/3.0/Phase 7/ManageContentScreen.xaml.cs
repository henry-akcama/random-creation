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
    public partial class ManageContentScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        // ── Selection state ──────────────────────────────────────────────────
        private Collection?              _selectedCollection;
        private HashSet<CategoryGroup>   _expandedGroups   = new();
        private CategoryGroup?           _selectedGroup;           // primary (detail panel)
        private HashSet<CategoryGroup>   _selectedGroups   = new(); // multi-select
        private Category?                _selectedCategory;
        private HashSet<Option>          _selectedOptions   = new();
        private HashSet<Category>        _selectedCategories = new();

        // ── Search state ─────────────────────────────────────────────────────
        private string _searchText = "";

        // ── Drag state ───────────────────────────────────────────────────────
        private enum DragMode { None, Collection, Group, Category, Option }
        private DragMode               _dragMode        = DragMode.None;
        private Point                  _dragStartScreen;
        private bool                   _dragActive;
        private bool                   _dragIsCopy;     // Ctrl held during drag
        private CollectionViewModel?   _dragColSource;
        private GroupViewModel?        _dragGrpSource;
        private CategoryViewModel?     _dragCatSource;
        private OptionViewModel?       _dragOptSource;
        private int                    _dropIndex   = -1;
        private Point                  _lastDragPt;

        private GroupViewModel?        _pendingGroupTarget;
        private CategoryViewModel?     _pendingCategoryTarget;

        private Border?                _ghostEl;
        private TextBlock?             _ghostCopyBadge;
        private Line?                  _lineEl;

        private Border?  _highlightedBorder;
        private Brush?   _highlightedOrigBrush;
        private Brush?   _highlightedOrigBg;

        public ManageContentScreen() => InitializeComponent();


        // ════════════════════════════════════════════════════════════════════
        // REFRESH
        // ════════════════════════════════════════════════════════════════════

        public void Refresh()
        {
            ClipboardService.Clear();
            RestoreSidebarWidth();
            RefreshCollections();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // TextBox has focus — don't intercept shortcuts
            if (e.OriginalSource is TextBox) { base.OnKeyDown(e); return; }

            if (_dragActive)
            {
                if (e.Key == Key.Escape) { CancelDrag(); e.Handled = true; }
                base.OnKeyDown(e);
                return;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    if (ClipboardService.Mode == ClipboardService.ClipMode.Cut)
                    {
                        // Cancel cut — clear clipboard and redraw (removes dim)
                        ClipboardService.Clear();
                        RefreshGroupsPanel();
                        e.Handled = true;
                    }
                    else
                    {
                        Main.NavigateToMain();
                        e.Handled = true;
                    }
                    break;

                case Key.Z when Keyboard.Modifiers == ModifierKeys.Control:
                    var desc = UndoService.Undo();
                    if (desc != null)
                        ToastService.Show($"Undone: {desc}");
                    e.Handled = true;
                    break;

                case Key.D when Keyboard.Modifiers == ModifierKeys.Control:
                    HandleDuplicate(); e.Handled = true; break;

                case Key.C when Keyboard.Modifiers == ModifierKeys.Control:
                    HandleCopy(); e.Handled = true; break;

                case Key.X when Keyboard.Modifiers == ModifierKeys.Control:
                    HandleCut(); e.Handled = true; break;

                case Key.V when Keyboard.Modifiers == ModifierKeys.Control:
                    HandlePaste(); e.Handled = true; break;

                case Key.Delete:
                    HandleDelete(); e.Handled = true; break;
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


        private void HandleDuplicate()
        {
            // Duplicate selected options (supports multi-select)
            if (_selectedOptions.Count > 0 && _selectedCategory != null)
            {
                var cat      = _selectedCategory;
                var toClone  = _selectedOptions.ToList();
                var newOpts  = new List<Option>();

                foreach (var src in toClone)
                {
                    string name   = MakeUniqueName(src.Name, n => cat.Options.Any(o => o.Name == n) || newOpts.Any(o => o.Name == n));
                    var newOpt    = new Option { Name = name, Weight = src.Weight, IsEnabled = src.IsEnabled };
                    int idx       = cat.Options.IndexOf(src);
                    cat.Options.Insert(idx + 1, newOpt);
                    newOpts.Add(newOpt);
                }

                UndoService.Push($"Duplicate {toClone.Count} option(s)", () =>
                {
                    foreach (var o in newOpts) cat.Options.Remove(o);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                string label = toClone.Count == 1 ? $"\"{toClone[0].Name}\"" : $"{toClone.Count} options";
                ToastService.Show($"Duplicated {label}");
                _selectedOptions.Clear();
                foreach (var o in newOpts) _selectedOptions.Add(o);
                _selectedOption = newOpts.LastOrDefault();
                RefreshGroupsPanel();
                return;
            }

            // Duplicate selected categories (supports multi-select)
            if (_selectedCategories.Count > 0 && _selectedGroup != null)
            {
                var grp      = _selectedGroup;
                var toClone  = _selectedCategories.ToList();
                var newCats  = new List<Category>();

                foreach (var src in toClone)
                {
                    string name  = MakeUniqueName(src.Name, n => grp.Categories.Any(c => c.Name == n) || newCats.Any(c => c.Name == n));
                    var newCat   = DeepCloneCategory(src, name);
                    int idx      = grp.Categories.IndexOf(src);
                    grp.Categories.Insert(idx + 1, newCat);
                    newCats.Add(newCat);
                }

                UndoService.Push($"Duplicate {toClone.Count} categor{(toClone.Count == 1 ? "y" : "ies")}", () =>
                {
                    foreach (var c in newCats) grp.Categories.Remove(c);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                string label = toClone.Count == 1 ? $"\"{toClone[0].Name}\"" : $"{toClone.Count} categories";
                ToastService.Show($"Duplicated {label}");
                _selectedCategories.Clear();
                foreach (var c in newCats) _selectedCategories.Add(c);
                _selectedCategory = newCats.LastOrDefault();
                RefreshGroupsPanel();
                return;
            }

            // Duplicate selected group(s)
            if (_selectedGroups.Count > 0 && _selectedCollection != null)
            {
                var col     = _selectedCollection;
                var toClone = _selectedGroups.ToList();
                var newGrps = new List<CategoryGroup>();

                foreach (var src in toClone)
                {
                    string name = MakeUniqueName(src.Name, n => col.Groups.Any(g => g.Name == n) || newGrps.Any(g => g.Name == n));
                    var newGrp  = DeepCloneGroup(src, name);
                    int idx     = col.Groups.IndexOf(src);
                    col.Groups.Insert(idx + 1, newGrp);
                    newGrps.Add(newGrp);
                }

                UndoService.Push($"Duplicate {toClone.Count} group(s)", () =>
                {
                    foreach (var g in newGrps) col.Groups.Remove(g);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                string label = toClone.Count == 1 ? $"\"{toClone[0].Name}\"" : $"{toClone.Count} groups";
                ToastService.Show($"Duplicated {label}");
                _selectedGroups.Clear();
                foreach (var g in newGrps) { _selectedGroups.Add(g); _expandedGroups.Add(g); }
                _selectedGroup = newGrps.LastOrDefault();
                RefreshGroupsPanel();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // INLINE EDIT — second click on name opens TextBox in place
        // ════════════════════════════════════════════════════════════════════

        // ── Category name inline edit ─────────────────────────────────────

        private void CatNameBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock tb) return;
            var parent = VisualTreeHelper.GetParent(tb) as FrameworkElement;
            if (parent == null) return;
            var vm = FindTagInSubtree<CategoryViewModel>(parent);
            if (vm == null) return;
            if (_selectedCategory != vm.Model) return;
            e.Handled = true;

            var box = FindChildByName<TextBox>(parent, "CatNameBox");
            if (box == null) return;
            _editingCategory   = vm;
            box.Text           = vm.Name;
            box.Visibility     = Visibility.Visible;
            tb.Visibility      = Visibility.Collapsed;
            box.Focus(); box.SelectAll();
        }

        private void CatNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            if (e.Key == Key.Enter)       { CommitCatEdit(box);  e.Handled = true; }
            else if (e.Key == Key.Escape) { CancelCatEdit(box);  e.Handled = true; }
        }

        private void CatNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox box) CommitCatEdit(box);
        }

        private void CommitCatEdit(TextBox box)
        {
            if (_editingCategory == null) return;
            var vm           = _editingCategory;
            _editingCategory = null;
            string newName   = box.Text.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != vm.Name)
            {
                if (vm.Group.Categories.Any(c => c != vm.Model && c.Name == newName))
                {
                    ToastService.Show($"A category named \"{newName}\" already exists");
                    _editingCategory = vm;
                    box.Focus(); box.SelectAll(); return;
                }
                string oldName = vm.Name;
                vm.Model.Name  = newName;
                UndoService.Push($"Rename category to {newName}", () =>
                {
                    vm.Model.Name = oldName;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
                DataService.SaveCategories();
            }
            box.Visibility = Visibility.Collapsed;
            RefreshGroupsPanel();
        }

        private void CancelCatEdit(TextBox box)
        {
            _editingCategory   = null;
            box.Visibility     = Visibility.Collapsed;
        }

        // ── Option name inline edit ───────────────────────────────────────

        private void OptNameBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock tb) return;
            var parent = VisualTreeHelper.GetParent(tb) as FrameworkElement;
            if (parent == null) return;
            var vm = FindTagInSubtree<OptionViewModel>(parent);
            if (vm == null) return;
            if (_selectedOption != vm.Model && !_selectedOptions.Contains(vm.Model)) return;
            e.Handled = true;

            var box = FindChildByName<TextBox>(parent, "OptNameBox");
            if (box == null) return;
            _editingOption = vm;
            box.Text       = vm.Name;
            box.Visibility = Visibility.Visible;
            tb.Visibility  = Visibility.Collapsed;
            box.Focus(); box.SelectAll();
        }

        private void OptNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            if (e.Key == Key.Enter)       { CommitOptEdit(box); e.Handled = true; }
            else if (e.Key == Key.Escape) { CancelOptEdit(box); e.Handled = true; }
        }

        private void OptNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox box) CommitOptEdit(box);
        }

        private void CommitOptEdit(TextBox box)
        {
            if (_editingOption == null) return;
            var vm         = _editingOption;
            _editingOption = null;
            string newName = box.Text.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != vm.Name)
            {
                string oldName = vm.Name;
                vm.Model.Name  = newName;
                UndoService.Push($"Rename option to {newName}", () =>
                {
                    vm.Model.Name = oldName;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
                DataService.SaveCategories();
            }
            box.Visibility = Visibility.Collapsed;
            RefreshOptionsPanel();
        }

        private void CancelOptEdit(TextBox box)
        {
            _editingOption = null;
            box.Visibility = Visibility.Collapsed;
        }

        // ── Group detail name inline edit ─────────────────────────────────

        private void GroupDetailName_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedGroup == null) return;
            _editingGroupDetail           = true;
            GroupDetailName.Visibility    = Visibility.Collapsed;
            GroupDetailNameBox.Text       = _selectedGroup.Name;
            GroupDetailNameBox.Visibility = Visibility.Visible;
            GroupDetailNameBox.Focus();
            GroupDetailNameBox.SelectAll();
            e.Handled = true;
        }

        private void GroupDetailNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)       { CommitGroupDetailEdit(); e.Handled = true; }
            else if (e.Key == Key.Escape) { CancelGroupDetailEdit(); e.Handled = true; }
        }

        private void GroupDetailNameBox_LostFocus(object sender, RoutedEventArgs e)
            => CommitGroupDetailEdit();

        private void CommitGroupDetailEdit()
        {
            if (!_editingGroupDetail || _selectedGroup == null) return;
            _editingGroupDetail = false;
            string newName      = GroupDetailNameBox.Text.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != _selectedGroup.Name &&
                _selectedCollection != null)
            {
                if (_selectedCollection.Groups.Any(g => g != _selectedGroup && g.Name == newName))
                {
                    ToastService.Show($"A group named \"{newName}\" already exists");
                    GroupDetailNameBox.Focus(); GroupDetailNameBox.SelectAll();
                    _editingGroupDetail = true; return;
                }
                string oldName      = _selectedGroup.Name;
                _selectedGroup.Name = newName;
                UndoService.Push($"Rename group to {newName}", () =>
                {
                    _selectedGroup.Name = oldName;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
                DataService.SaveCategories();
            }

            GroupDetailNameBox.Visibility = Visibility.Collapsed;
            GroupDetailName.Text          = _selectedGroup?.Name ?? "";
            GroupDetailName.Visibility    = Visibility.Visible;
            RefreshGroupsPanel();
        }

        private void CancelGroupDetailEdit()
        {
            _editingGroupDetail           = false;
            GroupDetailNameBox.Visibility = Visibility.Collapsed;
            GroupDetailName.Visibility    = Visibility.Visible;
        }

        private void HandleCopy()
        {
            if (_selectedOptions.Count > 0 && _selectedCategory != null)
            {
                ClipboardService.CopyOptions(_selectedOptions.ToList());
                string names = _selectedOptions.Count == 1
                    ? _selectedOptions.First().Name
                    : $"{_selectedOptions.Count} options";
                ToastService.Show($"Copied {names}");
                _selectedOptions.Clear();
                _selectedOption = null;
                RefreshOptionsPanel();
            }
            else if (_selectedCategories.Count > 0 && _selectedGroup != null)
            {
                var pairs = _selectedCategories
                    .Select(c => (c, _selectedGroup)).ToList();
                ClipboardService.CopyCategories(pairs);
                string names = _selectedCategories.Count == 1
                    ? _selectedCategories.First().Name
                    : $"{_selectedCategories.Count} categories";
                ToastService.Show($"Copied {names}");
                _selectedCategories.Clear();
                _selectedCategory = null;
                RefreshGroupsPanel();
            }
            else if (_selectedGroup != null && _selectedCollection != null)
            {
                ClipboardService.CopyGroups(
                    new[] { (_selectedGroup, _selectedCollection) });
                ToastService.Show($"Copied {_selectedGroup.Name}");
            }
        }

        private void HandleCut()
        {
            if (_selectedOptions.Count > 0 && _selectedCategory != null)
            {
                ClipboardService.CutOptions(
                    _selectedOptions.ToList(),
                    _selectedOptions.ToList());
                string names = _selectedOptions.Count == 1
                    ? _selectedOptions.First().Name
                    : $"{_selectedOptions.Count} options";
                ToastService.Show($"Cut {names}");
                RefreshOptionsPanel();
            }
            else if (_selectedCategories.Count > 0 && _selectedGroup != null)
            {
                var pairs = _selectedCategories
                    .Select(c => (c, _selectedGroup)).ToList();
                ClipboardService.CutCategories(pairs,
                    _selectedCategories.ToList());
                string names = _selectedCategories.Count == 1
                    ? _selectedCategories.First().Name
                    : $"{_selectedCategories.Count} categories";
                ToastService.Show($"Cut {names} — Ctrl+V to paste");
                RefreshGroupsPanel();
            }
        }

        private void HandlePaste()
        {
            if (!ClipboardService.HasData) return;

            switch (ClipboardService.Level)
            {
                case ClipboardService.ClipLevel.Option:
                    PasteOptions();
                    break;
                case ClipboardService.ClipLevel.Category:
                    PasteCategories();
                    break;
                case ClipboardService.ClipLevel.Group:
                    PasteGroups();
                    break;
            }
        }

        private void PasteOptions()
        {
            if (_selectedCategory == null)
            {
                ToastService.Show("Select a category to paste into");
                return;
            }
            var cat = _selectedCategory;
            var toInsert = ClipboardService.PasteOptions(cat);

            if (ClipboardService.Mode == ClipboardService.ClipMode.Cut)
            {
                // Remove from source
                foreach (var src in ClipboardService.CutOptionItems.ToList())
                    foreach (var grp in DataService.Categories.Collections
                        .SelectMany(c => c.Groups))
                        foreach (var srcCat in grp.Categories)
                            srcCat.Options.Remove(src);
            }

            // Snapshot before for undo
            var snapshot = cat.Options.ToList();
            foreach (var opt in toInsert) cat.Options.Add(opt);

            UndoService.Push($"Paste {ClipboardService.FirstItemName}", () =>
            {
                cat.Options.Clear();
                foreach (var o in snapshot) cat.Options.Add(o);
                DataService.SaveCategories();
                RefreshGroupsPanel();
            });

            DataService.SaveCategories();
            string names = toInsert.Count == 1 ? toInsert[0].Name : $"{toInsert.Count} options";
            ToastService.Show($"Pasted {names}");

            // Select first pasted item
            _selectedOptions.Clear();
            if (toInsert.Count > 0) _selectedOptions.Add(toInsert[0]);

            if (ClipboardService.Mode == ClipboardService.ClipMode.Cut)
                ClipboardService.Clear();

            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void PasteCategories()
        {
            if (_selectedGroup == null)
            {
                ToastService.Show("Select a group to paste into");
                return;
            }
            var grp = _selectedGroup;
            var toInsert = ClipboardService.PasteCategories(grp);

            if (ClipboardService.Mode == ClipboardService.ClipMode.Cut)
            {
                foreach (var src in ClipboardService.CutCategoryItems.ToList())
                    foreach (var g in DataService.Categories.Collections.SelectMany(c => c.Groups))
                        g.Categories.Remove(src);
            }

            var snapshot = grp.Categories.ToList();
            foreach (var cat in toInsert) grp.Categories.Add(cat);

            UndoService.Push($"Paste {ClipboardService.FirstItemName}", () =>
            {
                grp.Categories.Clear();
                foreach (var c in snapshot) grp.Categories.Add(c);
                DataService.SaveCategories();
                RefreshGroupsPanel();
            });

            DataService.SaveCategories();
            string names = toInsert.Count == 1 ? toInsert[0].Name : $"{toInsert.Count} categories";
            ToastService.Show($"Pasted {names}");

            _selectedCategories.Clear();
            if (toInsert.Count > 0) _selectedCategories.Add(toInsert[0]);

            if (ClipboardService.Mode == ClipboardService.ClipMode.Cut)
                ClipboardService.Clear();

            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void PasteGroups()
        {
            if (_selectedCollection == null)
            {
                ToastService.Show("Select a collection to paste into");
                return;
            }
            var col = _selectedCollection;
            var toInsert = ClipboardService.PasteGroups(col);

            var snapshot = col.Groups.ToList();
            foreach (var grp in toInsert) col.Groups.Add(grp);

            UndoService.Push($"Paste {ClipboardService.FirstItemName}", () =>
            {
                col.Groups.Clear();
                foreach (var g in snapshot) col.Groups.Add(g);
                DataService.SaveCategories();
                RefreshGroupsPanel();
            });

            DataService.SaveCategories();
            ToastService.Show($"Pasted {toInsert[0].Name}");
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void HandleDelete()
        {
            // Delete selected options
            if (_selectedOptions.Count > 0 && _selectedCategory != null)
            {
                var cat = _selectedCategory;
                var toDelete = _selectedOptions.ToList();

                if (DataService.Settings.ConfirmOnDelete)
                {
                    string msg = toDelete.Count == 1
                        ? $"Delete option \"{toDelete[0].Name}\"?"
                        : $"Delete {toDelete.Count} options?";
                    if (new ConfirmDialog("Confirm Delete", msg + " This cannot be undone.")
                        { Owner = Main }.ShowDialog() != true) return;
                }

                var snapshot = cat.Options.ToList();
                foreach (var opt in toDelete) cat.Options.Remove(opt);
                _selectedOptions.Clear();

                UndoService.Push($"Delete {(toDelete.Count == 1 ? toDelete[0].Name : $"{toDelete.Count} options")}", () =>
                {
                    cat.Options.Clear();
                    foreach (var o in snapshot) cat.Options.Add(o);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Delete selected categories
            if (_selectedCategories.Count > 0 && _selectedGroup != null)
            {
                var grp = _selectedGroup;
                var toDelete = _selectedCategories.ToList();

                if (DataService.Settings.ConfirmOnDelete)
                {
                    string msg = toDelete.Count == 1
                        ? $"Delete category \"{toDelete[0].Name}\"?"
                        : $"Delete {toDelete.Count} categories?";
                    if (new ConfirmDialog("Confirm Delete", msg + " This cannot be undone.")
                        { Owner = Main }.ShowDialog() != true) return;
                }

                var snapshot = grp.Categories.ToList();
                foreach (var cat in toDelete) grp.Categories.Remove(cat);
                _selectedCategories.Clear();
                if (_selectedCategory != null && toDelete.Contains(_selectedCategory))
                    _selectedCategory = null;

                UndoService.Push($"Delete {(toDelete.Count == 1 ? toDelete[0].Name : $"{toDelete.Count} categories")}", () =>
                {
                    grp.Categories.Clear();
                    foreach (var c in snapshot) grp.Categories.Add(c);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Delete selected group(s) — supports multi-select
            if (_selectedGroups.Count > 0 && _selectedCollection != null &&
                _selectedCategories.Count == 0 && _selectedOptions.Count == 0)
            {
                var col      = _selectedCollection;
                var toDelete = _selectedGroups.ToList();

                if (DataService.Settings.ConfirmOnDelete)
                {
                    string msg = toDelete.Count == 1
                        ? $"Delete group \"{toDelete[0].Name}\"? This cannot be undone."
                        : $"Delete {toDelete.Count} groups? This cannot be undone.";
                    if (new ConfirmDialog("Confirm Delete", msg)
                    { Owner = Main }.ShowDialog() != true) return;
                }

                var snapshot = col.Groups.ToList();
                foreach (var grp in toDelete)
                {
                    col.Groups.Remove(grp);
                    _expandedGroups.Remove(grp);
                }
                _selectedGroup = null;
                _selectedGroups.Clear();

                string undoDesc = toDelete.Count == 1
                    ? $"Delete group {toDelete[0].Name}"
                    : $"Delete {toDelete.Count} groups";
                UndoService.Push(undoDesc, () =>
                {
                    col.Groups.Clear();
                    foreach (var g in snapshot) col.Groups.Add(g);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
            }
        }


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
                _expandedGroups.Clear();
                _selectedGroup                  = null;
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
            RefreshGroupsPanel();
        }

        private void CollectionRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CollectionViewModel vm)
            {
                _selectedCollection = vm.Model;
                _expandedGroups.Clear();
                _selectedGroup      = null;
                _selectedGroups.Clear();
                _selectedCategory   = null;
                _selectedOption     = null;
                _selectedOptions.Clear();
                _selectedCategories.Clear();
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
        // GROUPS
        // ════════════════════════════════════════════════════════════════════

        private void RefreshGroupsPanel()
        {
            bool hasCollection = _selectedCollection != null;

            GroupsZoneLabel.Text = hasCollection
                ? $"GROUPS — {_selectedCollection!.Name.ToUpper()}"
                : "GROUPS";

            AddGroupButton.IsEnabled = hasCollection && string.IsNullOrEmpty(_searchText);

            if (!hasCollection)
            {
                GroupsList.ItemsSource       = null;
                GroupsEmptyText.Visibility   = Visibility.Collapsed;
                GroupBulkControls.Visibility = Visibility.Collapsed;
                ShowRightPanelState(RightPanelState.Default);
                return;
            }

            var groups = _selectedCollection!.Groups;

            GroupBulkControls.Visibility = groups.Count > 0
                ? Visibility.Visible : Visibility.Collapsed;

            if (groups.Count == 0)
            {
                GroupsList.ItemsSource     = null;
                GroupsEmptyText.Visibility = Visibility.Visible;
                ShowRightPanelState(RightPanelState.Default);
                return;
            }

            GroupsEmptyText.Visibility = Visibility.Collapsed;

            _expandedGroups.RemoveWhere(g => !groups.Contains(g));
            if (_selectedGroup != null && !groups.Contains(_selectedGroup))
            {
                _selectedGroup    = null;
                _selectedGroups.Clear();
                _selectedCategory = null;
            }

            GroupsList.ItemsSource = groups
                .Select(g => BuildGroupViewModel(g))
                .ToList();

            RefreshRightPanel();
        }

        private GroupViewModel BuildGroupViewModel(CategoryGroup grp)
        {
            bool isExpanded = _expandedGroups.Contains(grp);
            string search   = _searchText.ToLower();
            bool hasSearch  = !string.IsNullOrEmpty(search);

            bool groupNameMatches = grp.Name.ToLower().Contains(search);
            bool anyCatMatches    = grp.Categories.Any(c => c.Name.ToLower().Contains(search));
            bool groupMatches     = !hasSearch || groupNameMatches || anyCatMatches;

            // During search: collapse non-matching groups regardless of prior expand state
            bool showCategories  = !hasSearch ? isExpanded : groupMatches;
            bool displayExpanded = !hasSearch ? isExpanded : groupMatches;

            var catVms = showCategories
                ? grp.Categories.Select(c =>
                {
                    bool catVisible = !hasSearch || c.Name.ToLower().Contains(search) || groupNameMatches;
                    bool isSelected = _selectedCategories.Contains(c) || c == _selectedCategory;
                    bool isCut = ClipboardService.Mode == ClipboardService.ClipMode.Cut &&
                                 ClipboardService.CutCategoryItems.Contains(c);
                    return new CategoryViewModel(c, grp, _selectedCollection!, isSelected, catVisible, isCut);
                }).ToList()
                : new List<CategoryViewModel>();

            bool isGroupSelected = _selectedGroups.Contains(grp);
            return new GroupViewModel(grp, displayExpanded, groupMatches, catVms, isGroupSelected);
        }

        private void GroupRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is GroupViewModel vm)
            {
                var grp  = vm.Group;
                bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

                if (ctrl)
                {
                    // Ctrl+click — toggle in multi-select, don't change expand state
                    if (_selectedGroups.Contains(grp))
                    {
                        _selectedGroups.Remove(grp);
                        if (_selectedGroup == grp)
                            _selectedGroup = _selectedGroups.FirstOrDefault();
                    }
                    else
                    {
                        _selectedGroups.Add(grp);
                        _selectedGroup = grp;
                    }
                    _selectedCategory = null;
                    _selectedOption   = null;
                    _selectedOptions.Clear();
                    _selectedCategories.Clear();
                }
                else
                {
                    // Single click — toggle expand, select this group, deselect others
                    if (_expandedGroups.Contains(grp))
                        _expandedGroups.Remove(grp);
                    else
                        _expandedGroups.Add(grp);

                    _selectedGroup    = grp;
                    _selectedGroups.Clear();
                    _selectedGroups.Add(grp);
                    _selectedCategory = null;
                    _selectedOption   = null;
                    _selectedOptions.Clear();
                    _selectedCategories.Clear();
                }
                RefreshGroupsPanel();
            }
        }

        private void GroupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is GroupViewModel vm)
            {
                var grp       = vm.Group;
                bool newState = grp.IsEnabled;
                bool oldState = !newState;
                UndoService.Push($"{(newState ? "Enable" : "Disable")} {grp.Name}", () =>
                {
                    grp.IsEnabled = oldState;
                    DataService.SaveCategories();
                    Main.RefreshGenerateButtonState();
                    RefreshGroupsPanel();
                });
            }
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshGroupsPanel();
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCollection == null) return;
            var col = _selectedCollection;
            var dlg = new InputDialog("Add Group",
                $"Enter name for new group in {col.Name}:", "",
                name => NameValidator.IsDuplicateGroup(name, col.Groups))
            { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                var grp = new CategoryGroup { Name = dlg.Result, IsEnabled = true };
                col.Groups.Add(grp);
                _expandedGroups.Add(grp);
                _selectedGroup = grp;

                UndoService.Push($"Add group {grp.Name}", () =>
                {
                    col.Groups.Remove(grp);
                    _expandedGroups.Remove(grp);
                    if (_selectedGroup == grp) _selectedGroup = null;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
            }
        }

        private void RenameGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedCollection == null) return;
            var grp     = _selectedGroup;
            var oldName = grp.Name;
            var dlg = new InputDialog("Rename Group", "Enter new name:", grp.Name,
                name => NameValidator.IsDuplicateGroup(
                    name, _selectedCollection.Groups, grp.Name))
            { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                grp.Name = dlg.Result;
                UndoService.Push($"Rename group to {grp.Name}", () =>
                {
                    grp.Name = oldName;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
                DataService.SaveCategories();
                RefreshGroupsPanel();
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedCollection == null) return;
            var grp = _selectedGroup;
            var col = _selectedCollection;
            if (DataService.Settings.ConfirmOnDelete)
            {
                int catCount = grp.Categories.Count;
                int optCount = grp.Categories.Sum(c => c.Options.Count);
                string msg   = catCount > 0
                    ? $"Delete group \"{grp.Name}\" and all {catCount} categories and {optCount} options? This cannot be undone."
                    : $"Delete group \"{grp.Name}\"? This cannot be undone.";
                if (new ConfirmDialog("Confirm Delete", msg) { Owner = Main }
                    .ShowDialog() != true) return;
            }
            var snapshot = col.Groups.ToList();
            col.Groups.Remove(grp);
            _expandedGroups.Remove(grp);
            if (_selectedGroup  == grp) { _selectedGroup = null; _selectedGroups.Remove(grp); }
            _selectedCategory = null;
            _selectedOption   = null;

            UndoService.Push($"Delete group {grp.Name}", () =>
            {
                col.Groups.Clear();
                foreach (var g in snapshot) col.Groups.Add(g);
                DataService.SaveCategories();
                RefreshGroupsPanel();
            });

            DataService.SaveCategories();
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void GroupDetailToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null) return;
            var grp       = _selectedGroup;
            bool oldState = grp.IsEnabled;
            bool newState = GroupDetailToggle.IsOn;
            if (newState == oldState) return; // no change

            grp.IsEnabled = newState;
            UndoService.Push($"{(newState ? "Enable" : "Disable")} {grp.Name}", () =>
            {
                grp.IsEnabled = oldState;
                DataService.SaveCategories();
                Main.RefreshGenerateButtonState();
                RefreshGroupsPanel();
            });
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshGroupsPanel();
        }

        private void BulkEnableGroups_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCollection == null) return;
            var dlg = new ConfirmDialog("Enable All Groups",
                $"Enable all {_selectedCollection.Groups.Count} groups in {_selectedCollection.Name}?",
                "Cancel", "Enable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            var states = _selectedCollection.Groups.ToDictionary(g => g, g => g.IsEnabled);
            foreach (var g in _selectedCollection.Groups) g.IsEnabled = true;
            UndoService.Push("Enable all groups", () =>
            {
                foreach (var kv in states) kv.Key.IsEnabled = kv.Value;
                DataService.SaveCategories();
                Main.RefreshGenerateButtonState();
                RefreshGroupsPanel();
            });
            DataService.SaveCategories();
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void BulkDisableGroups_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCollection == null) return;
            var dlg = new ConfirmDialog("Disable All Groups",
                $"Disable all {_selectedCollection.Groups.Count} groups in {_selectedCollection.Name}?",
                "Cancel", "Disable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            var states = _selectedCollection.Groups.ToDictionary(g => g, g => g.IsEnabled);
            foreach (var g in _selectedCollection.Groups) g.IsEnabled = false;
            UndoService.Push("Disable all groups", () =>
            {
                foreach (var kv in states) kv.Key.IsEnabled = kv.Value;
                DataService.SaveCategories();
                Main.RefreshGenerateButtonState();
                RefreshGroupsPanel();
            });
            DataService.SaveCategories();
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }


        // ════════════════════════════════════════════════════════════════════
        // CATEGORIES
        // ════════════════════════════════════════════════════════════════════

        private void AddCategoryInGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is GroupViewModel vm)
                AddCategoryToGroup(vm.Group);
        }

        private void AddCategoryInGroupDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup != null)
                AddCategoryToGroup(_selectedGroup);
        }

        private void AddCategoryToGroup(CategoryGroup grp)
        {
            var dlg = new InputDialog("Add Category",
                $"Enter name for new category in {grp.Name}:", "",
                name => NameValidator.IsDuplicateCategory(name, grp.Categories))
            { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                var cat = new Category { Name = dlg.Result };
                grp.Categories.Add(cat);
                _expandedGroups.Add(grp);
                _selectedGroup    = grp;
                _selectedCategory = cat;
                _selectedOption   = null;
                _selectedOptions.Clear();

                UndoService.Push($"Add category {cat.Name}", () =>
                {
                    grp.Categories.Remove(cat);
                    if (_selectedCategory == cat) _selectedCategory = null;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
            }
        }

        private void CategoryRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                if (ctrl)
                {
                    // Multi-select within same group only
                    if (_selectedGroup == vm.Group)
                    {
                        if (_selectedCategories.Contains(vm.Model))
                            _selectedCategories.Remove(vm.Model);
                        else
                            _selectedCategories.Add(vm.Model);
                        _selectedCategory = vm.Model;
                    }
                    else
                    {
                        // Different group — clear and select new
                        _selectedCategories.Clear();
                        _selectedCategories.Add(vm.Model);
                        _selectedCategory = vm.Model;
                        _expandedGroups.Add(vm.Group);
                        _selectedGroup = vm.Group;
                    }
                }
                else
                {
                    bool wasSelected = _selectedCategory == vm.Model &&
                                       _selectedCategories.Count <= 1;
                    _selectedCategories.Clear();
                    _selectedCategory = wasSelected ? null : vm.Model;
                    _selectedOption   = null;
                    _selectedOptions.Clear();
                    if (_selectedCategory != null)
                    {
                        _selectedCategories.Add(_selectedCategory);
                        _expandedGroups.Add(vm.Group);
                        _selectedGroup = vm.Group;
                    }
                }
                RefreshGroupsPanel();
            }
        }

        private void GroupDetailCategoryRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                _selectedCategories.Clear();
                _selectedCategory = vm.Model;
                _selectedCategories.Add(vm.Model);
                _expandedGroups.Add(vm.Group);
                _selectedGroup    = vm.Group;
                _selectedOption   = null;
                _selectedOptions.Clear();
                RefreshGroupsPanel();
            }
        }

        private void CategoryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                var cat       = vm.Model;
                bool newState = cat.IsEnabled;
                bool oldState = !newState;
                UndoService.Push($"{(newState ? "Enable" : "Disable")} {cat.Name}", () =>
                {
                    cat.IsEnabled = oldState;
                    DataService.SaveCategories();
                    Main.RefreshGenerateButtonState();
                    RefreshGroupsPanel();
                });
            }
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshGroupsPanel();
        }

        private void RenameCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                var oldName = vm.Name;
                var dlg = new InputDialog("Rename Category", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicateCategory(
                        name, vm.Group.Categories, vm.Name))
                { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    UndoService.Push($"Rename category to {vm.Model.Name}", () =>
                    {
                        vm.Model.Name = oldName;
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                }
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    string msg = vm.Model.Options.Count > 0
                        ? $"Delete \"{vm.Name}\" and all {vm.Model.Options.Count} option(s)? This cannot be undone."
                        : $"Delete category \"{vm.Name}\"? This cannot be undone.";
                    if (new ConfirmDialog("Confirm Delete", msg) { Owner = Main }
                        .ShowDialog() != true) return;
                }
                var grp      = vm.Group;
                var snapshot = grp.Categories.ToList();
                grp.Categories.Remove(vm.Model);
                if (_selectedCategory == vm.Model) { _selectedCategory = null; _selectedOption = null; }
                _selectedCategories.Remove(vm.Model);

                UndoService.Push($"Delete category {vm.Name}", () =>
                {
                    grp.Categories.Clear();
                    foreach (var c in snapshot) grp.Categories.Add(c);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
            }
        }


        // ════════════════════════════════════════════════════════════════════
        // OPTIONS
        // ════════════════════════════════════════════════════════════════════

        // ── Inline edit state ────────────────────────────────────────────────
        private CategoryViewModel? _editingCategory;
        private OptionViewModel?   _editingOption;
        private bool               _editingGroupDetail;

        // Keep Option? _selectedOption for backwards compat with RefreshOptionsPanel
        private Option? _selectedOption;

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

            // Header with multi-select count
            int selCount = _selectedOptions.Count;
            RightPanelTitle.Text = selCount > 1
                ? $"{_selectedCategory.Name} — Options ({selCount} selected)"
                : $"{_selectedCategory.Name} — Options";
            AddOptionButton.Visibility = Visibility.Visible;

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
                var cat = _selectedCategory;
                OptionsList.ItemsSource = cat.Options
                    .Select(o => new OptionViewModel(o, cat,
                        _selectedOptions.Contains(o) || o == _selectedOption,
                        ClipboardService.Mode == ClipboardService.ClipMode.Cut &&
                        ClipboardService.CutOptionItems.Contains(o)))
                    .ToList();
            }
        }

        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory == null) return;
            var cat = _selectedCategory;
            var dlg = new InputDialog("Add Option", "Enter option name:", "",
                name => NameValidator.IsDuplicateOption(name, cat.Options))
            { Owner = Main };
            if (dlg.ShowDialog() == true)
            {
                var opt = new Option { Name = dlg.Result };
                cat.Options.Add(opt);

                UndoService.Push($"Add option {opt.Name}", () =>
                {
                    cat.Options.Remove(opt);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
            }
        }

        private void EditOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm &&
                _selectedCategory != null)
            {
                var oldName = vm.Name;
                var dlg = new InputDialog("Edit Option", "Enter new name:", vm.Name,
                    name => NameValidator.IsDuplicateOption(
                        name, _selectedCategory.Options, vm.Name))
                { Owner = Main };
                if (dlg.ShowDialog() == true)
                {
                    vm.Model.Name = dlg.Result;
                    UndoService.Push($"Rename option to {vm.Model.Name}", () =>
                    {
                        vm.Model.Name = oldName;
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
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
                var cat      = _selectedCategory;
                var snapshot = cat.Options.ToList();
                cat.Options.Remove(vm.Model);
                _selectedOptions.Remove(vm.Model);
                if (_selectedOption == vm.Model) _selectedOption = null;

                UndoService.Push($"Delete option {vm.Name}", () =>
                {
                    cat.Options.Clear();
                    foreach (var o in snapshot) cat.Options.Add(o);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
            }
        }

        private void OptionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                var opt       = vm.Model;
                bool newState = opt.IsEnabled;
                bool oldState = !newState;
                UndoService.Push($"{(newState ? "Enable" : "Disable")} {opt.Name}", () =>
                {
                    opt.IsEnabled = oldState;
                    DataService.SaveCategories();
                    Main.RefreshGenerateButtonState();
                    RefreshGroupsPanel();
                });
            }
            DataService.SaveCategories();
            Main.RefreshGenerateButtonState();
            RefreshGroupsPanel();
        }

        private void OptionRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                if (ctrl && _selectedCategory == vm.Category)
                {
                    if (_selectedOptions.Contains(vm.Model))
                        _selectedOptions.Remove(vm.Model);
                    else
                        _selectedOptions.Add(vm.Model);
                    _selectedOption = vm.Model;
                }
                else
                {
                    _selectedOptions.Clear();
                    bool wasSelected = _selectedOption == vm.Model;
                    _selectedOption = wasSelected ? null : vm.Model;
                    if (_selectedOption != null) _selectedOptions.Add(_selectedOption);
                }
                RefreshOptionsPanel();
            }
        }

        private void WeightBadge_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is OptionViewModel vm)
            {
                var oldWeight = vm.Model.Weight;
                // Left-click cycles up (higher chance), right-click cycles down (lower chance)
                vm.Model.Weight = e.ChangedButton == MouseButton.Right
                    ? CycleWeightDown(vm.Model.Weight)
                    : CycleWeightUp(vm.Model.Weight);

                UndoService.Push($"Change weight of {vm.Name}", () =>
                {
                    vm.Model.Weight = oldWeight;
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
                DataService.SaveCategories();
                RefreshGroupsPanel();
            }
        }

        private static WeightTier CycleWeightDown(WeightTier w) => w switch
        {
            WeightTier.UltraHigh => WeightTier.High,
            WeightTier.High      => WeightTier.Normal,
            WeightTier.Normal    => WeightTier.Low,
            WeightTier.Low       => WeightTier.Rare,
            WeightTier.Rare      => WeightTier.UltraRare,
            WeightTier.UltraRare => WeightTier.UltraHigh,
            _                    => WeightTier.Normal
        };

        private static WeightTier CycleWeightUp(WeightTier w) => w switch
        {
            WeightTier.UltraHigh => WeightTier.UltraRare,
            WeightTier.UltraRare => WeightTier.Rare,
            WeightTier.Rare      => WeightTier.Low,
            WeightTier.Low       => WeightTier.Normal,
            WeightTier.Normal    => WeightTier.High,
            WeightTier.High      => WeightTier.UltraHigh,
            _                    => WeightTier.Normal
        };

        private void BulkEnableOptions_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategory == null) return;
            var dlg = new ConfirmDialog("Enable All Options",
                $"Enable all {_selectedCategory.Options.Count} options in {_selectedCategory.Name}?",
                "Cancel", "Enable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            var cat    = _selectedCategory;
            var states = cat.Options.ToDictionary(o => o, o => o.IsEnabled);
            foreach (var opt in cat.Options) opt.IsEnabled = true;
            UndoService.Push("Enable all options", () =>
            {
                foreach (var kv in states) kv.Key.IsEnabled = kv.Value;
                DataService.SaveCategories();
                Main.RefreshGenerateButtonState();
                RefreshGroupsPanel();
            });
            DataService.SaveCategories();
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }

        private void BulkDisableOptions_Click(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategory == null) return;
            var dlg = new ConfirmDialog("Disable All Options",
                $"Disable all {_selectedCategory.Options.Count} options in {_selectedCategory.Name}?",
                "Cancel", "Disable All") { Owner = Main };
            if (dlg.ShowDialog() != true) return;
            var cat    = _selectedCategory;
            var states = cat.Options.ToDictionary(o => o, o => o.IsEnabled);
            foreach (var opt in cat.Options) opt.IsEnabled = false;
            UndoService.Push("Disable all options", () =>
            {
                foreach (var kv in states) kv.Key.IsEnabled = kv.Value;
                DataService.SaveCategories();
                Main.RefreshGenerateButtonState();
                RefreshGroupsPanel();
            });
            DataService.SaveCategories();
            RefreshGroupsPanel();
            Main.RefreshGenerateButtonState();
        }


        // ════════════════════════════════════════════════════════════════════
        // RIGHT PANEL STATE MACHINE
        // ════════════════════════════════════════════════════════════════════

        private enum RightPanelState { Default, GroupDetail, Options }

        private void RefreshRightPanel()
        {
            if (_selectedCategory != null)
            {
                ShowRightPanelState(RightPanelState.Options);
                RefreshOptionsPanel();
            }
            else if (_selectedGroup != null)
            {
                ShowRightPanelState(RightPanelState.GroupDetail);
                RefreshGroupDetail();
            }
            else
            {
                ShowRightPanelState(RightPanelState.Default);
            }
        }

        private void ShowRightPanelState(RightPanelState state)
        {
            SelectItemText.Visibility   = state == RightPanelState.Default    ? Visibility.Visible : Visibility.Collapsed;
            GroupDetailPanel.Visibility = state == RightPanelState.GroupDetail ? Visibility.Visible : Visibility.Collapsed;
            OptionsPanel.Visibility     = state == RightPanelState.Options     ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshGroupDetail()
        {
            if (_selectedGroup == null) return;
            var grp = _selectedGroup;
            GroupDetailName.Text   = grp.Name;
            GroupDetailToggle.IsOn = grp.IsEnabled;

            int catCount    = grp.Categories.Count;
            int optCount    = grp.Categories.Sum(c => c.Options.Count);
            int enabledCats = grp.Categories.Count(c => c.IsEnabled);
            // Compact single-line stats for the header bar
            GroupDetailStats.Text =
                $"{catCount} categor{(catCount == 1 ? "y" : "ies")} · {optCount} options · {enabledCats}/{catCount} enabled";

            // Pass IsSelected so selected category highlights in both panels
            GroupDetailCategoryList.ItemsSource = grp.Categories
                .Select(c => new CategoryViewModel(
                    c, grp, _selectedCollection!,
                    isSelected: c == _selectedCategory || _selectedCategories.Contains(c)))
                .ToList();
        }


        // ════════════════════════════════════════════════════════════════════
        // SEARCH
        // ════════════════════════════════════════════════════════════════════

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text ?? "";
            // Show/hide placeholder
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = string.IsNullOrEmpty(_searchText)
                    ? Visibility.Visible : Visibility.Collapsed;
            ClearSearchButton.Visibility = string.IsNullOrEmpty(_searchText)
                ? Visibility.Collapsed : Visibility.Visible;
            AddGroupButton.IsEnabled = string.IsNullOrEmpty(_searchText) &&
                                       _selectedCollection != null;

            if (!string.IsNullOrEmpty(_searchText) && _selectedCollection != null)
            {
                string search = _searchText.ToLower();
                foreach (var grp in _selectedCollection.Groups)
                {
                    bool grpMatches = grp.Name.ToLower().Contains(search);
                    bool catMatches = grp.Categories.Any(c => c.Name.ToLower().Contains(search));
                    if (grpMatches || catMatches) _expandedGroups.Add(grp);
                }
            }

            RefreshGroupsPanel();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text               = "";
            _searchText                  = "";
            ClearSearchButton.Visibility = Visibility.Collapsed;
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = Visibility.Visible;
            AddGroupButton.IsEnabled     = _selectedCollection != null;
            RefreshGroupsPanel();
        }


        // ════════════════════════════════════════════════════════════════════
        // NAV
        // ════════════════════════════════════════════════════════════════════

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
        // DRAG AND DROP
        //
        // Phase 5 additions:
        // - _dragIsCopy: Ctrl held during drag = copy mode
        // - Ghost border turns green with "COPY" badge in copy mode
        // - Source item dims to 40% in move mode, returns to full in copy mode
        // - PreviewKeyUp hooked on Window to toggle copy mode live
        // - Copy commits create deep copies; move commits stay as before
        // - Undo pushed on every commit
        // ════════════════════════════════════════════════════════════════════

        private void CollectionDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is CollectionViewModel vm)
            {
                if (_dragColSource == null)
                {
                    _dragColSource   = vm;
                    _dragMode        = DragMode.Collection;
                    _dragStartScreen = PointToScreen(e.GetPosition(this));
                }
            }
        }

        private void GroupsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartScreen = PointToScreen(e.GetPosition(this));
            _dragGrpSource   = null;
            _dragCatSource   = null;
            _dragMode        = DragMode.None;
        }

        private void GroupDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is GroupViewModel vm)
            {
                _dragGrpSource = vm;
                _dragMode      = DragMode.Group;
            }
        }

        private void GroupsList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragActive) return;
            if (_dragGrpSource == null && _dragCatSource == null) return;
            var screen = PointToScreen(e.GetPosition(this));
            if (Math.Abs(screen.X - _dragStartScreen.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(screen.Y - _dragStartScreen.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                string name = _dragGrpSource?.Name ?? _dragCatSource?.Name ?? "";
                BeginDrag(name);
            }
        }

        private void CategorySubList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartScreen = PointToScreen(e.GetPosition(this));
            _dragCatSource   = null;
            _dragMode        = DragMode.None;
        }

        private void CategoryDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is FrameworkElement fe && fe.Tag is CategoryViewModel vm)
            {
                _dragCatSource = vm;
                _dragMode      = DragMode.Category;
            }
        }

        private void CategorySubList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||
                _dragCatSource == null || _dragActive) return;
            var screen = PointToScreen(e.GetPosition(this));
            if (Math.Abs(screen.X - _dragStartScreen.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(screen.Y - _dragStartScreen.Y) > SystemParameters.MinimumVerticalDragDistance)
                BeginDrag(_dragCatSource.Name);
        }

        private void OptionsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartScreen = PointToScreen(e.GetPosition(this));
            _dragOptSource   = null;
            _dragMode        = DragMode.Option;
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

        private void BeginDrag(string name)
        {
            _dragActive = true;
            _dragIsCopy = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            // Ghost label text
            var labelText = new TextBlock
            {
                Text = name,
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            };
            _ghostCopyBadge = new TextBlock
            {
                Text = "COPY",
                Foreground = new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58)),
                FontSize = 10, FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
                Visibility = _dragIsCopy ? Visibility.Visible : Visibility.Collapsed
            };

            _ghostEl = new Border
            {
                BorderThickness  = new Thickness(1.5),
                CornerRadius     = new CornerRadius(6),
                Padding          = new Thickness(8, 7, 8, 7),
                Opacity          = _dragIsCopy ? 0.95 : 0.80,
                IsHitTestVisible = false,
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "⠿",
                            Foreground = (Brush)Application.Current.Resources["BorderSelectedBrush"],
                            FontSize = 14, Margin = new Thickness(0, 0, 8, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        labelText,
                        _ghostCopyBadge
                    }
                }
            };

            UpdateGhostCopyStyle();

            _lineEl = new Line
            {
                StrokeThickness  = 2,
                Stroke           = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff)),
                IsHitTestVisible = false
            };

            DragOverlay.Children.Clear();
            DragOverlay.Children.Add(_ghostEl);
            DragOverlay.Children.Add(_lineEl);
            DragOverlay.Background       = Brushes.Transparent;
            DragOverlay.IsHitTestVisible = true;
            DragOverlay.Visibility       = Visibility.Visible;

            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         += OnDragMouseMove;
                win.MouseLeftButtonUp += OnDragMouseUp;
                win.LostMouseCapture  += OnDragCancelled;
                win.PreviewKeyDown    += OnDragKeyDown;
                win.PreviewKeyUp      += OnDragKeyUp;
            }
        }

        private void UpdateGhostCopyStyle()
        {
            if (_ghostEl == null) return;
            if (_dragIsCopy)
            {
                _ghostEl.Background  = new SolidColorBrush(Color.FromArgb(30, 0x30, 0xd1, 0x58));
                _ghostEl.BorderBrush = new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58));
                _ghostEl.Opacity     = 0.95;
            }
            else
            {
                _ghostEl.Background  = (Brush)Application.Current.Resources["BackgroundCardBrush"];
                _ghostEl.BorderBrush = new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff));
                _ghostEl.Opacity     = 0.80;
            }
            if (_ghostCopyBadge != null)
                _ghostCopyBadge.Visibility = _dragIsCopy
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragActive || _ghostEl == null) return;
            var pt = e.GetPosition(this);
            _lastDragPt = pt;
            Canvas.SetLeft(_ghostEl, pt.X + 10);
            Canvas.SetTop(_ghostEl,  pt.Y - 12);
            UpdateDragFeedback(pt);
        }

        private void UpdateDragFeedback(Point pt)
        {
            ClearHighlight();
            HideInsertionLine();
            _pendingGroupTarget    = null;
            _pendingCategoryTarget = null;

            switch (_dragMode)
            {
                case DragMode.Group:    UpdateGroupDragFeedback(pt);    break;
                case DragMode.Category: UpdateCategoryDragFeedback(pt); break;
                case DragMode.Option:   UpdateOptionDragFeedback(pt);   break;
                default: SetGhostValid(false); break;
            }
        }

        private void UpdateGroupDragFeedback(Point pt)
        {
            if (_ghostEl == null) return;
            var listPt = this.TranslatePoint(pt, GroupsList);
            bool isOver = listPt.X >= 0 && listPt.Y >= 0 &&
                          listPt.X <= GroupsList.ActualWidth &&
                          listPt.Y <= GroupsList.ActualHeight;
            if (isOver)
            {
                SetGhostValid(true);
                _ghostEl.Width = GroupsScrollViewer.ActualWidth;
                ShowInsertionLine(GroupsList, listPt, pt);
            }
            else SetGhostValid(false);
        }

        private void UpdateCategoryDragFeedback(Point pt)
        {
            if (_ghostEl == null || _dragCatSource == null) return;
            var listPt = this.TranslatePoint(pt, GroupsList);
            bool overGroups = listPt.X >= 0 && listPt.Y >= 0 &&
                              listPt.X <= GroupsList.ActualWidth &&
                              listPt.Y <= GroupsList.ActualHeight;
            if (overGroups)
            {
                var grpVm = GetViewModelAtPoint<GroupViewModel>(GroupsList, listPt);
                if (grpVm != null && (grpVm.Group != _dragCatSource.Group || _dragIsCopy))
                {
                    SetGhostValid(true);
                    _ghostEl.Width      = GroupsScrollViewer.ActualWidth;
                    HighlightBorderAtPoint(GroupsList, listPt);
                    _pendingGroupTarget = grpVm;
                    return;
                }
            }
            if (_expandedGroups.Count > 0)
            {
                foreach (var (catList, _) in FindAllExpandedCategoryLists())
                {
                    var catPt = this.TranslatePoint(pt, catList);
                    bool overCats = catPt.X >= 0 && catPt.Y >= 0 &&
                                    catPt.X <= catList.ActualWidth &&
                                    catPt.Y <= catList.ActualHeight;
                    if (overCats)
                    {
                        SetGhostValid(true);
                        _ghostEl.Width = catList.ActualWidth;
                        ShowInsertionLine(catList, catPt, pt);
                        return;
                    }
                }
            }
            SetGhostValid(false);
        }

        private void UpdateOptionDragFeedback(Point pt)
        {
            if (_ghostEl == null) return;
            var optPt = this.TranslatePoint(pt, OptionsList);
            bool overOpts = optPt.X >= 0 && optPt.Y >= 0 &&
                            optPt.X <= OptionsList.ActualWidth &&
                            optPt.Y <= OptionsList.ActualHeight;
            if (overOpts)
            {
                SetGhostValid(true);
                _ghostEl.Width = OptionsScrollViewer.ActualWidth;
                ShowInsertionLine(OptionsList, optPt, pt);
                return;
            }
            if (_expandedGroups.Count > 0 && _dragOptSource != null)
            {
                foreach (var (catList, _) in FindAllExpandedCategoryLists())
                {
                    var catPt = this.TranslatePoint(pt, catList);
                    bool overCats = catPt.X >= 0 && catPt.Y >= 0 &&
                                    catPt.X <= catList.ActualWidth &&
                                    catPt.Y <= catList.ActualHeight;
                    if (overCats)
                    {
                        var catVm = GetViewModelAtPoint<CategoryViewModel>(catList, catPt);
                        if (catVm != null && (catVm.Model != _dragOptSource.Category || _dragIsCopy))
                        {
                            SetGhostValid(true);
                            _ghostEl.Width         = catList.ActualWidth;
                            HighlightBorderAtPoint(catList, catPt);
                            _pendingCategoryTarget = catVm;
                            return;
                        }
                    }
                }
            }
            SetGhostValid(false);
        }

        private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragActive) return;
            e.Handled = true;
            CommitDrop(_lastDragPt);
            EndDrag();
        }

        private void CommitDrop(Point pt)
        {
            switch (_dragMode)
            {
                case DragMode.Group:    CommitGroupDrop(pt);    break;
                case DragMode.Category: CommitCategoryDrop(pt); break;
                case DragMode.Option:   CommitOptionDrop(pt);   break;
            }
        }

        private void CommitGroupDrop(Point pt)
        {
            if (_dragGrpSource == null || _selectedCollection == null) return;
            var groups = _selectedCollection.Groups;
            var listPt = this.TranslatePoint(pt, GroupsList);
            bool isOver = listPt.X >= 0 && listPt.Y >= 0 &&
                          listPt.X <= GroupsList.ActualWidth &&
                          listPt.Y <= GroupsList.ActualHeight;
            if (!isOver || _dropIndex < 0) return;
            var srcGrp = _dragGrpSource.Group;

            if (_dragIsCopy)
            {
                // Copy — deep clone the group with counter suffix if needed
                string finalName = MakeUniqueName(srcGrp.Name, n => groups.Any(g => g.Name == n));

                var newGrp   = DeepCloneGroup(srcGrp, finalName);
                int insertAt = Math.Min(_dropIndex, groups.Count);
                groups.Insert(insertAt, newGrp);

                UndoService.Push($"Copy group {srcGrp.Name}", () =>
                {
                    groups.Remove(newGrp);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }
            else
            {
                // Move — reorder
                int fromIdx = groups.IndexOf(srcGrp);
                int count   = groups.Count;
                if (fromIdx < 0 || _dropIndex > count) return;
                if (_dropIndex == fromIdx || _dropIndex == fromIdx + 1) return;
                int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
                toIdx = Math.Max(0, Math.Min(toIdx, groups.Count - 1));

                var snapshot = groups.ToList();
                groups.RemoveAt(fromIdx);
                groups.Insert(Math.Min(toIdx, groups.Count), srcGrp);

                UndoService.Push($"Reorder group {srcGrp.Name}", () =>
                {
                    groups.Clear();
                    foreach (var g in snapshot) groups.Add(g);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }

            DataService.SaveCategories();
            RefreshGroupsPanel();
        }

        private void CommitCategoryDrop(Point pt)
        {
            if (_dragCatSource == null) return;
            var srcCat = _dragCatSource.Model;
            var srcGrp = _dragCatSource.Group;

            if (_pendingGroupTarget != null &&
                (_pendingGroupTarget.Group != srcGrp || _dragIsCopy) &&
                _selectedCollection != null &&
                _selectedCollection.Groups.Contains(_pendingGroupTarget.Group))
            {
                var targetGrp = _pendingGroupTarget.Group;

                if (_dragIsCopy)
                {
                    string name = MakeUniqueName(srcCat.Name, n => targetGrp.Categories.Any(c => c.Name == n));
                    var newCat = DeepCloneCategory(srcCat, name);
                    targetGrp.Categories.Add(newCat);

                    UndoService.Push($"Copy category {srcCat.Name}", () =>
                    {
                        targetGrp.Categories.Remove(newCat);
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                }
                else
                {
                    var srcSnap  = srcGrp.Categories.ToList();
                    var destSnap = targetGrp.Categories.ToList();
                    srcGrp.Categories.Remove(srcCat);
                    targetGrp.Categories.Add(srcCat);

                    if (_selectedCategory == srcCat)
                    {
                        _expandedGroups.Add(targetGrp);
                        _selectedGroup = targetGrp;
                    }

                    UndoService.Push($"Move category {srcCat.Name}", () =>
                    {
                        targetGrp.Categories.Clear();
                        foreach (var c in destSnap) targetGrp.Categories.Add(c);
                        srcGrp.Categories.Clear();
                        foreach (var c in srcSnap) srcGrp.Categories.Add(c);
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                }

                DataService.SaveCategories();
                RefreshGroupsPanel();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Same-group reorder (copy just inserts a clone)
            var reorderGroup = srcGrp;
            var cats    = reorderGroup.Categories;
            int fromIdx = cats.IndexOf(srcCat);
            int count   = cats.Count;
            if (fromIdx < 0 || _dropIndex < 0 || _dropIndex > count) return;

            if (_dragIsCopy)
            {
                string name = MakeUniqueName(srcCat.Name, n => cats.Any(c => c.Name == n));
                var newCat = DeepCloneCategory(srcCat, name);
                int insertAt = Math.Min(_dropIndex, cats.Count);
                cats.Insert(insertAt, newCat);

                UndoService.Push($"Copy category {srcCat.Name}", () =>
                {
                    cats.Remove(newCat);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }
            else
            {
                if (_dropIndex == fromIdx || _dropIndex == fromIdx + 1) return;
                int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
                toIdx = Math.Max(0, Math.Min(toIdx, cats.Count - 1));
                var snapshot = cats.ToList();
                cats.RemoveAt(fromIdx);
                cats.Insert(Math.Min(toIdx, cats.Count), srcCat);

                UndoService.Push($"Reorder category {srcCat.Name}", () =>
                {
                    cats.Clear();
                    foreach (var c in snapshot) cats.Add(c);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }

            DataService.SaveCategories();
            RefreshGroupsPanel();
        }

        private void CommitOptionDrop(Point pt)
        {
            if (_dragOptSource == null) return;
            var srcOpt = _dragOptSource.Model;
            var srcCat = _dragOptSource.Category;

            if (_pendingCategoryTarget != null &&
                (_pendingCategoryTarget.Model != srcCat || _dragIsCopy))
            {
                var targetCat = _pendingCategoryTarget.Model;

                string name = MakeUniqueName(srcOpt.Name, n => targetCat.Options.Any(o => o.Name == n));
                var newOpt = new Option { Name = name, Weight = srcOpt.Weight, IsEnabled = srcOpt.IsEnabled };

                if (!_dragIsCopy)
                {
                    var srcSnap = srcCat.Options.ToList();
                    srcCat.Options.Remove(srcOpt);

                    UndoService.Push($"Move option {srcOpt.Name}", () =>
                    {
                        targetCat.Options.Remove(newOpt);
                        srcCat.Options.Clear();
                        foreach (var o in srcSnap) srcCat.Options.Add(o);
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                }
                else
                {
                    UndoService.Push($"Copy option {srcOpt.Name}", () =>
                    {
                        targetCat.Options.Remove(newOpt);
                        DataService.SaveCategories();
                        RefreshGroupsPanel();
                    });
                }

                targetCat.Options.Add(newOpt);
                DataService.SaveCategories();
                RefreshOptionsPanel();
                Main.RefreshGenerateButtonState();
                return;
            }

            // Same-category reorder
            if (_selectedCategory == null) return;
            var opts    = _selectedCategory.Options;
            int fromIdx = opts.IndexOf(srcOpt);
            int cnt     = opts.Count;
            if (fromIdx < 0 || _dropIndex < 0 || _dropIndex > cnt) return;

            if (_dragIsCopy)
            {
                // Same-container copy — apply counter suffix
                string finalName = MakeUniqueName(srcOpt.Name, n => opts.Any(o => o.Name == n));
                var newOpt   = new Option { Name = finalName, Weight = srcOpt.Weight, IsEnabled = srcOpt.IsEnabled };
                int insertAt = Math.Min(_dropIndex, opts.Count);
                opts.Insert(insertAt, newOpt);

                UndoService.Push($"Copy option {srcOpt.Name}", () =>
                {
                    opts.Remove(newOpt);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }
            else
            {
                if (_dropIndex == fromIdx || _dropIndex == fromIdx + 1) return;
                int toIdx = _dropIndex > fromIdx ? _dropIndex - 1 : _dropIndex;
                toIdx = Math.Max(0, Math.Min(toIdx, opts.Count - 1));
                var snapshot = opts.ToList();
                opts.RemoveAt(fromIdx);
                opts.Insert(Math.Min(toIdx, opts.Count), srcOpt);

                UndoService.Push($"Reorder option {srcOpt.Name}", () =>
                {
                    opts.Clear();
                    foreach (var o in snapshot) opts.Add(o);
                    DataService.SaveCategories();
                    RefreshGroupsPanel();
                });
            }

            DataService.SaveCategories();
            RefreshOptionsPanel();
        }

        private void OnDragCancelled(object sender, MouseEventArgs e) => CancelDrag();

        private void OnDragKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { CancelDrag(); return; }
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _dragIsCopy = true;
                UpdateGhostCopyStyle();
            }
        }

        private void OnDragKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _dragIsCopy = false;
                UpdateGhostCopyStyle();
            }
        }

        private void CancelDrag() => EndDrag();

        private void EndDrag()
        {
            _dragActive    = false;
            _dragIsCopy    = false;
            _dragMode      = DragMode.None;
            _dragColSource = null;
            _dragGrpSource = null;
            _dragCatSource = null;
            _dragOptSource = null;
            _dropIndex     = -1;
            _pendingGroupTarget    = null;
            _pendingCategoryTarget = null;
            ClearHighlight();
            DragOverlay.Children.Clear();
            DragOverlay.Background       = null;
            DragOverlay.IsHitTestVisible = false;
            DragOverlay.Visibility       = Visibility.Collapsed;
            _ghostEl        = null;
            _ghostCopyBadge = null;
            _lineEl         = null;

            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.MouseMove         -= OnDragMouseMove;
                win.MouseLeftButtonUp -= OnDragMouseUp;
                win.LostMouseCapture  -= OnDragCancelled;
                win.PreviewKeyDown    -= OnDragKeyDown;
                win.PreviewKeyUp      -= OnDragKeyUp;
            }
        }


        // ── Visual feedback ───────────────────────────────────────────────

        private void SetGhostValid(bool valid)
        {
            if (_ghostEl == null) return;
            if (!valid)
            {
                _ghostEl.BorderBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0x50, 0x50));
                _ghostEl.Background  = (Brush)Application.Current.Resources["BackgroundCardBrush"];
                if (_ghostCopyBadge != null) _ghostCopyBadge.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateGhostCopyStyle();
            }
        }

        private void ShowInsertionLine(ItemsControl list, Point listPt, Point overlayPt)
        {
            if (_lineEl == null) return;
            _dropIndex = GetInsertionIndex(list, listPt);
            double lineY = GetInsertionY(list, _dropIndex);
            if (lineY < 0) { HideInsertionLine(); return; }
            var lineInOverlay = list.TranslatePoint(new Point(0, lineY), this);
            var listLeft      = list.TranslatePoint(new Point(0, 0), this);
            _lineEl.X1 = listLeft.X + 4;
            _lineEl.X2 = listLeft.X + list.ActualWidth - 4;
            _lineEl.Y1 = lineInOverlay.Y;
            _lineEl.Y2 = lineInOverlay.Y;
            _lineEl.Stroke = _dragIsCopy
                ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                : new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff));
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
            b.BorderBrush    = _dragIsCopy
                ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                : new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff));
            b.BorderThickness = new Thickness(1.5);
            b.Background     = new SolidColorBrush(_dragIsCopy
                ? Color.FromArgb(30, 0x30, 0xd1, 0x58)
                : Color.FromArgb(30, 0x0a, 0x84, 0xff));
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


        // ── Coordinate helpers ────────────────────────────────────────────

        private static int GetInsertionIndex(ItemsControl list, Point pt)
        {
            int count = list.Items.Count;
            if (count == 0) return 0;
            var first = list.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            if (first != null)
            {
                var top0 = first.TranslatePoint(new Point(), list).Y;
                if (pt.Y < top0 + first.ActualHeight / 2.0) return 0;
            }
            var last = list.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
            if (last != null)
            {
                var topN = last.TranslatePoint(new Point(), list).Y;
                if (pt.Y >= topN + last.ActualHeight / 2.0) return count;
            }
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
                var last = list.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
                if (last == null) return -1;
                return last.TranslatePoint(new Point(0, last.ActualHeight), list).Y;
            }
            if (index == 0)
            {
                var first = list.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                if (first == null) return -1;
                return first.TranslatePoint(new Point(), list).Y;
            }
            var above = list.ItemContainerGenerator.ContainerFromIndex(index - 1) as FrameworkElement;
            var below = list.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (above == null || below == null) return -1;
            double bot  = above.TranslatePoint(new Point(0, above.ActualHeight), list).Y;
            double top2 = below.TranslatePoint(new Point(), list).Y;
            return (bot + top2) / 2.0;
        }

        private static T? GetViewModelAtPoint<T>(ItemsControl list, Point pt) where T : class
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                var container = list.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                var origin = container.TranslatePoint(new Point(), list);
                if (new Rect(origin.X, origin.Y, container.ActualWidth, container.ActualHeight).Contains(pt))
                {
                    var vm = FindTagInSubtree<T>(container);
                    if (vm != null) return vm;
                }
            }
            return null;
        }

        private static T? FindTagInSubtree<T>(DependencyObject root) where T : class
        {
            if (root is FrameworkElement fe && fe.Tag is T vm) return vm;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var found = FindTagInSubtree<T>(VisualTreeHelper.GetChild(root, i));
                if (found != null) return found;
            }
            return null;
        }

        private static Border? FindRowBorderAtPoint(ItemsControl list, Point pt)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                var container = list.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                var origin = container.TranslatePoint(new Point(), list);
                if (new Rect(origin, container.RenderSize).Contains(pt))
                    return FindTaggedBorder(container);
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

        private ItemsControl? FindExpandedCategoryList()
        {
            if (_expandedGroups.Count == 0) return null;
            for (int i = 0; i < GroupsList.Items.Count; i++)
            {
                var vm = GroupsList.Items[i] as GroupViewModel;
                if (vm == null || !_expandedGroups.Contains(vm.Group)) continue;
                var container = GroupsList.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                var list = FindChildByName<ItemsControl>(container, "CategorySubList");
                if (list != null) return list;
            }
            return null;
        }

        private List<(ItemsControl List, CategoryGroup Group)> FindAllExpandedCategoryLists()
        {
            var result = new List<(ItemsControl, CategoryGroup)>();
            if (_expandedGroups.Count == 0) return result;
            for (int i = 0; i < GroupsList.Items.Count; i++)
            {
                var vm = GroupsList.Items[i] as GroupViewModel;
                if (vm == null || !_expandedGroups.Contains(vm.Group)) continue;
                var container = GroupsList.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                var list = FindChildByName<ItemsControl>(container, "CategorySubList");
                if (list != null) result.Add((list, vm.Group));
            }
            return result;
        }

        private static T? FindChildByName<T>(DependencyObject root, string name) where T : FrameworkElement
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


        // ── Name helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Strips any trailing " (N)" counter suffix from a name.
        /// "Head (2)" → "Head", "Head (2) (3)" → "Head"
        /// </summary>
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

        private static string MakeUniqueName(string rawName,
            System.Func<string, bool> nameExists)
        {
            string baseName = StripCounterSuffix(rawName);
            if (!nameExists(baseName)) return baseName;
            int counter = 2;
            while (nameExists($"{baseName} ({counter})")) counter++;
            return $"{baseName} ({counter})";
        }

        // ── Deep clone helpers ────────────────────────────────────────────

        private static CategoryGroup DeepCloneGroup(CategoryGroup src, string newName)
        {
            var grp = new CategoryGroup { Name = newName, IsEnabled = src.IsEnabled };
            foreach (var cat in src.Categories)
                grp.Categories.Add(DeepCloneCategory(cat, cat.Name));
            return grp;
        }

        private static Category DeepCloneCategory(Category src, string newName)
        {
            var cat = new Category { Name = newName, IsEnabled = src.IsEnabled };
            foreach (var opt in src.Options)
                cat.Options.Add(new Option { Name = opt.Name, Weight = opt.Weight, IsEnabled = opt.IsEnabled });
            return cat;
        }
    }


    // ════════════════════════════════════════════════════════════════════════
    // GROUP VIEW MODEL
    // ════════════════════════════════════════════════════════════════════════

    public class GroupViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public CategoryGroup           Group              { get; }
        public bool                    IsExpanded         { get; }
        public bool                    IsVisible          { get; }
        public bool                    IsSelected         { get; }
        public List<CategoryViewModel> CategoryViewModels { get; }

        public GroupViewModel(CategoryGroup group, bool isExpanded, bool isVisible,
                              List<CategoryViewModel> catVms, bool isSelected = false)
        {
            Group              = group;
            IsExpanded         = isExpanded;
            IsVisible          = isVisible;
            IsSelected         = isSelected;
            CategoryViewModels = catVms;
        }

        public string Name         => Group.Name;
        public string NameUpper    => Group.Name.ToUpper();
        public int    CategoryCount => Group.Categories.Count;
        public string Chevron      => IsExpanded ? "▼" : "▶";

        public bool IsEnabled
        {
            get => Group.IsEnabled;
            set { Group.IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public System.Windows.Visibility ExpandedVisibility =>
            IsExpanded ? System.Windows.Visibility.Visible
                       : System.Windows.Visibility.Collapsed;

        public double RowOpacity => IsVisible ? 1.0 : 0.35;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
    }
}
