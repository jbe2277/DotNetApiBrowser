namespace Waf.DotNetApiBrowser.Applications.DataModels
{
    public class AssemblyInfo
    {
        public AssemblyInfo(string fileName, string assemblyName, string assemblyApi)
        {
            FileName = fileName;
            AssemblyName = assemblyName;
            AssemblyApi = assemblyApi;
        }

        public string FileName { get; }

        public string AssemblyName { get; }
        
        public string AssemblyApi { get; }
    }
}
