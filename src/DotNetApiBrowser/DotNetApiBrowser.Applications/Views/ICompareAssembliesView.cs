﻿using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views;

public interface ICompareAssembliesView : IView
{
    Task ShowDialogAsync(object ownerWindow);

    void Close();
}
