namespace PeopleWithResearch;

public partial class LostConnectionPage : ContentPage
{
	public LostConnectionPage()
	{
		InitializeComponent();

        Shell.SetNavBarIsVisible(this, false);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;

            if (accessType == NetworkAccess.Internet)
            {
                await Navigation.PushAsync(new MainDashboard(), false);

                Navigation.RemovePage(this);
            }
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
    }
}