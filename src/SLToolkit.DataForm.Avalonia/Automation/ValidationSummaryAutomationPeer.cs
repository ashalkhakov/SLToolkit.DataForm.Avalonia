using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using SLToolkit.DataForm.WPF.Controls;

namespace SLToolkit.DataForm.WPF.Automation
{
    public class ValidationSummaryAutomationPeer : ControlAutomationPeer, IInvokeProvider
    {
        public ValidationSummaryAutomationPeer(ValidationSummary owner) : base((FrameworkElement) owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore() => 
            AutomationControlType.Group;

        protected override string GetClassNameCore() => 
            typeof(ValidationSummary).Name;

        protected override string GetNameCore()
        {
            ValidationSummary owner = base.Owner as ValidationSummary;
            if ((owner != null) && (owner.HeaderContentControlInternal != null))
            {
                string content = owner.HeaderContentControlInternal.Content as string;
                if (content != null)
                {
                    return content;
                }
            }
            return base.GetNameCore();
        }

#if false
        public override object GetPattern(PatternInterface patternInterface) => 
            ((patternInterface != PatternInterface.Invoke) ? base.GetPattern(patternInterface) : this);
#endif

        void IInvokeProvider.Invoke()
        {
            ValidationSummary owner = base.Owner as ValidationSummary;
            if (owner != null)
            {
                owner.ExecuteClickInternal();
            }
        }
    }
}

