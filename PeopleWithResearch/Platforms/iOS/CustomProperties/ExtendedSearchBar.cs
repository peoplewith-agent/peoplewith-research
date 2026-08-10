using System;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Compatibility;
using PeopleWithResearch;
using UIKit;

[assembly: ExportRenderer(typeof(ExtSearchBar), typeof(ExtendedSearchBar))]
namespace PeopleWithResearch
{
    public class ExtendedSearchBar : SearchBarRenderer
    {
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0) && Control != null)
            {
                Control.SearchTextField.BackgroundColor = UIColor.FromRGB(237, 237, 238);
                Control.BarTintColor = UIColor.FromRGB(129, 129, 133);
                Control.TintColor = UIColor.FromRGB(0, 159, 227);
                UISearchBar searchbar = Control as UISearchBar;
                searchbar.SearchTextField.LeftView.TintColor = UIColor.FromRGB(129, 129, 133);
            }
            if (e.PropertyName == "Text")
            {
                Control.ShowsCancelButton = false;
            }
        }
    }
}