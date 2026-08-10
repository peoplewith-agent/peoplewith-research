using System.Collections.ObjectModel;
using System.Text.Json;

namespace PeopleWithResearch;

public partial class PrivacyPolicyPage : ContentPage
{

    public PrivacyPolicyPage()
    {
        InitializeComponent();
        LoadData();
    }

    private async Task<string> FetchJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("privacypolicy.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async void LoadData()
    {
        try
        {
            var json = await FetchJsonAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var policyData = JsonSerializer.Deserialize<PolicyDocumentRoot>(json, options);

                if (policyData != null)
                {
                    BindingContext = policyData;            
                    PrivacyCollection.ItemsSource = policyData.Sections;
                }
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadPrivacyPolicy");
        }
    }
}
