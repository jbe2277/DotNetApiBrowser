using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(ISelectPackageView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SelectPackageView : ISelectPackageView
    {
        public SelectPackageView()
        {
            InitializeComponent();
            Loaded += LoadedHandler;
        }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            searchTextBox.Focus();
        }

        private void SearchTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var binding = BindingOperations.GetBindingExpression(searchTextBox, TextBox.TextProperty);
                binding.UpdateSource();
            }
        }
    }
}
