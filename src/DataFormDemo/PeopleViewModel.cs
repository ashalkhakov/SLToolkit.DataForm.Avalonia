using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    public class PeopleViewModel
    {
        private ObservableCollection<Person> _people =
            new ObservableCollection<Person>();

        public ObservableCollection<Person> People { get { return _people; } }

        public PeopleViewModel()
        {
            _people.Add(new Person()
            {
                FirstName = "Captain",
                LastName = "Avatar",
                IsRegistered = true,
                MaritalStatus = MaritalStatus.Unknown,
                DateOfBirth = DateTime.Parse("1912-01-01")
            });
            _people.Add(new Person()
            {
                FirstName = "Derek",
                LastName = "Wildstar",
                IsRegistered = true,
                MaritalStatus = MaritalStatus.Single,
                DateOfBirth = DateTime.Parse("1954-11-15")
            });
        }
    }
}
