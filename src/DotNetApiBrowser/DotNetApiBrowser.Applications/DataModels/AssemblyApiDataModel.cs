namespace Waf.DotNetApiBrowser.Applications.DataModels
{
    public class AssemblyApiDataModel
    {
        public AssemblyApiDataModel(string assemblyName, string api)
        {
            AssemblyName = assemblyName;
            Api = api;
        }
        
        public string AssemblyName { get; }

        public string Api { get; }
    }
}
