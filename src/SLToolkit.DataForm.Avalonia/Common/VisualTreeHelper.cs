using Avalonia.Input;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.VisualTree;
using Avalonia.Controls;

namespace SLToolkit.DataForm.WPF.Common
{
    /// <summary>
    /// Helper methods for UI-related tasks.
    /// This class was obtained from Philip Sumi (a fellow WPF Disciples blog)
    /// http://www.hardcodet.net/uploads/2009/06/UIHelper.cs
    /// </summary>
    internal static class VisualTreeHelper
    {
        public static IAvaloniaList<Visual> GetVisualChildren(Visual parent)
        {
            return (IAvaloniaList<Visual>)parent.GetVisualChildren();
        }


        public static object GetChild(AvaloniaObject element, int i)
        {
            return GetChildObjects(element as Visual)?.Skip(i).Take(1);
        }

        public static int GetChildrenCount(AvaloniaObject element)
        {
            return GetVisualChildren(element as Visual)?.Count ?? 0;
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetChild"/> method, which also
        /// supports content elements. Keep in mind that for content elements,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="parent">The item to be processed.</param>
        /// <param name="forceUsingTheVisualTreeHelper">Sometimes it's better to search in the VisualTree (e.g. in tests)</param>
        /// <returns>The submitted item's child elements, if available.</returns>
        public static IEnumerable<Visual> GetChildObjects(this Visual parent, bool forceUsingTheVisualTreeHelper = false)
        {
            if (parent == null) yield break;

            if (!forceUsingTheVisualTreeHelper && (parent is ContentControl || parent is Control))
            {
                //use the logical tree for content / framework elements
                foreach (object obj in GetVisualChildren(parent))
                {
                    var depObj = obj as Visual;
                    if (depObj != null) yield return (Visual)obj;
                }
            }
            else
            {
                //use the visual tree per default
                var visualChildren = GetVisualChildren(parent);
                for (int i = 0; i < visualChildren.Count; i++)
                {
                    yield return visualChildren[i];
                }
            }
        }

        public static DependencyObject? GetParent(DependencyObject? x)
        {
            return (x as Visual)?.Parent;
        }

        public static void HitTest(UIElement element,
            Func<DependencyObject, HitTestFilterBehavior> filter,
            Func<HitTestResult, HitTestResultBehavior> test,
            PointHitTestParameters parameters)
        {
            DependencyObject? topMost = element.InputHitTest(parameters.Position) as DependencyObject;
            while (topMost != null)
            {
                if (filter(topMost) == HitTestFilterBehavior.Stop)
                {
                    break;
                }
                topMost = GetParent(topMost);
            }
        }

        public static Window? GetWindow(Control control)
        {
            while (control != null && !(control is Window))
            {
                control = control.Parent as Control;
            }
            return control as Window;
        }
    }

    internal class PointHitTestParameters
    {
        public PointHitTestParameters(Point position)
        {
            Position = position;
        }

        public Point Position { get; }
    }

    internal enum HitTestFilterBehavior
    {
        Continue,
        ContinueSkipSelfAndChildren,
        Stop
    }

    internal enum HitTestResultBehavior
    {
        Continue,
        Stop
    }

    internal class HitTestResult
    {
        public HitTestResult(Visual visualHit)
        {
            VisualHit = visualHit;
        }

        public Visual VisualHit { get; }
    }
}
