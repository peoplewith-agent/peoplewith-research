using System;
using Microsoft.Maui.Controls.Xaml;

namespace PeopleWithResearch
{
    /// <summary>
    /// XAML markup extension that resolves a localisation key at page-construction time
    /// using the currently persisted language (Helpers.Settings.SelectedLanguage).
    /// Usage: Text="{loc:Localize Key=My_ResourceKey}"
    /// Because pages are re-created on language change (App.SetMainPage), every
    /// {loc:Localize} binding is re-evaluated in the new language automatically.
    /// </summary>
    [ContentProperty(nameof(Key))]
    public class LocalizeExtension : IMarkupExtension<string>
    {
        public string Key { get; set; } = string.Empty;

        public string ProvideValue(IServiceProvider serviceProvider)
            => LocalizationManager.Get(Key);

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
            => ProvideValue(serviceProvider);
    }
}
