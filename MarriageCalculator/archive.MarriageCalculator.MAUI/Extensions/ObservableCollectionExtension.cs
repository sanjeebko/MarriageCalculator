
using System.Collections.ObjectModel;

namespace MarriageCalculator.Extensions;

public static class ObservableCollectionExtension
{
    //create a extension method to load string to ObservableCollection<string> 
    public static void Load<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
