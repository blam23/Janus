using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Janus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Watcher> Watchers = new ObservableCollection<Watcher>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnWatcher_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateWindow();
            createWindow.Init(this);
            createWindow.Show();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            listBox.ItemsSource = Watchers;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var watcher = btn.DataContext as Watcher;

            if (watcher == null) return;
            watcher.Stop();
            Watchers.Remove(watcher);
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var watcher = btn.DataContext as Watcher;

            watcher?.Synchronise();
        }
    }
}
