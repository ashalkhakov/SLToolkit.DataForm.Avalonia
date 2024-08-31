using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    public class Employee : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dataErrors[nameof(LastName)] = "Last Name is required";
                else if (value.Trim().Length < 2)
                    _dataErrors[nameof(LastName)] =
                    "Last Name must be at least 2 letters long.";
                else if (_dataErrors.ContainsKey(nameof(LastName)))
                    _dataErrors.Remove(nameof(LastName));
                
                _lastName = value;
                NotifyPropertyChanged(nameof(LastName));
            }
        }
        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                NotifyPropertyChanged(nameof(FirstName));
            }
        }
        private int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (ValidateSalaryAndLevel(value, Salary))
                {
                    _level = value;
                    NotifyPropertyChanged(nameof(Level));
                }
            }
        }

        private decimal _salary;
        public decimal Salary
        {
            get { return _salary; }
            set
            {
                if (ValidateSalaryAndLevel(Level, value))
                {
                    _salary = value;
                    NotifyPropertyChanged(nameof(Salary));
                }
            }
        }

        private bool ValidateSalaryAndLevel(int level, decimal salary)
        {
            if (level < 100 || level > 102)
            {
                _dataErrors[nameof(Level)] = "Level must be between 100 and 102";
                return false;
            }
            bool isValid = false;
            switch (level)
            {
                case 100:
                    isValid = (salary >= 50000 && salary < 65000);
                    break;
                case 101:
                    isValid = (salary >= 65000 && salary < 80000);
                    break;
                case 102:
                    isValid = (salary >= 80000 && salary < 105000);
                    break;
            }
            if (isValid)
            {
                if (_dataErrors.ContainsKey(nameof(Level)))
                    _dataErrors.Remove(nameof(Level));
                if (_dataErrors.ContainsKey(nameof(Salary)))
                    _dataErrors.Remove(nameof(Salary));
            }
            else
            {
                _dataErrors[nameof(Level)] = "Level does not match salary range";
                _dataErrors[nameof(Salary)] = "Salary does not match level";
            }
            return isValid;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDataErrorInfo Members
        private string _dataError = string.Empty;
        string IDataErrorInfo.Error
        {
            get { return _dataError; }
        }
        private Dictionary<string, string> _dataErrors =
        new Dictionary<string, string>();
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (_dataErrors.ContainsKey(columnName))
                    return _dataErrors[columnName];
                else
                    return null;
            }
        }
        #endregion
    }
}
