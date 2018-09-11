using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Janus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DataStore MainStore = new DataStore();
        public static bool Exiting { get; private set; }
        public static DataProvider Data;
        public static ObservableCollection<Watcher> Watchers;
        private static readonly string Startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        private static readonly string Shortcut = Path.Combine(Startup, "Janus.url");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnWatcher_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateWindow();
            createWindow.Init();
            createWindow.Show();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Logging.WriteLine("Initialising Main Window");
            Hide();
            MainStore.Initialise();

            Logging.WriteLine("Loading Data");
            var d = MainStore.Load();
            Logging.WriteLine("Finished Loading Data");

            Data = d.DataProvider;
            Watchers = d.Watchers;

            if (Watchers.Count == 0)
            {
                Show();
            }
            else
            {
                NotificationSystem.Default.Push(NotifcationType.Info, "Janus", "Started in minimised mode. Double click 'Ja' icon to interact.");
            }
            ListBox.ItemsSource = Watchers;
            Watchers.CollectionChanged += Watchers_CollectionChanged;

            CbStartup.IsChecked = File.Exists(Shortcut);
        }

        private static void Watchers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateStore();
        }

        public static void UpdateStore()
        {
            Logging.WriteLine("Saving Data");
            MainStore.Store(new JanusData(Watchers, Data));
            Logging.WriteLine("Finished Saving Data");
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn?.DataContext is Watcher watcher)
            {
                watcher.Stop();
                Watchers.Remove(watcher);
            }

            //NotificationSystem.Default.Push(NotifcationType.Info, "Removed Watcher", "Removed watcher successfully.");
        }

        private async void btnSync_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is Watcher watcher)
                await watcher.SynchroniseAsync().ConfigureAwait(false);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is Watcher watcher)
                UpdateStore();
        }

        private void CbStartup_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CbStartup.IsChecked.HasValue) return;

            if (CbStartup.IsChecked.Value)
            {
                AddToStartup();
            }
            else
            {
                RemoveFromStartup();
            }
        }

        private static void RemoveFromStartup()
        {
            if (File.Exists(Shortcut))
            {
                File.Delete(Shortcut);
            }
        }

        private static void AddToStartup()
        {
            using (var writer = new StreamWriter(Shortcut))
            {
                var app = Assembly.GetExecutingAssembly().Location;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + app);
                writer.WriteLine("IconIndex=0");
                var icon = app.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
            }
        }

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (Exiting) return;

            e.Cancel = true;
            Hide();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Exiting) return;

            Show();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Exiting = true;
            Close();
        }


    }
}
