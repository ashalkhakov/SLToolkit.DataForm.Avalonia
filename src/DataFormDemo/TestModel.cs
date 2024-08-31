using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    public class TestModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string Name {
            get => _name;
            set => this.SetValue(ref _name, value, nameof(Name));
        }

        private string _address;
        public string Address {
            get => _address;
            set => this.SetValue(ref _address, value, nameof(Address));
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrEmpty(Name))
                            result = "Enter name";
                        break;
                    case nameof(Address):
                        if (string.IsNullOrEmpty(Name))
                            result = "Enter address";
                        break;
                }

                return result;
            }
        }


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        private void SetValue<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Error { get; }
    }
}
