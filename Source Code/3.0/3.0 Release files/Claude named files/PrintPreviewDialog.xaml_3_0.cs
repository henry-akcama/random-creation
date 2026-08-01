using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RandomCreation
{
    /// <summary>
    /// Fixed-size print preview dialog.
    /// Shows a white paper-proportioned preview of the result using Scheme D colours.
    /// Print… opens the standard Windows PrintDialog and builds a FixedDocument.
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        private readonly HistoryEntry _entry;

        // Scheme D colours — readable on both colour and mono printers
        private static readonly System.Windows.Media.Color CardHeaderBg    = Color.FromRgb(0xf0, 0xf4, 0xff);
        private static readonly System.Windows.Media.Color CardHeaderAccent = Color.FromRgb(0x0a, 0x84, 0xff);
        private static readonly System.Windows.Media.Color CardBorder       = Color.FromRgb(0xaa, 0xaa, 0xaa);
        private static readonly System.Windows.Media.Color HeaderText       = Color.FromRgb(0x33, 0x33, 0x33);
        private static readonly System.Windows.Media.Color CategoryColor    = Color.FromRgb(0x11, 0x11, 0x11);
        private static readonly System.Windows.Media.Color OptionColor      = Color.FromRgb(0x00, 0x55, 0xcc);
        private static readonly System.Windows.Media.Color MetaColor        = Color.FromRgb(0xaa, 0xaa, 0xaa);

        public PrintPreviewDialog(HistoryEntry entry)
        {
            InitializeComponent();
            _entry = entry;
            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
            BuildPreview();
        }

        private void BuildPreview()
        {
            PrintContent.Children.Clear();

            // App name + timestamp header
            PrintContent.Children.Add(new TextBlock
            {
                Text       = "Random Creation",
                Foreground = new SolidColorBrush(HeaderText),
                FontSize   = 20, FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 4)
            });
            PrintContent.Children.Add(new TextBlock
            {
                Text       = _entry.FullTimestamp,
                Foreground = new SolidColorBrush(MetaColor),
                FontSize   = 14, Margin = new Thickness(0, 0, 0, 4)
            });
            PrintContent.Children.Add(new TextBlock
            {
                Text       = _entry.Summary,
                Foreground = new SolidColorBrush(MetaColor),
                FontSize   = 14, Margin = new Thickness(0, 0, 0, 20)
            });

            // Separator
            PrintContent.Children.Add(new Border
            {
                Background = new SolidColorBrush(CardBorder),
                Height     = 1, Margin = new Thickness(0, 0, 0, 14)
            });

            // Group cards in a WrapPanel — 4 columns
            bool isFlat = _entry.Result.All(r => string.IsNullOrEmpty(r.GroupName));

            var wrap = new UniformGrid { Columns = 2 };

            if (isFlat)
            {
                foreach (var pair in _entry.Result)
                    wrap.Children.Add(BuildPrintFlatCard(pair));
            }
            else
            {
                var groups = _entry.Result
                    .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "" : p.GroupName);
                foreach (var grp in groups)
                    wrap.Children.Add(BuildPrintGroupCard(grp.Key, grp.ToList()));
            }

            PrintContent.Children.Add(wrap);
        }

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
                Opacity         = pair.IsDimmed ? 0.4 : 1.0
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

        private static Border BuildPrintGroupCard(string groupName,
            System.Collections.Generic.List<ResultPair> pairs)
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

            foreach (var pair in pairs.Where(p => !p.IsDimmed))
            {
                var rowGrid = new Grid { Margin = new Thickness(0, 1, 0, 1) };
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

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() != true) return;

            // Build a FixedDocument for printing
            var doc      = new FixedDocument();
            var pageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);

            doc.DocumentPaginator.PageSize = pageSize;

            var page    = new FixedPage { Width = pageSize.Width, Height = pageSize.Height };
            var content = new ScrollViewer
            {
                Width  = pageSize.Width - 96,
                Height = pageSize.Height - 96,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility   = ScrollBarVisibility.Disabled
            };

            // Clone the preview content for the print page
            var printPanel = new StackPanel { Width = pageSize.Width - 96 };
            BuildPreviewInto(printPanel, scale: 1.3);
            content.Content = printPanel;

            FixedPage.SetLeft(content, 48);
            FixedPage.SetTop(content, 48);
            page.Children.Add(content);

            var pageContent = new PageContent();
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
            doc.Pages.Add(pageContent);

            dlg.PrintDocument(doc.DocumentPaginator, "Random Creation Result");
            Close();
        }

        private void BuildPreviewInto(StackPanel panel, double scale)
        {
            panel.Children.Add(new TextBlock
            {
                Text       = "Random Creation",
                Foreground = new SolidColorBrush(HeaderText),
                FontSize   = 16 * scale, FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 2)
            });
            panel.Children.Add(new TextBlock
            {
                Text       = _entry.FullTimestamp,
                Foreground = new SolidColorBrush(MetaColor),
                FontSize   = 10 * scale, Margin = new Thickness(0, 0, 0, 2)
            });
            panel.Children.Add(new TextBlock
            {
                Text       = _entry.Summary,
                Foreground = new SolidColorBrush(MetaColor),
                FontSize   = 10 * scale, Margin = new Thickness(0, 0, 0, 14)
            });
            panel.Children.Add(new Border
            {
                Background = new SolidColorBrush(CardBorder),
                Height     = 1, Margin = new Thickness(0, 0, 0, 14)
            });

            var wrap = new UniformGrid { Columns = 2 };
            var isFlat = _entry.Result.All(r => string.IsNullOrEmpty(r.GroupName));

            if (isFlat)
            {
                foreach (var pair in _entry.Result)
                    wrap.Children.Add(BuildPrintFlatCard(pair));
            }
            else
            {
                var groups = _entry.Result
                    .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "" : p.GroupName);
                foreach (var grp in groups)
                    wrap.Children.Add(BuildPrintGroupCard(grp.Key, grp.ToList()));
            }

            panel.Children.Add(wrap);
        }
    }
}
