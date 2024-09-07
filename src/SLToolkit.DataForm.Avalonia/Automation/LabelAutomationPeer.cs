using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Label = SLToolkit.DataForm.WPF.Controls.Label;

namespace SLToolkit.DataForm.WPF.Automation
{
    public class LabelAutomationPeer : ControlAutomationPeer
    {
        public LabelAutomationPeer(Label owner) : base((FrameworkElement) owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore() => 
            AutomationControlType.Text;

        protected override string GetClassNameCore() => 
            "Label";

        protected override string GetNameCore()
        {
            string nameCore = base.GetNameCore();
            if (string.IsNullOrEmpty(nameCore))
            {
                Label owner = base.Owner as Label;
                if ((owner != null) && (owner.Content != null))
                {
                    TextBlock content = owner.Content as TextBlock;
                    nameCore = (content == null) ? owner.Content.ToString() : content.Text;
                }
            }
            return nameCore;
        }
    }
}

