namespace Waf.DotNetApiBrowser.Applications.DataModels
{
    public class AssemblyInfo
    {
        public AssemblyInfo(string fileName, string assemblyName)
        {
            FileName = fileName;
            AssemblyName = assemblyName;
        }

        public string FileName { get; }

        public string AssemblyName { get; }
    }
}
