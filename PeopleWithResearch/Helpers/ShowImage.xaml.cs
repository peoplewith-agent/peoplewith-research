using Mopups.Services;

namespace PeopleWithResearch;

public partial class ShowImage : Mopups.Pages.PopupPage
{
    public Uri ImageUrl { get; set; }
    public ShowImage(Uri imageUrl)
    {
        InitializeComponent();
        ImageUrl = imageUrl;
        BindingContext = this; 
    }


    private async void ClosePopup_Tapped(object sender, EventArgs e)
    {
        if (MopupService.Instance.PopupStack.Count > 0)
        {
            await MopupService.Instance.PopAsync();
        }
    }
}