using Avalonia.Controls;
using Microsoft.Silverlight.Testing;

namespace SLToolkit.DataForm.WPF.Tests.MiniTestHarness
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class TestPage : Window, ITestPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the test surface, a dynamic Panel that removes its children 
        /// elements after each test completes.
        /// </summary>
        public Panel TestPanel
        {
            get { return TestStage; }
        }
    }
}
