using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Waf.DotNetApiBrowser.Applications.ViewModels;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(IShellView))]
    public partial class ShellWindow : IShellView
    {
        private readonly Lazy<ShellViewModel> viewModel;

        public ShellWindow()
        {
            InitializeComponent();
            viewModel = new Lazy<ShellViewModel>(() => ViewHelper.GetViewModel<ShellViewModel>(this));
            Loaded += LoadedHandler;
        }

        private ShellViewModel ViewModel => viewModel.Value;

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsApplicationBusy))
            {
                if (ViewModel.IsApplicationBusy)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }
                else
                {
                    // Delay removing the wait cursor so that the UI has finished its work as well.
                    Dispatcher.InvokeAsync(() => Mouse.OverrideCursor = null, DispatcherPriority.ApplicationIdle);
                }
            }
        }
    }
}
