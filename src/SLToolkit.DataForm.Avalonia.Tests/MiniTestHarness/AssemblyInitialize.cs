using Microsoft.Silverlight.Testing;
using SLToolkit.DataForm.WPF.Controls;
using Microsoft.Silverlight.Testing.Harness;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading;

namespace SLToolkit.DataForm.WPF.Tests
{
    [TestClass]
    public class AssemblyInitialize
    {
        private static AutoResetEvent _waitHandle;
        private static App _application;
        private static Thread _uiThread;
        private static GlobalExceptionHandler _globalExceptions;

        public static App ApplicationInstance => _application;
        public static Thread UIThread => _uiThread;
        
        //[AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            _waitHandle = new AutoResetEvent(false);

            _uiThread = new Thread(() =>
            {
                var builder = BuildAvaloniaApp();

                builder.SetupWithClassicDesktopLifetime(["foo.exe"]);
                AppMain(builder.Instance!, ["foo.exe"]);
            });

            _globalExceptions = new GlobalExceptionHandler(GlobalUnhandledExceptionListener);

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Name = "TestWPFUIThread";
            _uiThread.Start();
            _waitHandle.WaitOne();

            /*Dispatcher.UIThread.Invoke(() =>
            {
                _application.MainWindow.Show();
            });*/
            _globalExceptions.AttachGlobalHandler = true;
        }

        // Application entry point. Avalonia is completely initialized.
        static void AppMain(Application app, string[] args)
        {
            // A cancellation token source that will be 
            // used to stop the main loop
            var cts = new CancellationTokenSource();

            // Do your startup code here
            _application = (App)app;
            //((App)app).MainWindow.Show();

            _waitHandle.Set();

            // Start the main loop
            app.Run(cts.Token);
        }

        //[AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Dispatcher.UIThread.InvokeShutdown();
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

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                //.UsePlatformDetect()
                //.WithInterFont()
                .LogToTrace();
    }
}
