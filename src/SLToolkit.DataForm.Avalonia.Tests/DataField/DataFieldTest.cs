using Avalonia.Controls;
using Microsoft.Silverlight.Testing;

namespace SLToolkit.DataForm.Avalonia.Tests.DataField
{
    [TestClass]
    public class DataFieldTest : PresentationTest
    {
        [TestInitialize]
        public override Task Initialize()
        {
            return base.Initialize();
        }

        [TestCleanup]
        public override void Shutdown()
        {
            base.Shutdown();
        }

        [TestMethod]
        public async Task Test()
        {
            await EnqueueCallback(() =>
            {
                _textbox = new TextBox();

                _textbox.Text = "Hello Avalonia";

                StackPanel stackPanel = new StackPanel();

                stackPanel.Children.Add(this._textbox);

                this.TestPanel.Children.Add(stackPanel);
            });

            await EnqueueDelay(TimeSpan.FromMinutes(1));
        }

        private TextBox _textbox;
    }
}
