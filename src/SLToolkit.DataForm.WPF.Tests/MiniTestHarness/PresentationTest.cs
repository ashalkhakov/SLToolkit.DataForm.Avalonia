using Microsoft.Silverlight.Testing.Harness;
using SLToolkit.DataForm.WPF.Tests;
using SLToolkit.DataForm.WPF.Tests.MiniTestHarness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Microsoft.Silverlight.Testing
{
    public class PresentationTest
    {
        /// <summary>
        /// Handles unit test that need to execute on UI thread Synchronously.
        /// </summary>
        /// the unit test to execute  
        public void ExecuteOnUIThread(Action unitTest)
        {
            AssemblyInitialize.ApplicationInstance.Dispatcher.Invoke(unitTest);
        }

        public void ExecuteOnUIThread(Func<bool> predicate)
        {
            while (AssemblyInitialize.ApplicationInstance.Dispatcher.Invoke(predicate))
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
                    TestPanelManager.TestPage = (ITestPage)AssemblyInitialize.ApplicationInstance.MainWindow;
                }

                return TestPanelManager.TestPanel;
            }
        }

        public virtual void EnqueueCallback(Action testCallbackDelegate)
        {
            ExecuteOnUIThread(testCallbackDelegate);
        }

        public virtual void EnqueueTestComplete()
        {

        }

        public virtual void EnqueueDelay(TimeSpan delay)
        {
            Thread.Sleep(delay);
        }

        public virtual void EnqueueDelay(double milliseconds)
        {
            EnqueueDelay(TimeSpan.FromMilliseconds(milliseconds));
        }

        public virtual void EnqueueConditional(Func<bool> conditionalDelegate)
        {
            while (!AssemblyInitialize.ApplicationInstance.Dispatcher.Invoke(conditionalDelegate))
            {
                // spin-wait
            }
        }
    }
}

