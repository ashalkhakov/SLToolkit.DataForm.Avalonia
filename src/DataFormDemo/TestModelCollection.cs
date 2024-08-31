using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    internal class TestModelCollection : ObservableCollection<TestModel>
    {
        public TestModelCollection()
        {
            Add(new TestModel()
            {
                Name = "Name",
                Address = string.Empty
            });
        }
    }
}
