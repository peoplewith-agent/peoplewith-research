using System.Collections.ObjectModel;

namespace PeopleWithResearch;

/// <summary>
/// Mirrors List&lt;T&gt;.RemoveAll for ObservableCollection&lt;T&gt;, which doesn't have one
/// built in. Imperial.xaml.cs calls Allregfields.RemoveAll(...) several times on an
/// ObservableCollection&lt;RegField&gt;, so an extension like this must already exist
/// somewhere in the project — if you can find it, delete this copy instead of keeping both
/// (a duplicate definition in the same namespace will fail to compile with a CS0121
/// "ambiguous call" error).
/// </summary>
public static class ObservableCollectionExtensions
{
    public static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
    {
        var itemsToRemove = collection.Where(predicate).ToList();
        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }
        return itemsToRemove.Count;
    }
}
