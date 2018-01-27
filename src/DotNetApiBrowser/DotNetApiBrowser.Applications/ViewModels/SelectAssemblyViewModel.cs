using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Compression;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SelectAssemblyViewModel : ViewModel<ISelectAssemblyView>
    {
        private IReadOnlyList<ZipArchiveEntry> assemblies;
        private ZipArchiveEntry selectedAssembly;

        [ImportingConstructor]
        public SelectAssemblyViewModel(ISelectAssemblyView view) : base(view)
        {
        }
        
        public IReadOnlyList<ZipArchiveEntry> Assemblies
        {
            get => assemblies;
            set => SetProperty(ref assemblies, value);
        }
        
        public ZipArchiveEntry SelectedAssembly
        {
            get => selectedAssembly;
            set => SetProperty(ref selectedAssembly, value);
        }
    }
}
