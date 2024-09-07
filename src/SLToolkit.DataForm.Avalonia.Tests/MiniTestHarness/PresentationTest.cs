using Avalonia.Controls;
using Avalonia.Headless;
using Microsoft.Silverlight.Testing.Harness;
using SLToolkit.DataForm.WPF.Tests;
using SLToolkit.DataForm.WPF.Tests.MiniTestHarness;

namespace Microsoft.Silverlight.Testing
{
    public class PresentationTest
    {
        protected HeadlessUnitTestSession _headlessUnitTestSession;

        public virtual async Task Initialize()
        {
            _headlessUnitTestSession = HeadlessUnitTestSession.StartNew(typeof(App));
            await _headlessUnitTestSession.Dispatch(() =>
            {
                TestPanelManager = new TestPanelManager();
                TestPanelManager.TestPage = new TestPage();
                ((Window)TestPanelManager.TestPage).Show();
            }, CancellationToken.None);
        }

        public virtual void Shutdown()
        {
            _headlessUnitTestSession.Dispose();
        }

        /// <summary>
        /// Handles unit test that need to execute on UI thread Synchronously.
        /// </summary>
        /// the unit test to execute  
        public async Task ExecuteOnUIThread(Action unitTest)
        {
            await _headlessUnitTestSession.Dispatch(unitTest, CancellationToken.None);
        }

        public async Task ExecuteOnUIThread(Func<bool> predicate)
        {
            while (await _headlessUnitTestSession.Dispatch(predicate, CancellationToken.None))
            {
                // spin-wait
            }
        }

        public TestPanelManager TestPanelManager { get; private set; }

        public Panel TestPanel
        {
            get
            {
                if (TestPanelManager == null)
                {
                    TestPanelManager = new TestPanelManager();
                }

                if (TestPanelManager.TestPage == null)
                {
                    TestPanelManager.TestPage = new TestPage();
                }

                return TestPanelManager.TestPanel;
            }
        }

        public virtual async Task EnqueueCallback(Action testCallbackDelegate)
        {
            await ExecuteOnUIThread(testCallbackDelegate);
        }

        public virtual async Task EnqueueTestComplete()
        {

        }

        public virtual async Task EnqueueDelay(TimeSpan delay)
        {
            await Task.Delay(delay);
        }

        public virtual async Task EnqueueDelay(double milliseconds)
        {
            await EnqueueDelay(TimeSpan.FromMilliseconds(milliseconds));
        }

        public virtual async Task EnqueueConditional(Func<bool> conditionalDelegate)
        {
            while (!await _headlessUnitTestSession.Dispatch(conditionalDelegate, CancellationToken.None))
            {
                // spin-wait
            }
        }
    }
}

