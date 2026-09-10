namespace PeopleWithResearch;

/// <summary>
/// Lets a ViewModel show a native alert without holding a reference to a Page (the original
/// called ContentPage.DisplayAlert directly from inside Imperial.xaml.cs's ValidateNameStack —
/// this is the same call, just reachable from a class that isn't itself a Page).
/// </summary>
public interface IAlertService
{
    Task DisplayAlertAsync(string title, string message, string cancel);
}

public sealed class AlertService : IAlertService
{
    public Task DisplayAlertAsync(string title, string message, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            return page.DisplayAlert(title, message, cancel);
        }

        return Task.CompletedTask;
    }
}
