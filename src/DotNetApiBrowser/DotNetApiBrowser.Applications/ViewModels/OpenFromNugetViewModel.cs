﻿using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class OpenFromNugetViewModel : ViewModel<IOpenFromNugetView>
    {
        private object contentView;

        [ImportingConstructor]
        public OpenFromNugetViewModel(IOpenFromNugetView view) : base(view)
        {
        }

        public ICommand BackCommand { get; set; }
        
        public ICommand NextCommand { get; set; }
        
        public object ContentView
        {
            get { return contentView; }
            set { SetProperty(ref contentView, value); }
        }

        public bool IsVisible { get; private set; }

        public async Task ShowDialogAsync(object ownerWindow)
        {
            IsVisible = true;
            await ViewCore.ShowDialogAsync(ownerWindow);
            IsVisible = false;
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}
