using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Janus.Filters;
using JanusSharedLib;
using TextBox = System.Windows.Controls.TextBox;

namespace Janus
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow
    {
        private const ulong Delay = 5000;
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
            var errorMsg = new StringBuilder();
            if(!Directory.Exists(TxtDirectory.Text))
                errorMsg.AppendLine("Unable to find/access Watch directory.");
            if(!Directory.Exists(TxtOutDirectory.Text))
                errorMsg.AppendLine("Unable to find/access Out directory.");

            if (errorMsg.Length == 0)
            {
                var filters = new ObservableCollection<IFilter>();
                if (!string.IsNullOrEmpty(TxtFilterExclude.Text) && TxtFilterExclude.Text != "*")
                {
                    filters.Add(new ExcludeFilter(TxtFilterExclude.Text.SplitEscapable(';')));
                }
                if (!string.IsNullOrEmpty(TxtFilterInclude.Text) && TxtFilterInclude.Text != "*")
                {
                    filters.Add(new IncludeFilter(TxtFilterInclude.Text.SplitEscapable(';')));
                }

                ulong delay = 0;
                if (CbAddDelay.IsChecked.HasValue && CbAddDelay.IsChecked.Value)
                    delay = Delay;

                var watcher = new Watcher(
                    TxtName.Text,
                    TxtDirectory.Text,
                    TxtOutDirectory.Text,
                    CbAdd.IsChecked ?? false,
                    CbDelete.IsChecked ?? false,
                    filters,
                    CbRecurse.IsChecked ?? false,
                    delay);

                if (CbSaveSettings.IsChecked ?? false)
                {
                    UpdateLastData();
                }

                if (CbImmediate.IsChecked ?? false)
                {
                    // Don't want to move to BG thread here as later on
                    //  it's required to be UI thread to add watcher.
                    // Therefore: No ConfigureAwait(false).
                    await watcher.DoInitialSynchronise();
                }

                MainWindow.Watchers.Add(watcher);
                Logging.WriteLine(Properties.Resources.Debug_Added_Watcher);
                //NotificationSystem.Default.Push(NotifcationType.Info, "New Watcher", "Added a new watcher successfully.");

                Close();
            }
            else
            {
                Logging.WriteLine($"Error(s) encountered adding watcher:\n{errorMsg}\nWatch Dir: {TxtDirectory.Text}\nOut Dir: {TxtOutDirectory.Text}");
                NotificationSystem.Default.Push(NotifcationType.Error, "Error", $"Error(s) encountered adding watcher: <br/> {errorMsg}");
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
            MainWindow.Data["addDelay"] = CbAddDelay.IsChecked ?? false;
            MainWindow.Data["lastName"] = TxtName.Text;
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
            CbAddDelay.IsChecked = MainWindow.Data.GetOr("addDelay", CbAddDelay.IsChecked);
            TxtName.Text = MainWindow.Data.GetOr("lastName", TxtName.Text);
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
