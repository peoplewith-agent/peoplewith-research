using Mopups.Pages;
using Mopups.Services;
namespace PeopleWithResearch;
public partial class PopupPageHelper : PopupPage
{
    private CancellationTokenSource heartbeatCts;
    private TaskCompletionSource<string> ReturnAcknowledged;

    //const double NormalScale = 1.0;
    //const double PrepScale = 0.80;
    //const double MaxScale = 1.25;
    //const uint ThumpDuration = 150;
    //const uint PauseDuration = 400;

    const double NormalScale = 1.0;
    const double PrepScale = 0.90;
    const double MaxScale = 1.25;
    const double MidScale = 1.10;

    const uint LubDuration = 150;
    const uint DubDuration = 100;
    const uint ReturnDuration = 200;
    const uint PauseDuration = 500;


    public PopupPageHelper()
    {
        InitializeComponent();
    }
    public PopupPageHelper(string message)
    {
        InitializeComponent();


        mainstack.IsVisible = true;
        switchstack.IsVisible = false;

        if (message != null)
        {
            if (message == "Updating...")
            {
                listloader.IsRunning = true;
                detaillbl2.IsVisible = true;
                img.IsVisible = false;
            }
            if (message == "Uploading Questionnaire...")
            {
                listloader.IsRunning = true;
                detaillbl2.IsVisible = true;
                img.IsVisible = false;
            }
            if (message.Contains("Loading"))
            {
                mainstack.IsVisible = false;
                switchstack.IsVisible = true;
                imgstack.IsVisible = false;
                // listloaderpp.IsVisible = true; 
                /// listloaderpp.IsRunning = true;
                detaillblpp.Text = message;
            }
        }
        detaillbl.Text = message;
    }

    public PopupPageHelper(bool RegtoDash, bool HHRep)
    {
        InitializeComponent();
        mainstack.IsVisible = false;
        switchstack.IsVisible = false;
        CreateAccountStack.IsVisible = true; 
        StartSetupSequence(HHRep);
    }

    private async void StartSetupSequence(bool HHRep)
    {
        string pronoun = HHRep ? "your" : "their";

        if (!HHRep)
        {
            BottomStacklbl.Text = "Welcome to the community. This account is ready for use.";
        }

        await UpdateStep($"Creating {pronoun} profile...", 30, 3000);
        await UpdateStep($"Securing {pronoun} data...", 65, 2500);
        await UpdateStep($"Finalising setup...", 100, 3100);


        await LoadingStack.FadeTo(0, 250);
        LoadingStack.IsVisible = false;

        FinsihedStack.IsVisible = true;
        await FinsihedStack.FadeTo(1, 400);

        LoadImage.IsAnimationPlaying = true;

        //await FinsihedStack.ScaleTo(1.05, 200, Easing.CubicOut);
        //await FinsihedStack.ScaleTo(1.0, 150, Easing.CubicIn);

        await Task.Delay(5000);
        await MopupService.Instance.PopAsync();
    }

    private async Task UpdateStep(string message, double progressValue, int delay)
    {
        loadinglbl.Text = message;
        CreateProgressbar.Progress = progressValue;
        await Task.Delay(delay);
    }

    public PopupPageHelper(string message, ImageSource pp, string userintials)
    {
        InitializeComponent();

        StartRefreshAnimation();

        mainstack.IsVisible = false;
        switchstack.IsVisible = true;

        if (message != null)
        {
            detaillblpp.Text = message;
        }

        if (pp != null)
        {
            ppsource.Source = pp;
            ppsource.IsVisible = true;
            ilbl.IsVisible = false;
        }
        else
        {
            ilbl.Text = userintials;
            ppsource.IsVisible = false;
            ilbl.IsVisible = true;
        }
    }

    public PopupPageHelper(TaskCompletionSource<String> ttc, string Message)
    {
        InitializeComponent();

        ReturnAcknowledged = ttc;
        mainstack.IsVisible = false;
        ConfirmationStack.IsVisible = true;

        if (Message != null)
        {
            Messagelbl.Text = Message;
        }

    }


    //public PopupPageHelper(string message, string userintials)
    //{
    //    InitializeComponent();

    //    //StartRefreshAnimation();

    //    mainstack.IsVisible = false;
    //    switchstack.IsVisible = true;
    //    ilbl.Text = userintials;
    //    ppsource.IsVisible = false;
    //    ilbl.IsVisible = true;

    //    if (message != null)
    //    {
    //        detaillblpp.Text = message;
    //    }
    //}


    public void UpdateMessage(string newMessage)
    {
        try
        {
            if (newMessage != null)
            {
                img.IsVisible = true;
                listloader.IsRunning = false;
                detaillbl2.IsVisible = false;
                detaillbl.Text = newMessage;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private async Task StartRefreshAnimation()
    {
        try
        {
            heartbeatCts?.Cancel();
            heartbeatCts = new CancellationTokenSource();
            var token = heartbeatCts.Token;

            imgstack.Scale = NormalScale;
            while (!token.IsCancellationRequested)
            {
                await imgstack.ScaleTo(PrepScale, LubDuration / 2, Easing.CubicIn);
                await imgstack.ScaleTo(MaxScale, LubDuration, Easing.CubicOut);
                await imgstack.ScaleTo(NormalScale, ReturnDuration, Easing.SinInOut);
                await imgstack.ScaleTo(PrepScale, DubDuration / 2, Easing.CubicIn);
                await imgstack.ScaleTo(MidScale, DubDuration, Easing.CubicOut);
                await imgstack.ScaleTo(NormalScale, ReturnDuration, Easing.SinInOut);
                await Task.Delay((int)PauseDuration, token);
            }
        }
        catch (TaskCanceledException)
        {

        }
        catch (Exception ex)
        {

        }
        finally
        {
            await imgstack.ScaleTo(NormalScale, 100, Easing.Linear);
            StopHeartbeat();
        }
    }


    //private async Task StartRefreshAnimation()
    //{
    //    try
    //    {
    //        // Cancel any existing animation
    //        heartbeatCts?.Cancel();
    //        heartbeatCts = new CancellationTokenSource();
    //        var token = heartbeatCts.Token;

    //        imgstack.Scale = NormalScale;

    //        // Continuous heartbeat loop
    //        while (!token.IsCancellationRequested)
    //        {
    //            // Gentle “contract”
    //            await imgstack.ScaleTo(PrepScale, ThumpDuration, Easing.CubicIn);
    //            // Strong “thump”
    //            await imgstack.ScaleTo(MaxScale, ThumpDuration, Easing.CubicOut);
    //            // Return to normal
    //            await imgstack.ScaleTo(NormalScale, ThumpDuration, Easing.SinInOut);

    //            // Short rest
    //            await Task.Delay((int)PauseDuration, token);
    //        }
    //    }
    //    catch (TaskCanceledException)
    //    {
    //        // Expected on stop
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Heartbeat animation error: {ex.Message}");
    //    }
    //    finally
    //    {
    //        await imgstack.ScaleTo(NormalScale, 100, Easing.Linear);
    //        StopHeartbeat();
    //    }
    //}


    private void StopHeartbeat()
    {
        heartbeatCts?.Cancel();
    }

    private async Task AnimateProfileSwitchAsync()
    {
        try
        {
            // First half: rotate to edge (1s)
            // First half: rotate to 90° (1s)
            await imgstack.RotateYTo(90, 500, Easing.CubicIn);

            // Hide the label while it's "edge-on"
            ilbl.Opacity = 0;
            ilbl.Text = "CF";
            ilbl.Opacity = 1;
            // Second half: continue rotation to 180° (1s)
            await imgstack.RotateYTo(180, 500, Easing.CubicOut);

            // Snap back to 0° so it looks normal next time
            imgstack.RotationY = 0;

            // Fade text back in quickly
            // await ilbl.FadeTo(1, 200, Easing.CubicIn);
        }
        catch (Exception ex)
        {

        }
    }

    private async void ConfirmNext_Clicked(object sender, EventArgs e)
    {
        try
        {
            ReturnAcknowledged.TrySetResult("Confirm");
            // Close Popup
            await MopupService.Instance.PopAsync();
        }
        catch (Exception Ex)
        {

        }

    }
}