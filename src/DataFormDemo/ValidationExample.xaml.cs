using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataFormDemo
{
    /// <summary>
    /// Interaction logic for ValidationExample.xaml
    /// </summary>
    public partial class ValidationExample : UserControl
    {
        private Employee _employee = new Employee();

        public ValidationExample()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = _employee;
        }
    }
}
