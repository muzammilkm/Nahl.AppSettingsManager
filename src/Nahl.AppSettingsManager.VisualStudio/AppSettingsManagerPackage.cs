using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Nahl.AppSettingsManager.VisualStudio.Commands;
using Nahl.AppSettingsManager.VisualStudio.Extensions;
using Nahl.AppSettingsManager.VisualStudio.ToolWindows;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Nahl.AppSettingsManager.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", StringConstants.ProductVersion)]
    [Guid(GuidConstants.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(AppSettingsManagerToolWindow), Style = VsDockStyle.MDI, MultiInstances = false, Transient = true, DocumentLikeTool = true)]
    public sealed class AppSettingsManagerPackage : AsyncPackage
    {
        public static AppSettingsManagerPackage Instance { get; internal set; }

        public DTE2 DTE { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            DTE = await GetServiceAsync(typeof(DTE)) as DTE2;

            await ManageAppSettingsCommand.InitializeAsync(this);
            Logger.Initialize(this, "AppSettings Manager");
        }
    }
}
