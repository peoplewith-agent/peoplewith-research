using System;
using System.Collections.Generic;
using System.Timers;
//using Xam.Forms.VideoPlayer;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PeopleWithResearch
{
    public partial class WithdrawVideoPopUp : ContentPage
    {
        public System.Threading.Timer buttonTimer;

        public WithdrawVideoPopUp()
        {
            InitializeComponent();

         //   Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
          //  On<iOS>().SetUseSafeArea(false);


           // getwithdrawvideo();
           // InitializeTimer();
        }

        //private void InitializeTimer()
        //{
        //    try
        //    {
        //        buttonTimer = new Timer(15000); // 15 seconds
        //        buttonTimer.Elapsed += ButtonTimerElapsed;
        //        buttonTimer.AutoReset = false; // Set to false to run the timer only once

        //        // Start the timer when the video completes playback
        //        buttonTimer.Start();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //private void ButtonTimerElapsed(object sender, EventArgs e)
        //{
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        closebutton.IsVisible = true;

        //    });
        //}

        //async void getwithdrawvideo()
        //{
        //    try
        //    {

        //        // string imgPath = Constants.BlobURL + newimagename + ".mp4";


        //        string videolink = "https://peoplewithappiamges.blob.core.windows.net/appimages/appimages" + "/Iid3-Withdrawing_From_A_Study_In_App.mp4";

        //        UriVideoSource uriVideoSurce = new UriVideoSource()
        //        {
        //            Uri = videolink

        //        };

        //        videoPlayer.Source = uriVideoSurce;

        //        videoPlayer.Play();



        //    }
        //    catch (Exception ex)
        //    {
        //        videoPlayer.IsVisible = false;
        //        videoloader.IsVisible = false;
        //        somethingwrong.IsVisible = true;

        //    }
        //}

        //void videoPlayer_PropertyChanged(System.Object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //}

        //void videoPlayer_PlayCompletion(System.Object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        proceedbtn.IsVisible = true;

        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        //void Button_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    //close button
        //    Navigation.RemovePage(this);
        //}

        //void proceedbtn_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    //proceed button clicked

        //    try
        //    {
        //        Navigation.RemovePage(this);
        //        // Send a message to notify the first page
        //        MessagingCenter.Send(this, "VideoPageLeft");


        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //}

        //void btnwrong_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Navigation.RemovePage(this);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //void videoPlayer_PlayError(System.Object sender, Xam.Forms.VideoPlayer.VideoPlayer.PlayErrorEventArgs e)
        //{
        //    try
        //    {
        //        videoPlayer.IsVisible = false;
        //        videoloader.IsVisible = false;
        //        somethingwrong.IsVisible = true;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}

