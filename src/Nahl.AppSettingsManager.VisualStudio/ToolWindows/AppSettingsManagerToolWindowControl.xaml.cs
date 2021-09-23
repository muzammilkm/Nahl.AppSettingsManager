using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nahl.AppSettingsManager.VisualStudio.ToolWindows
{
    public partial class AppSettingsManagerToolWindowControl : UserControl
    {
        private readonly MainViewModel _viewModel;

        public AppSettingsManagerToolWindowControl()
        {
            this.InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            AppSettingsManagerPackage.Instance.JoinableTaskFactory.RunAsync(() =>
            {
                _viewModel.RefreshData(AppSettingsManagerPackage.Instance.DTE);

                Logger.Log("Ready");
                return Task.CompletedTask;
            });
        }

        private void cbProjList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProjList.SelectedItem != null)
                cbProjList.SelectedItem = null;
        }

        private void cbJsonFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbJsonFileList.SelectedItem != null)
                cbJsonFileList.SelectedItem = null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveVariables();
            AppSettingsManagerPackage.Instance.JoinableTaskFactory.RunAsync(() =>
            {
                _viewModel.RefreshData(AppSettingsManagerPackage.Instance.DTE);
                return Task.CompletedTask;
            });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();
        }
    }
}