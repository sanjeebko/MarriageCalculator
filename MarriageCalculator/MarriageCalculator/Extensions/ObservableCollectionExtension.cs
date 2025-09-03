
using System.Collections.ObjectModel;

namespace MarriageCalculator.Extensions;

public static class ObservableCollectionExtension
{
    //create a extension method to load string to ObservableCollection<string> 
    public static void SafeLoad<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        where T : class
    {
        collection.Clear();
        foreach (var item in items.Where(i => i != null))
        {
            collection.Add(item);
        }
    }
}
