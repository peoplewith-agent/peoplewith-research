using System;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using PeopleWithResearch;
using PeopleWithResearch.iOS;
using UIKit;
[assembly: ExportRenderer(typeof(HideKeyboardEntry), typeof(HideKeyboardEntryRenderer))]
namespace PeopleWithResearch.iOS
{
    public class HideKeyboardEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            try
            {
                base.OnElementChanged(e);
                // Disabling the keyboard
                this.Control.InputView = new UIView();
            }
            catch
            {
            }
        }
    }
}