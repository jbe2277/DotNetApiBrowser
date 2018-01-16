using System.Threading.Tasks;

namespace Waf.DotNetApiBrowser.Applications.Services
{
    public interface IEnvironmentService
    {
        string GetTempFileName();

        Task<(string path, string arguments)> GetDefaultDiffToolPathAsync();
    }
}
