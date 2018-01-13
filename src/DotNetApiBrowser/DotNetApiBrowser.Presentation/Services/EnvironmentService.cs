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
    }
}
