using System;

namespace Waf.DotNetApiBrowser.Applications.DataModels
{
    public class AssemblyInfo
    {
        public AssemblyInfo(string fileName, string assemblyName, Version version, string assemblyApi)
        {
            FileName = fileName;
            AssemblyName = assemblyName;
            Version = version;
            AssemblyApi = assemblyApi;
        }

        public string FileName { get; }

        public string AssemblyName { get; }
        
        public Version Version { get; }

        public string AssemblyApi { get; }
    }
}
