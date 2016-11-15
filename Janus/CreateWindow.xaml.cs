using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;

namespace Janus
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window
    {
        private MainWindow _parent;

        public CreateWindow()
        {
            InitializeComponent();
        }

        public void Init(MainWindow parent)
        {
            _parent = parent;
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TxtDirectory.Text)
                && Directory.Exists(TxtOutDirectory.Text))
            {
                var watcher = new Watcher(
                    TxtDirectory.Text,
                    TxtOutDirectory.Text,
                    CbAdd.IsChecked ?? false,
                    CbDelete.IsChecked ?? false,
                    TxtFilter.Text,
                    CbRecurse.IsChecked ?? false);

                if (CbImmediate.IsChecked ?? false)
                {
                    await watcher.DoInitialSynchronise();
                }

                MainWindow.Watchers.Add(watcher);
                Console.WriteLine("Added new watcher.");
                Close();
            }
            else
            {
                Console.WriteLine("Invalid path!"); 
            }
        }



        private void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(TxtDirectory);
        }

        private void ShowFolderBrowser(TextBox tb)
        {
            var dialog = new FolderBrowserDialog {SelectedPath = tb.Text};
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tb.Text = dialog.SelectedPath;
                Unconfirm();
            }
        }

        private void Unconfirm()
        {
            CbConfirm.IsChecked = false;
            BtnAdd.IsEnabled = false;
        }

        private void btnBrowseOutDirectory_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(TxtOutDirectory);
        }

        private void cbConfirm_Checked(object sender, RoutedEventArgs e)
        {
            BtnAdd.IsEnabled = CbConfirm.IsChecked ?? false;
        }

        private void OptionChangeEvent(object sender, RoutedEventArgs e)
        {
            if (CbConfirm != null) Unconfirm();
        }
    }
}
