using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RandomCreation
{
    /// <summary>
    /// Page-accurate print preview dialog.
    ///
    /// ONE SHARED BUILDER (BuildPages) produces the pages for both the preview
    /// and the printer, so they cannot drift apart — the v3.0 preview showed a
    /// scrolling area the printer silently clipped to one page, and reassured
    /// the developer about output it never checked. What is seen is what prints:
    /// real page breaks, a footer with page numbers on every page, cards kept
    /// whole across page boundaries.
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        private readonly HistoryEntry _entry;

        // Scheme D colours — readable on both colour and mono printers.
        // MetaColor darkened for v4.0 (was 170,170,170 — too light on paper).
        private static readonly Color CardHeaderBg     = Color.FromRgb(0xf0, 0xf4, 0xff);
        private static readonly Color CardHeaderAccent = Color.FromRgb(0x0a, 0x84, 0xff);
        private static readonly Color CardBorder       = Color.FromRgb(0xaa, 0xaa, 0xaa);
        private static readonly Color HeaderText       = Color.FromRgb(0x33, 0x33, 0x33);
        private static readonly Color CategoryColor    = Color.FromRgb(0x11, 0x11, 0x11);
        private static readonly Color OptionColor      = Color.FromRgb(0x00, 0x55, 0xcc);
        private static readonly Color MetaColor        = Color.FromRgb(0x55, 0x55, 0x55);

        /// <summary>Dimmed rows print at this opacity — an OPEN QUESTION per the
        /// v4.0 plan, to be settled by the developer looking at a real proof
        /// print. 40% was about as faint as the grey the header fix removed.</summary>
        private const double DimmedOpacity = 0.6;

        // Preview page metrics: US Letter at 96 DPI (8.5in × 11in), 0.5in
        // margins. The printer pass uses the printer's real printable area at
        // the same DIP scale, so preview and print measure identically.
        private static readonly Size      PreviewPageSize = new(816, 1056);
        private static readonly Thickness PageMargin      = new(48);

        private const double FooterHeight = 30;

        public PrintPreviewDialog(HistoryEntry entry)
        {
            InitializeComponent();
            _entry = entry;
            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
            BuildPreview();
        }

        private void BuildPreview()
        {
            PagesPanel.Children.Clear();
            foreach (var page in BuildPages(PreviewPageSize, PageMargin))
            {
                PagesPanel.Children.Add(new Border
                {
                    Background      = Brushes.White,
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
                    BorderThickness = new Thickness(1),
                    Width           = PreviewPageSize.Width,
                    Padding         = PageMargin,
                    Margin          = new Thickness(0, 0, 0, 16),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child           = page
                });
            }
        }


        // ── The shared page builder ──────────────────────────────────────────

        /// <summary>Builds the full set of pages for a given page size and
        /// margin: header on the first page, cards flowed two per row, a new
        /// page whenever the next row will not fit, and a footer with page
        /// numbers on EVERY page — a footer on page one is what says "of 3",
        /// which is how a missing sheet is noticed.</summary>
        private List<Grid> BuildPages(Size pageSize, Thickness margin)
        {
            double contentWidth  = pageSize.Width  - margin.Left - margin.Right;
            double contentHeight = pageSize.Height - margin.Top  - margin.Bottom;
            double availHeight   = contentHeight - FooterHeight;
            var    measureBox    = new Size(contentWidth, double.PositiveInfinity);

            // ── 1. Blocks to place: header first, then card rows ────────────
            var blocks = new List<FrameworkElement> { BuildHeaderBlock() };

            bool isFlat = _entry.Result.All(r => string.IsNullOrEmpty(r.GroupName));
            var cards = isFlat
                ? _entry.Result.Select(BuildPrintFlatCard).ToList()
                : _entry.Result
                    .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "" : p.GroupName)
                    .Select(g => BuildPrintGroupCard(g.Key, g.ToList()))
                    .ToList();

            // Two equal columns, cards paired into rows so a row is the unit
            // that moves to the next page — cards are never split.
            for (int i = 0; i < cards.Count; i += 2)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(cards[i], 0);
                row.Children.Add(cards[i]);
                if (i + 1 < cards.Count)
                {
                    Grid.SetColumn(cards[i + 1], 1);
                    row.Children.Add(cards[i + 1]);
                }
                blocks.Add(row);
            }

            // ── 2. Fill pages: measure each block, break when one won't fit ──
            var pagePanels = new List<StackPanel>();
            var current    = new StackPanel();
            double used    = 0;

            foreach (var block in blocks)
            {
                block.Measure(measureBox);
                double h = block.DesiredSize.Height;

                if (used > 0 && used + h > availHeight)
                {
                    pagePanels.Add(current);
                    current = new StackPanel();
                    used    = 0;
                }
                current.Children.Add(block);
                used += h;
            }
            pagePanels.Add(current);

            // ── 3. Wrap each panel in a fixed-height page grid with footer ──
            var pages = new List<Grid>();
            for (int i = 0; i < pagePanels.Count; i++)
            {
                var page = new Grid { Width = contentWidth, Height = contentHeight };
                page.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                page.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FooterHeight) });

                Grid.SetRow(pagePanels[i], 0);
                page.Children.Add(pagePanels[i]);

                string serialPart = _entry.Serial > 0 ? $" · {_entry.SerialDisplay}" : "";
                var footer = new TextBlock
                {
                    Text       = $"Random Creation{serialPart} · Page {i + 1} of {pagePanels.Count}",
                    Foreground = new SolidColorBrush(MetaColor),
                    FontSize   = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Bottom
                };
                Grid.SetRow(footer, 1);
                page.Children.Add(footer);

                pages.Add(page);
            }
            return pages;
        }

        /// <summary>Compact header — title and serial on one line, timestamp
        /// and summary sharing a second, then a separator. Two lines carrying
        /// more information than the old three.</summary>
        private StackPanel BuildHeaderBlock()
        {
            var header = new StackPanel();

            var titleRow = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var title = new TextBlock
            {
                Text       = "Random Creation",
                Foreground = new SolidColorBrush(HeaderText),
                FontSize   = 20, FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(title, 0);
            titleRow.Children.Add(title);

            if (_entry.Serial > 0)
            {
                var serial = new TextBlock
                {
                    Text       = _entry.SerialDisplay,
                    Foreground = new SolidColorBrush(HeaderText),
                    FontSize   = 20, FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                Grid.SetColumn(serial, 1);
                titleRow.Children.Add(serial);
            }
            header.Children.Add(titleRow);

            header.Children.Add(new TextBlock
            {
                Text       = $"{_entry.FullTimestamp} · {_entry.SummaryCore}",
                Foreground = new SolidColorBrush(MetaColor),
                FontSize   = 13,
                Margin     = new Thickness(0, 0, 0, 12)
            });

            header.Children.Add(new Border
            {
                Background = new SolidColorBrush(CardBorder),
                Height     = 1, Margin = new Thickness(0, 0, 0, 14)
            });

            return header;
        }


        // ── Cards (unchanged design; dimmed handling is CHANGE 3) ────────────

        private static Border BuildPrintFlatCard(ResultPair pair)
        {
            var card = new Border
            {
                BorderBrush     = new SolidColorBrush(CardBorder),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4),
                Margin          = new Thickness(4),
                Padding         = new Thickness(12, 10, 12, 10),
                VerticalAlignment = VerticalAlignment.Top,
                Opacity         = pair.IsDimmed ? DimmedOpacity : 1.0
            };
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = pair.Category, FontSize = 16,
                Foreground = new SolidColorBrush(CategoryColor),
                TextWrapping = TextWrapping.Wrap
            });
            sp.Children.Add(new TextBlock
            {
                Text = pair.Option, FontSize = 18,
                Foreground = new SolidColorBrush(OptionColor),
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
            card.Child = sp;
            return card;
        }

        private static Border BuildPrintGroupCard(string groupName, List<ResultPair> pairs)
        {
            var card = new Border
            {
                BorderBrush     = new SolidColorBrush(CardBorder),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4),
                Margin          = new Thickness(4),
                VerticalAlignment = VerticalAlignment.Top,
                ClipToBounds    = true
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            if (!string.IsNullOrEmpty(groupName))
            {
                var header = new Border
                {
                    Background      = new SolidColorBrush(CardHeaderBg),
                    BorderBrush     = new SolidColorBrush(CardHeaderAccent),
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding         = new Thickness(10, 8, 10, 8)
                };
                Grid.SetRow(header, 0);
                header.Child = new TextBlock
                {
                    Text       = groupName,
                    Foreground = new SolidColorBrush(HeaderText),
                    FontSize   = 16, FontWeight = FontWeights.SemiBold
                };
                grid.Children.Add(header);
            }

            var body = new StackPanel { Margin = new Thickness(6, 5, 6, 5) };
            Grid.SetRow(body, 1);

            // CHANGE 3: dimmed rows are included at reduced opacity — grouped
            // cards adopt the flat-card behaviour. Excluding them was drift:
            // dimmed items were always meant to print.
            foreach (var pair in pairs)
            {
                var rowGrid = new Grid
                {
                    Margin  = new Thickness(0, 1, 0, 1),
                    Opacity = pair.IsDimmed ? DimmedOpacity : 1.0
                };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(1, GridUnitType.Star) });

                var catTb = new TextBlock
                {
                    Text = pair.Category, FontSize = 16,
                    Foreground = new SolidColorBrush(CategoryColor),
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(catTb, 0);

                var optTb = new TextBlock
                {
                    Text = pair.Option, FontSize = 16,
                    Foreground = new SolidColorBrush(OptionColor),
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(optTb, 1);

                rowGrid.Children.Add(catTb);
                rowGrid.Children.Add(optTb);
                body.Children.Add(rowGrid);
            }

            grid.Children.Add(body);
            card.Child = grid;
            return card;
        }


        // ── Buttons ──────────────────────────────────────────────────────────

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() != true) return;

            var pageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
            var doc      = new FixedDocument();
            doc.DocumentPaginator.PageSize = pageSize;

            // The same builder the preview used, at the printer's page size.
            foreach (var pageGrid in BuildPages(pageSize, PageMargin))
            {
                var fixedPage = new FixedPage
                {
                    Width  = pageSize.Width,
                    Height = pageSize.Height
                };
                FixedPage.SetLeft(pageGrid, PageMargin.Left);
                FixedPage.SetTop(pageGrid, PageMargin.Top);
                fixedPage.Children.Add(pageGrid);

                var pageContent = new PageContent();
                ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                doc.Pages.Add(pageContent);
            }

            dlg.PrintDocument(doc.DocumentPaginator, "Random Creation Result");
            Close();
        }
    }
}
