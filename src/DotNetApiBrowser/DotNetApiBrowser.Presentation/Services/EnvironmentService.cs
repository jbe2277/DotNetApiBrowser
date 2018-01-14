using Microsoft.Win32;
using System;
using System.ComponentModel.Composition;
using System.IO;
using Waf.DotNetApiBrowser.Applications.Services;

namespace Waf.DotNetApiBrowser.Presentation.Services
{
    [Export(typeof(IEnvironmentService))]
    internal class EnvironmentService : IEnvironmentService
    {
        public string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), "DotNetApiBrowser" + Guid.NewGuid() + ".cs");
        }

        public (string path, string arguments) GetDefaultDiffToolPath()
        {
            var value = Registry.ClassesRoot.OpenSubKey(@"VisualStudio.accessor.e36df4e1\shell\Open\Command")?.GetValue("");
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(" /dde", StringComparison.OrdinalIgnoreCase)) path = path.Substring(0, path.Length - 5);
                return (path, " /Diff");
            }
            return (null, null);
        }
    }
}
