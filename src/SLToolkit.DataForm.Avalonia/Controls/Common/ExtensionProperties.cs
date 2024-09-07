using Avalonia;

namespace SLToolkit.DataForm.WPF.Controls.Common
{
    internal class ExtensionProperties : DependencyObject
    {
        public static readonly DependencyProperty AreHandlersSuspended =
            AvaloniaProperty.RegisterAttached<ExtensionProperties, FrameworkElement, bool>(
                "AreHandlersSuspended",
                false);

        public static bool GetAreHandlersSuspended(DependencyObject obj) => 
            ((bool) obj.GetValue(AreHandlersSuspended));

        public static void SetAreHandlersSuspended(DependencyObject obj, bool value)
        {
            obj.SetValue(AreHandlersSuspended, value);
        }
    }
}

