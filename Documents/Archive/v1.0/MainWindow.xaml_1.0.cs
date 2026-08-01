using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace CreatureCrafter
{
    public partial class MainWindow : Window
    {
        // Save file sits next to the .exe
        private static readonly string SavePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "creature_crafter_data.json");

        public AppData Data { get; set; } = new AppData();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();

            // Defer first-launch navigation until the window and its children
            // are fully loaded so Window.GetWindow() works in UserControls
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Data.Categories.Count == 0)
                NavigateToEdit();   // First launch — go straight to edit screen
            else
                NavigateToMain();
        }

        // ── Persistence ──────────────────────────────────────────────────────

        private void LoadData()
        {
            if (!File.Exists(SavePath)) return;
            try
            {
                var json = File.ReadAllText(SavePath);
                var loaded = JsonSerializer.Deserialize<AppData>(json);
                if (loaded != null) Data = loaded;

                // Restore window geometry
                if (Data.WindowWidth >= 750) Width = Data.WindowWidth;
                if (Data.WindowHeight >= 550) Height = Data.WindowHeight;
                if (Data.WindowLeft >= 0) Left = Data.WindowLeft;
                if (Data.WindowTop >= 0) Top = Data.WindowTop;
            }
            catch { /* Corrupt or missing save — start fresh */ }
        }

        public void SaveData()
        {
            Data.WindowWidth = Width;
            Data.WindowHeight = Height;
            Data.WindowLeft = Left;
            Data.WindowTop = Top;

            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SavePath, JsonSerializer.Serialize(Data, opts));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
            base.OnClosing(e);
        }

        // ── Navigation ───────────────────────────────────────────────────────

        public void NavigateToEdit()
        {
            MainPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
            HistoryPanel.Visibility = Visibility.Collapsed;
            EditPanel.RefreshCategories();
        }

        public void NavigateToMain()
        {
            MainPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility = Visibility.Collapsed;
            RefreshMainScreen();
        }

        private void NavigateToHistory()
        {
            MainPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility = Visibility.Visible;
            HistoryPanel.RefreshHistory(Data.History);
        }

        // ── Main screen refresh ──────────────────────────────────────────────

        public void RefreshMainScreen()
        {
            // Show last generated result cards
            ResultsGrid.ItemsSource = Data.LastResult;

            // Timestamp below the result card
            if (Data.LastResultTime.HasValue)
            {
                var dt = Data.LastResultTime.Value;
                ResultTimestamp.Text = dt.Date == DateTime.Today
                    ? $"Generated today at {dt:h:mm tt}"
                    : $"Generated {dt:MMM d} at {dt:h:mm tt}";
                ResultTimestamp.Visibility = Visibility.Visible;
            }
            else
            {
                ResultTimestamp.Visibility = Visibility.Collapsed;
            }

            // Recent history — last 3, most recent first, with assigned dot colors
            var recent = Data.History.TakeLast(3).Reverse().ToList();
            HistoryColorHelper.AssignColors(recent);   // Fix: assign colors here too
            RecentHistoryList.ItemsSource = recent;
        }

        // ── Generate ─────────────────────────────────────────────────────────

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random();
            var result = new List<ResultPair>();

            foreach (var cat in Data.Categories)
            {
                if (!cat.IsEnabled || cat.Options.Count == 0) continue;

                // Build weighted pool: Normal=3 entries, Low=2, Rare=1
                var pool = new List<CreatureOption>();
                foreach (var opt in cat.Options)
                {
                    int weight = opt.Weight switch
                    {
                        WeightTier.Normal => 3,
                        WeightTier.Low => 2,
                        WeightTier.Rare => 1,
                        _ => 3
                    };
                    for (int i = 0; i < weight; i++) pool.Add(opt);
                }

                result.Add(new ResultPair
                {
                    Category = cat.Name,
                    Option = pool[rng.Next(pool.Count)].Name
                });
            }

            if (result.Count == 0)
            {
                MessageBox.Show(
                    "No enabled categories with options found.\nAdd some on the Edit screen.",
                    "Nothing to generate", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Data.LastResult = result;
            Data.LastResultTime = DateTime.Now;

            Data.History.Add(new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Result = result.ToList()
            });

            SaveData();
            RefreshMainScreen();
        }

        // ── Toolbar buttons ──────────────────────────────────────────────────

        private void EditButton_Click(object sender, RoutedEventArgs e) => NavigateToEdit();
        private void HistoryButton_Click(object sender, RoutedEventArgs e) => NavigateToHistory();

        private void RecentHistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
                new ResultDetailDialog(entry) { Owner = this }.ShowDialog();
        }
    }
}
