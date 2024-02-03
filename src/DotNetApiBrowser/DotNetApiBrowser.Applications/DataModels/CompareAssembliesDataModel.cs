using System.ComponentModel.DataAnnotations;

namespace Waf.DotNetApiBrowser.Applications.DataModels;

public class CompareAssembliesDataModel : ValidatableModel
{
    private AssemblyInfo selectedAssembly1;
    private AssemblyInfo selectedAssembly2;
    private string diffToolPath;
    private string diffToolArguments;

    public IReadOnlyList<AssemblyInfo> AvailableAssemblies { get; set; }

    public bool NotEnoughAvailableAssemblies => AvailableAssemblies?.Count < 2;

    [Required, Display(Name =  "Assembly 1")]
    public AssemblyInfo SelectedAssembly1
    {
        get => selectedAssembly1;
        set => SetPropertyAndValidate(ref selectedAssembly1, value);
    }

    [Required, Display(Name = "Assembly 2")]
    public AssemblyInfo SelectedAssembly2
    {
        get => selectedAssembly2;
        set => SetPropertyAndValidate(ref selectedAssembly2, value);
    }

    [Required, Display(Name = "Diff Tool Path")]
    public string DiffToolPath
    {
        get => diffToolPath;
        set => SetPropertyAndValidate(ref diffToolPath, value);
    }

    public string DiffToolArguments
    {
        get => diffToolArguments;
        set => SetPropertyAndValidate(ref diffToolArguments, value);
    }
}
