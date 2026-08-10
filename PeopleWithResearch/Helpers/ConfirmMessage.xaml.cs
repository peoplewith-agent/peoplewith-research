using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommunityToolkit.Maui.Core;
using ExCSS;
using FreakyKit.Utils;
using Maui.FreakyControls;
using Mopups.Pages;
using Mopups.Services;
using Syncfusion.Maui.Graphics.Internals;
using Syncfusion.Maui.ProgressBar;
using System;
using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace PeopleWithResearch;

public partial class ConfirmMessage : PopupPage
{
    public ObservableCollection<confirmationmessage> ConfirmData { get; set; } = new();

    private readonly TaskCompletionSource<string> _returnItem;
    private CancellationTokenSource? _cameraCts;
    private readonly string _imageFileName = string.Empty;

    public Stream? ImageStream { get; set; }
    public MemoryStream? ImageMemoryStream { get; set; }

    public ConfirmMessage(ObservableCollection<confirmationmessage> passed, TaskCompletionSource<string> itemToReturn)
    {
        InitializeComponent();
        _returnItem = itemToReturn;
        ConfirmData = passed;
        MainCollectionView.ItemsSource = ConfirmData;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cameraCts?.Cancel();
        _cameraCts?.Dispose();
        ImageMemoryStream?.Dispose();
        ImageStream?.Dispose();
    }

    private async Task ClosePopup()
    {
        try
        {
            var dataItem = ConfirmData.FirstOrDefault();
            if (dataItem != null)
            {
                var setReturn = string.Empty;

                if (dataItem.action == "image-upload")
                {
                    var random = new Random();
                    int randomId = random.Next(100000, 99999999);
                    var fileName = $"testresults/{Helpers.Settings.UsersID}_{DateTime.Now:yyyyMMdd}_b1_samples_{randomId}.png";

                    await UploadToBlobStorage(fileName);
                    setReturn = fileName;
                }
                else if (dataItem.action == "complete" || dataItem.action == "logout")
                {
                    setReturn = dataItem.action;
                }

                _returnItem.TrySetResult(setReturn);
            }

            await MopupService.Instance.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing popup: {ex}");
        }
    }

    private async Task UploadToBlobStorage(string fileName)
    {
        try
        {
            if (ImageMemoryStream == null) return;

            var blobServiceClient = new BlobServiceClient(APICalls.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("imperial");
            var blobClient = containerClient.GetBlobClient(fileName);

            ImageMemoryStream.Position = 0;

            await blobClient.UploadAsync(ImageMemoryStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Blob upload failed: {ex}");
            // Optional: Alert user that upload failed.
        }
    }

    private async void OpenCamera_Clicked(object sender, EventArgs e)
    {
        try
        {
            var data = ConfirmData.FirstOrDefault();
            if (data == null) return;

            if (data.Picturebtn == "Change photo")
            {
                await OpenGallery(data);
            }
            else
            {
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();

                if (cameraStatus != PermissionStatus.Granted)
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

                if (micStatus != PermissionStatus.Granted)
                    micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

                if (cameraStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted)
                {
                    MainCollectionView.IsVisible = false;
                    CameraStack.IsVisible = true;

                    _cameraCts?.Cancel();
                    _cameraCts?.Dispose();
                    _cameraCts = new CancellationTokenSource();

                    var cameras = await cameraView.GetAvailableCameras(_cameraCts.Token);
                    cameraView.SelectedCamera = cameras.FirstOrDefault();
                }
                else
                {
                    await DisplayAlert("Permission Denied", "Camera access is required to take a photo.", "OK");
                    await ClosePopup();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OpenCamera_Clicked: {ex}");
        }
    }

    private async void OpenGallery_Clicked(object sender, EventArgs e)
    {
        try
        {
            var data = ConfirmData.FirstOrDefault();
            if (data == null) return;

            if (data.Completebtn != "Upload from Gallery")
            {
                await ClosePopup();
            }
            else
            {
                await OpenGallery(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening gallery: {ex}");
        }
    }

    private async void logout_Clicked(object sender, EventArgs e)
    {
        await ClosePopup();
    }

    private async void Complete_Clicked(object sender, EventArgs e)
    {
        await ClosePopup();
    }

    private async void CaptureButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var data = ConfirmData.FirstOrDefault();
            if (data == null) return;

            await OpenCamera();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error capturing image: {ex}");
        }
    }

    private async Task OpenGallery(confirmationmessage data)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;

            // Safe disposal of old stream data before reallocation
            ImageMemoryStream?.Dispose();
            ImageMemoryStream = new MemoryStream();

            using (var stream = await photo.OpenReadAsync())
            {
                await stream.CopyToAsync(ImageMemoryStream);
            }

            data.Hasimage = true;
            data.Hideimage = false;
            data.Details = data.Details = "Review your photo. Select 'Complete' to submit your response, or 'Change Photo' to try again. Tap the 'x' icon if you want to remove the current image completely.";
            data.Picturebtn = "Change photo";
            data.Completebtn = "Complete";

            var previewBytes = ImageMemoryStream.ToArray();
            data.ImageSource = ImageSource.FromStream(() => new MemoryStream(previewBytes));

            RefreshCollectionView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing gallery image: {ex}");
        }
    }

    private async Task OpenCamera()
    {
        try
        {
            var stream = await cameraView.CaptureImage(CancellationToken.None);
            if (stream == null) return;

            // Safe disposal of old stream data before reallocation
            ImageMemoryStream?.Dispose();
            ImageMemoryStream = new MemoryStream();
            await stream.CopyToAsync(ImageMemoryStream);

            var data = ConfirmData.FirstOrDefault();
            if (data != null)
            {
                data.Hasimage = true;
                data.Hideimage = false;
                data.Details = "Review your photo. Select 'Complete' to submit your response, or 'Retake Photo' to try again. Tap the 'x' icon if you want to remove the current image completely.";
                data.Picturebtn = "Retake Photo";
                data.Completebtn = "Complete";

                var previewBytes = ImageMemoryStream.ToArray();
                data.ImageSource = ImageSource.FromStream(() => new MemoryStream(previewBytes));

                RefreshCollectionView();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing captured camera stream: {ex}");
        }
    }

    private void RefreshCollectionView()
    {
        CameraStack.IsVisible = false;
        MainCollectionView.IsVisible = true;
        MainCollectionView.ItemsSource = null;
        MainCollectionView.ItemsSource = ConfirmData;
    }

    private void RemoveImage_Clicked(object sender, EventArgs e)
    {
        try
        {
            var data = ConfirmData.FirstOrDefault();
            if (data == null) return;

            ImageMemoryStream?.Dispose();
            ImageMemoryStream = null;
            ImageStream?.Dispose();
            ImageStream = null;

            data.Hasimage = false;
            data.Hideimage = true;
            data.ImageSource = null;

            data.Picturebtn = "Take Photo"; 
            data.Completebtn = "Upload from Gallery";

            RefreshCollectionView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing image view: {ex}");
        }
    }
}