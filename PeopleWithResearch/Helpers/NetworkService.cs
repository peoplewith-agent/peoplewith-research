using Microsoft.Maui.Networking;

namespace PeopleWithResearch;
public class NetworkService
{
    private static readonly NetworkService _instance = new NetworkService();
    public static NetworkService Instance => _instance;

    public event EventHandler<bool> ConnectivityChanged;

    private NetworkService()
    {
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        // Raise event with the current connectivity status
        ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet);
    }
}
