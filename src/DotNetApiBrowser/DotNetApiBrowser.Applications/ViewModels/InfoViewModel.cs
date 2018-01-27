using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Waf.Applications;
using System.Windows.Input;
using Microsoft.Win32;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class InfoViewModel : ViewModel<IInfoView>
    {
        private readonly DelegateCommand showWebsiteCommand;


        [ImportingConstructor]
        public InfoViewModel(IInfoView view)
            : base(view)
        {
            showWebsiteCommand = new DelegateCommand(ShowWebsite);
        }


        public ICommand ShowWebsiteCommand => showWebsiteCommand;

        public static string ProductName => ApplicationInfo.ProductName;

        public static string Version => ApplicationInfo.Version;

        public static string OSVersion => Environment.OSVersion.ToString();

        public static string NetVersion { get; } = GetDotNetVersion();

        public static bool Is64BitProcess => Environment.Is64BitProcess;


        public void ShowDialog(object owner)
        {
            ViewCore.ShowDialog(owner);
        }

        private void ShowWebsite(object parameter)
        {
            string url = (string)parameter;
            try
            {
                Process.Start(url);
            }
            catch (Exception e)
            {
                Trace.TraceError("An exception occured when trying to show the url '{0}'. Exception: {1}", url, e);
            }
        }

        private static string GetDotNetVersion()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                int? releaseKey = (int?)key?.GetValue("Release");
                string majorVersion = "";

                if (releaseKey > 461310) majorVersion = "4.7.1 or later";
                else if (releaseKey >= 461308) majorVersion = "4.7.1";
                else if (releaseKey >= 460798) majorVersion = "4.7";
                else if (releaseKey >= 394802) majorVersion = "4.6.2";
                else if (releaseKey >= 394254) majorVersion = "4.6.1";

                if (releaseKey != null) majorVersion += " (" + releaseKey + ")";
                return majorVersion;
            }
        }
    }
}
