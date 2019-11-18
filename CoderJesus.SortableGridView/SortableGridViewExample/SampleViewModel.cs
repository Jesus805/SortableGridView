using System.Collections.ObjectModel;

namespace SortableGridViewExample
{
    public class SampleViewModel
    {
        public ObservableCollection<SampleModel> SampleCollection { get; }

        public SampleViewModel()
        {
            // Populate SampleCollection with items.
            SampleCollection = new ObservableCollection<SampleModel>();
            SampleCollection.Add(new SampleModel("First", 1));
            SampleCollection.Add(new SampleModel("Second", 2));
            SampleCollection.Add(new SampleModel("Third", 3));
        }
    }
}
