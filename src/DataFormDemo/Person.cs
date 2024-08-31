using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    public enum MaritalStatus
    {
        Unknown,
        Married,
        Single,
        Divorced
    }
    public class Person
    {
        [Required]
        [StringLength(25)]
        public string LastName { get; set; }
        [Required]
        [StringLength(25)]
        public string FirstName { get; set; }
        [Display(Name = "Registered",
            Description = "Check if this person has registered with us.")]
        public bool IsRegistered { get; set; }
        [Display(Name = "Marital Status",
            Description = "Optional marital status information.")]
        [Editable(false)]
        public MaritalStatus MaritalStatus { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z][\w\.&-]*[a-zA-Z0-9]@[a-zA-Z0-9]
➥ [\w\.-]*[a-zA-Z0-9]\.[a-zA-Z\.]*[a-zA-Z]$")]
        public string EmailAddress { get; set; }
        [Required]
        [Range(0, 20)]
        public int NumberOfChildren { get; set; }
    }
}
