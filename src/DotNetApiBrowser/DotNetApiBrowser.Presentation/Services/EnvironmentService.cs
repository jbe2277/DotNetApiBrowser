using Microsoft.Win32;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public Task<(string path, string arguments)> GetDefaultDiffToolPathAsync()
        {
            return Task.Run(() =>
            {
                var subKeyNames = Registry.ClassesRoot.GetSubKeyNames().Where(x => x.StartsWith("VisualStudio.accessor.", StringComparison.OrdinalIgnoreCase));
                var pathList = subKeyNames.Select(x => GetDefaultDiffToolPathCore(x + @"\shell\Open\Command")).Where(x => !string.IsNullOrEmpty(x.path)).ToArray();
                var orderedPathList = pathList.Select(x => (path: x, ver: GetFileVersion(x.path))).Where(x => x.ver != null).Select(x => (path: x.path, ver: x.ver.Value))
                    .OrderBy(x => x.ver.major).ThenBy(x => x.ver.minor).ThenBy(x => x.ver.build).ThenBy(x => x.ver.privatePart);
                return orderedPathList.LastOrDefault().path;
            });
        }

        private static (string path, string arguments) GetDefaultDiffToolPathCore(string subKeyName)
        {
            using (var subKey = Registry.ClassesRoot.OpenSubKey(subKeyName))
            {
                var value = subKey?.GetValue("");
                if (value is string path && !string.IsNullOrEmpty(path))
                {
                    if (path.EndsWith(" /dde", StringComparison.OrdinalIgnoreCase)) path = path.Substring(0, path.Length - 5).Trim('"');
                    return (path, " /Diff");
                }
                return (null, null);
            }
        }

        private static (int major, int minor, int build, int privatePart)? GetFileVersion(string fileName)
        {
            try
            {
                var version = FileVersionInfo.GetVersionInfo(fileName);
                return (version.FileMajorPart, version.FileMinorPart, version.FileBuildPart, version.FilePrivatePart);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
