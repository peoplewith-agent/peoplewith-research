using System.Collections.ObjectModel;
using System.Text.Json;

namespace PeopleWithResearch;

public partial class FAQ_s : ContentPage
{
    private List<FAQItem> allFAQs = new();
    public ObservableCollection<FAQItem> FilteredFAQs { get; } = new();

    public List<string> AllFilters { get; set; } = new();
    public signupcode SignUp { get; set; } = new();

    private bool isClearing = false;
    private bool isResettingFilter = false;

    public FAQ_s()
    {
        InitializeComponent();
        FAQListView.ItemsSource = FilteredFAQs;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await LoadFAQDataAsync();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "OnAppearing");
            ShowEmptyState(true);
        }
    }

    private async Task LoadFAQDataAsync()
    {
        try
        {
            FilterStack.IsVisible = false;
            loadingstack.IsVisible = true;

            await Task.Delay(200); 

            var signUpList = await APICalls.Instance.GetSingupCode();
            if (signUpList == null || !signUpList.Any())
            {
                ShowEmptyState(true);
                return;
            }

            SignUp = signUpList.FirstOrDefault();
            if (SignUp?.FAQList == null)
            {
                ShowEmptyState(true);
                return;
            }

            allFAQs = SignUp.FAQList.ToList();

            InitializeFilters();
            ApplyFilterAndSearch();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadFAQDataAsync");
            ShowEmptyState(true);
        }
        finally
        {
            loadingstack.IsVisible = false;
        }
    }

    private void InitializeFilters()
    {
        AllFilters = allFAQs
            .Where(q => !string.IsNullOrEmpty(q.Group))
            .Select(q => q.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        AllFilters.Insert(0, "All");

        isResettingFilter = true;
        FiltersView.ItemsSource = AllFilters;
        FiltersView.SelectedItem = "All";
        isResettingFilter = false;
    }

    private void ApplyFilterAndSearch(string selectedFilter = null)
    {
        if (isClearing) return;

        string query = SearchEntry.Text?.Trim().ToLower() ?? string.Empty;
        string currentFilter = selectedFilter ?? (FiltersView.SelectedItem as string) ?? "All";

        var results = currentFilter == "All"
            ? allFAQs
            : allFAQs.Where(x => x.Group == currentFilter);

        if (!string.IsNullOrEmpty(query))
        {
            results = results.Where(x =>
                (x.Title != null && x.Title.ToLower().Contains(query)) ||
                (x.Description != null && x.Description.ToLower().Contains(query)));
        }

        var filteredList = results.ToList();

        FilteredFAQs.Clear();
        foreach (var item in filteredList)
        {
            FilteredFAQs.Add(item);
        }

        FilterStack.IsVisible = true;
        FAQListView.RefreshView();
        ShowEmptyState(FilteredFAQs.Count == 0);
    }

    private void ShowEmptyState(bool isEmpty)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EmptyView.IsVisible = isEmpty;
            FAQListView.IsVisible = !isEmpty;
            loadingstack.IsVisible = false;
        });
    }

    private void FilterItem_Tapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (isResettingFilter || isClearing) return;

        string filterTapped = e.DataItem as string;
        ApplyFilterAndSearch(filterTapped);
    }

    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearEntry.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);

        if (!string.IsNullOrEmpty(e.NewTextValue))
        {
            if (FiltersView.SelectedItem is string selected && selected != "All")
            {
                isResettingFilter = true;
                FiltersView.SelectedItem = "All";
                isResettingFilter = false;
            }
        }

        ApplyFilterAndSearch();
    }

    private void ClearEntry_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            isClearing = true;
            isResettingFilter = true;

            SearchEntry.Text = string.Empty;
            ClearEntry.IsVisible = false;
            FiltersView.SelectedItem = "All";
            foreach (var item in allFAQs)
            {
                item.IsExpanded = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ClearEntry_Tapped");
        }
        finally
        {
            isResettingFilter = false;
            isClearing = false;
            ApplyFilterAndSearch();
        }
    }

    private void FAQListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is not FAQItem tappedItem) return;

        tappedItem.IsExpanded = !tappedItem.IsExpanded;
        foreach (var item in allFAQs)
        {
            if (item != tappedItem && item.IsExpanded)
            {
                item.IsExpanded = false;
            }
        }
    }
}