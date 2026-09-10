using System.Text.Json;

namespace PeopleWithResearch;

public partial class PrivacyPolicyPage : ContentPage
{
    public PrivacyPolicyPage()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private static async Task<string> FetchJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("privacypolicy.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var json = await FetchJsonAsync();
            if (string.IsNullOrWhiteSpace(json)) return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var policyData = JsonSerializer.Deserialize<PolicyDocumentRoot>(json, options);
            if (policyData is not null)
            {
                BindingContext = policyData; 
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, Navigation, "LoadPrivacyPolicy");
        }
    }
}