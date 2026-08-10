namespace PeopleWithResearch;

public partial class pdfpage : ContentPage
{
    private MemoryStream memoryStream;
    public pdfpage()
	{
		InitializeComponent();
	}

    public pdfpage(Stream pdfStream)
    {
        InitializeComponent();
        if(DeviceInfo.Platform == DevicePlatform.iOS)
        {
            pdfViewer.LoadDocument(pdfStream);
        }
        else
        {
            memoryStream = new MemoryStream();
            pdfStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            pdfViewer.LoadDocument(memoryStream);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PopModalAsync();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
    }
}