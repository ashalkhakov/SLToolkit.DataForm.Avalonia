using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SLToolkit.DataForm.WPF.Controls;
using SLToolkit.DataForm.WPF.Tests.MiniTestHarness;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.UnitTests;
using System.Windows.Controls;
using System.Windows.Data;
using System.Runtime.InteropServices;
using Microsoft.Silverlight.Testing.Harness;

namespace SLToolkit.DataForm.WPF.Tests
{
    [TestClass]
    public class AssemblyInitialize
    {
        private static App _application;
        private static Thread _uiThread;
        private static GlobalExceptionHandler _globalExceptions;

        public static App ApplicationInstance => _application;
        public static Thread UIThread => _uiThread;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);

            _uiThread = new Thread(() =>
            {
                _application = new App();
                Application.ResourceAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                _application.InitializeComponent();
                waitHandle.Set();
                _application.Run();
            });

            _globalExceptions = new GlobalExceptionHandler(GlobalUnhandledExceptionListener);

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Name = "TestWPFUIThread";
            _uiThread.Start();
            waitHandle.WaitOne();

            _application.Dispatcher.Invoke(() =>
            {
                _application.MainWindow.Show();
            });
            _globalExceptions.AttachGlobalHandler = true;
        }

        [TestMethod]
        public async Task TestWpfApp()
        {
            ITestPage window = null;

            await _application.Dispatcher.InvokeAsync(async () =>
            {
                window = (ITestPage)_application.MainWindow;

                StackPanel stackPanel = new StackPanel();

                stackPanel.DataContext =
                    new DataClassWithValidation()
                    {
                        BoolProperty = true,
                        IntProperty = 1,
                        StringProperty = "test string"
                    };

                var _dataField = new DataField();

                var _textBox = new TextBox();
                _textBox.SetBinding(TextBox.TextProperty, new Binding("StringProperty") { Mode = BindingMode.TwoWay });

                _dataField.Content = _textBox;

                DataField.SetIsFieldGroup(stackPanel, true);
                stackPanel.Children.Add(_dataField);

                _initialized = false;
                _dataField.Loaded += new RoutedEventHandler(TestPanel_Initialized);
                window.TestPanel.Children.Add(stackPanel);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            while (!_initialized)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            _application.Dispatcher.InvokeShutdown();
            _uiThread.Join();
        }

        private bool _initialized = false;

        private void TestPanel_Initialized(object sender, EventArgs e)
        {
            _initialized = true;
        }

        /// <summary>
        /// Listener event for any unhandled exceptions.
        /// </summary>
        /// <param name="sender">Sender object instance.</param>
        /// <param name="e">Event arguments.</param>
        private static void GlobalUnhandledExceptionListener(object sender, EventArgs e)
        {
#if false
            if (DispatcherStack.CurrentCompositeWorkItem is CompositeWorkItem)
            {
                CompositeWorkItem cd = (CompositeWorkItem)DispatcherStack.CurrentCompositeWorkItem;
                Exception exception = GlobalExceptionHandler.GetExceptionObject(e);
                cd.WorkItemException(exception);
                GlobalExceptionHandler.ChangeExceptionBubbling(e, /* handled */ true);
            }
            else
#endif
            {
                throw new Exception("Something went wrong");
                GlobalExceptionHandler.ChangeExceptionBubbling(e, /* handled */ false);
            }
        }
    }
}
