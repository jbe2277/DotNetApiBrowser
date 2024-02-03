﻿using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views;

[Export(typeof(IInfoView)), PartCreationPolicy(CreationPolicy.NonShared)]
public partial class InfoWindow : IInfoView
{
    public InfoWindow()
    {
        InitializeComponent();
        using (Stream stream = Application.GetResourceStream(new Uri("/Resources/Images/Application.ico", UriKind.Relative)).Stream)
        {
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
            BitmapFrame frame = decoder.Frames.Where(f => f.Width < 300).OrderBy(f => f.Width).LastOrDefault();
            applicationImage.Source = frame;
        }
    }

    public void ShowDialog(object owner)
    {
        Owner = owner as Window;
        ShowDialog();
    }
}
