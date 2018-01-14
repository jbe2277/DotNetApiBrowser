namespace Waf.DotNetApiBrowser.Applications.Services
{
    public interface IEnvironmentService
    {
        string GetTempFileName();

        (string path, string arguments) GetDefaultDiffToolPath();
    }
}
