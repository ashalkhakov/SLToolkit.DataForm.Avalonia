using System.ComponentModel.DataAnnotations;

namespace DataFormDemo.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string greeting = "Welcome to Avalonia!";

    [Display(Name = "Greeting", Description = "This is a greeting")]
    [Required]
    [MinLength(2)]
    public string Greeting
    {
        get => greeting;
        set => SetProperty(ref greeting, value, true);
    }
}
