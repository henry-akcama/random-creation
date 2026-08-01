using System.Collections.Generic;
using System.Linq;

namespace RandomCreation
{
    /// <summary>
    /// Internal clipboard for copy/cut/paste operations in Manage Content.
    /// Not the system clipboard — only used within the app.
    /// Cleared on Escape and app close.
    /// </summary>
    public static class ClipboardService
    {
        public enum ClipLevel { Option, Category, Group }
        public enum ClipMode  { None, Copy, Cut }

        // ── State ────────────────────────────────────────────────────────────

        public static ClipMode  Mode    { get; private set; } = ClipMode.None;
        public static ClipLevel Level   { get; private set; }
        public static bool      HasData => Mode != ClipMode.None;

        // Copied options
        private static readonly List<Option>        _options    = new();
        // Copied categories (with their owning group for cut operations)
        private static readonly List<(Category Cat, CategoryGroup Group)> _categories = new();
        // Copied groups (with their owning collection)
        private static readonly List<(CategoryGroup Group, Collection Col)> _groups  = new();

        // Source references — used to dim source items during cut and clear green outline after paste
        private static readonly List<Option>       _cutOptionsList    = new();
        public static IReadOnlyList<Option>       CutOptionItems    => _cutOptionsList;
        private static readonly List<Category>     _cutCategoriesList = new();
        public static IReadOnlyList<Category>     CutCategoryItems => _cutCategoriesList;
        private static readonly List<CategoryGroup> _cutGroupsList    = new();
        public static IReadOnlyList<CategoryGroup> CutGroupItems    => _cutGroupsList;

        // ── Copy ─────────────────────────────────────────────────────────────

        public static void CopyOptions(IEnumerable<Option> options)
        {
            Clear();
            Mode  = ClipMode.Copy;
            Level = ClipLevel.Option;
            _options.AddRange(options);
        }

        public static void CopyCategories(IEnumerable<(Category, CategoryGroup)> cats)
        {
            Clear();
            Mode  = ClipMode.Copy;
            Level = ClipLevel.Category;
            _categories.AddRange(cats);
        }

        public static void CopyGroups(IEnumerable<(CategoryGroup, Collection)> groups)
        {
            Clear();
            Mode  = ClipMode.Copy;
            Level = ClipLevel.Group;
            _groups.AddRange(groups);
        }

        // ── Cut ──────────────────────────────────────────────────────────────

        public static void CutOptions(IEnumerable<Option> options, IEnumerable<Option> sources)
        {
            Clear();
            Mode  = ClipMode.Cut;
            Level = ClipLevel.Option;
            _options.AddRange(options);
            _cutOptionsList.AddRange(sources);
        }

        public static void CutCategories(IEnumerable<(Category, CategoryGroup)> cats,
                                          IEnumerable<Category> sources)
        {
            Clear();
            Mode  = ClipMode.Cut;
            Level = ClipLevel.Category;
            _categories.AddRange(cats);
            _cutCategoriesList.AddRange(sources);
        }

        // ── Paste — returns deep copies ready to insert ───────────────────────

        /// <summary>Returns deep copies of the clipboard options.
        /// Applies counter suffix if any name collides in the target category.</summary>
        public static List<Option> PasteOptions(Category targetCat)
        {
            return _options.Select(o =>
            {
                string name = MakeUniqueOptionName(o.Name, targetCat);
                return new Option
                {
                    Name      = name,
                    Weight    = o.Weight,
                    IsEnabled = o.IsEnabled
                };
            }).ToList();
        }

        /// <summary>Returns deep copies of the clipboard categories.
        /// Applies counter suffix if any name collides in the target group.</summary>
        public static List<Category> PasteCategories(CategoryGroup targetGroup)
        {
            return _categories.Select(t =>
            {
                string name = MakeUniqueCategoryName(t.Cat.Name, targetGroup);
                var cat = new Category
                {
                    Name      = name,
                    IsEnabled = t.Cat.IsEnabled
                };
                foreach (var opt in t.Cat.Options)
                    cat.Options.Add(new Option
                    {
                        Name      = opt.Name,
                        Weight    = opt.Weight,
                        IsEnabled = opt.IsEnabled
                    });
                return cat;
            }).ToList();
        }

        /// <summary>Returns deep copies of the clipboard groups.
        /// Applies counter suffix if any name collides in the target collection.</summary>
        public static List<CategoryGroup> PasteGroups(Collection targetCol)
        {
            return _groups.Select(t =>
            {
                string name = MakeUniqueGroupName(t.Group.Name, targetCol);
                var grp = new CategoryGroup
                {
                    Name      = name,
                    IsEnabled = t.Group.IsEnabled
                };
                foreach (var cat in t.Group.Categories)
                {
                    var newCat = new Category
                    {
                        Name      = cat.Name,
                        IsEnabled = cat.IsEnabled
                    };
                    foreach (var opt in cat.Options)
                        newCat.Options.Add(new Option
                        {
                            Name      = opt.Name,
                            Weight    = opt.Weight,
                            IsEnabled = opt.IsEnabled
                        });
                    grp.Categories.Add(newCat);
                }
                return grp;
            }).ToList();
        }

        // ── Level info ────────────────────────────────────────────────────────

        public static int ItemCount => Level switch
        {
            ClipLevel.Option   => _options.Count,
            ClipLevel.Category => _categories.Count,
            ClipLevel.Group    => _groups.Count,
            _                  => 0
        };

        public static string FirstItemName => Level switch
        {
            ClipLevel.Option   => _options.Count  > 0 ? _options[0].Name       : "",
            ClipLevel.Category => _categories.Count > 0 ? _categories[0].Cat.Name : "",
            ClipLevel.Group    => _groups.Count    > 0 ? _groups[0].Group.Name  : "",
            _                  => ""
        };

        // ── Clear ─────────────────────────────────────────────────────────────

        public static void Clear()
        {
            Mode = ClipMode.None;
            _options.Clear();
            _categories.Clear();
            _groups.Clear();
            _cutOptionsList.Clear();
            _cutCategoriesList.Clear();
            _cutGroupsList.Clear();
        }

        // ── Name collision helpers ────────────────────────────────────────────

        private static string MakeUniqueOptionName(string baseName, Category cat)
        {
            if (!cat.Options.Any(o => o.Name == baseName)) return baseName;
            int counter = 2;
            while (cat.Options.Any(o => o.Name == $"{baseName} ({counter})")) counter++;
            return $"{baseName} ({counter})";
        }

        private static string MakeUniqueCategoryName(string baseName, CategoryGroup grp)
        {
            if (!grp.Categories.Any(c => c.Name == baseName)) return baseName;
            int counter = 2;
            while (grp.Categories.Any(c => c.Name == $"{baseName} ({counter})")) counter++;
            return $"{baseName} ({counter})";
        }

        private static string MakeUniqueGroupName(string baseName, Collection col)
        {
            if (!col.Groups.Any(g => g.Name == baseName)) return baseName;
            int counter = 2;
            while (col.Groups.Any(g => g.Name == $"{baseName} ({counter})")) counter++;
            return $"{baseName} ({counter})";
        }
    }
}
