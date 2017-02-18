using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Janus.Filters;
using TextBox = System.Windows.Controls.TextBox;

namespace Janus
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow
    {
        public CreateWindow()
        {
            InitializeComponent();
        }

        public void Init()
        {
            GetLastData();
        }



        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TxtDirectory.Text)
                && Directory.Exists(TxtOutDirectory.Text))
            {
                var filters = new List<IFilter>();
                if (!string.IsNullOrEmpty(TxtFilterExclude.Text))
                {
                    filters.Add(new ExcludeFilter(TxtFilterExclude.Text.SplitEscapable(';')));
                }
                if (!string.IsNullOrEmpty(TxtFilterInclude.Text))
                {
                    filters.Add(new IncludeFilter(TxtFilterInclude.Text.SplitEscapable(';')));
                }

                var watcher = new Watcher(
                    TxtDirectory.Text,
                    TxtOutDirectory.Text,
                    CbAdd.IsChecked ?? false,
                    CbDelete.IsChecked ?? false,
                    filters,
                    CbRecurse.IsChecked ?? false);


                if (CbImmediate.IsChecked ?? false)
                {
                    await watcher.DoInitialSynchronise();
                }

                UpdateLastData();
                MainWindow.Watchers.Add(watcher);
                Debug.WriteLine(Properties.Resources.Debug_Added_Watcher);
                NotificationSystem.Default.Push(NotifcationType.Info, "New Watcher", "Added a new watcher successfully.");


                Close();
            }
            else
            {
                Debug.WriteLine(Properties.Resources.Debug_Invalid_Path);
            }
        }

        private void UpdateLastData()
        {
            MainWindow.Data["lastDirectory"] = TxtDirectory.Text;
            MainWindow.Data["lastOutDirectory"] = TxtOutDirectory.Text;
            MainWindow.Data["lastAddOnFile"] = CbAdd.IsChecked ?? false;
            MainWindow.Data["lastDeleteOnFile"] = CbDelete.IsChecked ?? false;
            MainWindow.Data["lastIncFilter"] = TxtFilterInclude.Text;
            MainWindow.Data["lastExcFilter"] = TxtFilterExclude.Text;
            MainWindow.Data["lastRecurse"] = CbRecurse.IsChecked ?? false;
        }

        private void GetLastData()
        {
            TxtDirectory.Text = MainWindow.Data.GetOr("lastDirectory", TxtDirectory.Text);
            TxtOutDirectory.Text = MainWindow.Data.GetOr("lastOutDirectory", TxtOutDirectory.Text);
            CbAdd.IsChecked = MainWindow.Data.GetOr("lastAddOnFile", CbAdd.IsChecked);
            CbDelete.IsChecked = MainWindow.Data.GetOr("lastDeleteOnFile", CbDelete.IsChecked);
            TxtFilterInclude.Text = MainWindow.Data.GetOr("lastIncFilter", TxtFilterInclude.Text);
            TxtFilterExclude.Text = MainWindow.Data.GetOr("lastExcFilter", TxtFilterExclude.Text);
            CbRecurse.IsChecked = MainWindow.Data.GetOr("lastRecurse", CbRecurse.IsChecked);
        }


        private void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(TxtDirectory);
        }

        private void ShowFolderBrowser(TextBox tb)
        {
            using (var dialog = new FolderBrowserDialog {SelectedPath = tb.Text})
            {
                var result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK) return;
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
