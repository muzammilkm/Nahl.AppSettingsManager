using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nahl.AppSettingManager.VisualStudio.ToolWindows
{
    public partial class AppSettingManagerToolWindowControl : UserControl
    {
        private readonly MainViewModel _viewModel;

        public AppSettingManagerToolWindowControl()
        {
            this.InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            AppSettingManagerPackage.Instance.JoinableTaskFactory.RunAsync(() =>
            {
                _viewModel.RefreshData(AppSettingManagerPackage.Instance.DTE);
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
            AppSettingManagerPackage.Instance.JoinableTaskFactory.RunAsync(() =>
            {
                _viewModel.RefreshData(AppSettingManagerPackage.Instance.DTE);
                return Task.CompletedTask;
            });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();
        }
    }
}