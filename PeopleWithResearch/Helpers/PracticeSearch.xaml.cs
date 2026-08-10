using Mopups.Pages;
using Mopups.Services;
using PeopleWithResearch.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PeopleWithResearch;

public partial class PractiseSearch : PopupPage
{
    private bool isediting = false;
    private TaskCompletionSource<OptionDetails> GPSelected;
    private List<OptionDetails> GPList = new(); 

    public PractiseSearch(TaskCompletionSource<OptionDetails> ReturnGP, List<OptionDetails> GPListPassed)
    {
        InitializeComponent();
        GPSelected = ReturnGP;
        GPList = GPListPassed; 
    }

    private void GpEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

        }
        catch (Exception Ex)
        {

        }
    }

    private void gplistview_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {

    }


    //private async void Okbtn_Clicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        // Ensure the label exists and has a value
    //        string selectedDate = Datelbl.Text ?? string.Empty;

    //        // Set the result
    //        GPSelected.TrySetResult(selectedDate);

    //        // Close Popup
    //        await MopupService.Instance.PopAsync();
    //    }
    //    catch (Exception Ex)
    //    {
    //        NotasyncMethod(Ex);
    //    }
    //}


}
