using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nahl.AppSettingManager.VisualStudio.Extensions;
using Nahl.AppSettingManager.VisualStudio.ToolWindows;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nahl.AppSettingManager.VisualStudio.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ManageAppSettingCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageAppSettingCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="msc">Command service to add command to, not null.</param>
        private ManageAppSettingCommand(AsyncPackage package, OleMenuCommandService msc)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            msc = msc ?? throw new ArgumentNullException(nameof(msc));

            var manageAppSettingsForSolutionCommandId = new CommandID(GuidConstants.CommandSetGuid, IntConstants.ManageAppSettingCommandId);
            var menuItem = new OleMenuCommand(ShowManageAppSettingForSolutionWindow, manageAppSettingsForSolutionCommandId);
            msc.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ManageAppSettingCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var msc = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (msc == null)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            Instance = new ManageAppSettingCommand(package, msc);
        }

        private void ShowManageAppSettingForSolutionWindow(object sender, EventArgs e)
        {
            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                var window = await this.package.ShowToolWindowAsync(typeof(AppSettingManagerToolWindow), 0, true, this.package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            });
        }
    }
}
