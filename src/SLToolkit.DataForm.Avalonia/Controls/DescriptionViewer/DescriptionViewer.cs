using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using SLToolkit.DataForm.WPF.Automation;
using SLToolkit.DataForm.WPF.Controls.Common;
using Avalonia.Reactive;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Primitives;
using SLToolkit.DataForm.WPF.Common;

namespace SLToolkit.DataForm.WPF.Controls
{
    /*
    [TemplateVisualState(Name="InvalidFocused", GroupName="ValidationStates")]
    [TemplateVisualState(Name="InvalidUnfocused", GroupName="ValidationStates")]
    [TemplateVisualState(Name="Disabled", GroupName="CommonStates")]
    [TemplateVisualState(Name="ValidUnfocused", GroupName="ValidationStates")]
    [TemplateVisualState(Name="NoDescription", GroupName="DescriptionStates")]
    [TemplateVisualState(Name="HasDescription", GroupName="DescriptionStates")]
    [TemplateVisualState(Name="ValidFocused", GroupName="ValidationStates")]
    [TemplateVisualState(Name="Normal", GroupName="CommonStates")]
    [StyleTypedProperty(Property="ToolTipStyle", StyleTargetType=typeof(ToolTip))]
    */
    public class DescriptionViewer : TemplatedControl
    {
        private bool _descriptionOverridden;
        private bool _initialized;
        private new static readonly StyledProperty<object> DataContextProperty =
            DependencyProperty.Register<DescriptionViewer, object>("DataContext");
        public static readonly StyledProperty<string> DescriptionProperty =
            DependencyProperty.Register<DescriptionViewer, string>("Description");
        public static readonly StyledProperty<ControlTemplate> GlyphTemplateProperty =
            DependencyProperty.Register<DescriptionViewer, ControlTemplate>("GlyphTemplate", null);
        public static readonly StyledProperty<ControlTheme> ToolTipStyleProperty =
            DependencyProperty.Register<DescriptionViewer, ControlTheme>("ToolTipStyle", null);
        public new static readonly StyledProperty<bool> IsFocusedProperty =
            DependencyProperty.Register<DescriptionViewer, bool>("IsFocused", false);
        public static readonly StyledProperty<bool> IsValidProperty =
            DependencyProperty.Register<DescriptionViewer, bool>("IsValid", true);
        public static readonly StyledProperty<string> PropertyPathProperty =
            DependencyProperty.Register<DescriptionViewer, string>("PropertyPath");
        public static readonly StyledProperty<FrameworkElement> TargetProperty =
            DependencyProperty.Register<DescriptionViewer, FrameworkElement>("Target");

        private IDisposable? _subscription = null;

        protected override Type StyleKeyOverride => typeof(DescriptionViewer);

        static DescriptionViewer()
        {
            DataContextProperty.Changed.AddClassHandler<DescriptionViewer>(OnDataContextPropertyChanged);
            DescriptionProperty.Changed.AddClassHandler<DescriptionViewer>(OnDescriptionPropertyChanged);
            IsFocusedProperty.Changed.AddClassHandler<Label>(OnIsFocusedPropertyChanged);
            IsValidProperty.Changed.AddClassHandler<DescriptionViewer>(OnIsValidPropertyChanged);
            PropertyPathProperty.Changed.AddClassHandler<DescriptionViewer>(OnPropertyPathPropertyChanged);
            TargetProperty.Changed.AddClassHandler<DescriptionViewer>(OnTargetPropertyChanged);
            IsEnabledProperty.Changed.AddClassHandler<DescriptionViewer>(DescriptionViewer_IsEnabledChanged);
        }

        public DescriptionViewer()
        {
            base[DataContextProperty] = new Binding();
            base.Loaded += new RoutedEventHandler(this.DescriptionViewer_Loaded);
            if (Design.IsDesignMode)
            {
                this.Description = typeof(DescriptionViewer).Name;
            }
        }

        private static void DescriptionViewer_IsEnabledChanged(DescriptionViewer sender, DependencyPropertyChangedEventArgs e)
        {
            sender.UpdateCommonState();
        }

        private void DescriptionViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._initialized)
            {
                this.LoadMetadata(false);
                this._initialized = true;
                base.Loaded -= new RoutedEventHandler(this.DescriptionViewer_Loaded);
            }
        }

        private void LoadMetadata(bool forceUpdate)
        {
            ValidationMetadata objB = null;
            object entity = null;
            BindingExpressionBase bindingExpression = null;
            if (!string.IsNullOrEmpty(this.PropertyPath))
            {
                entity = base.DataContext;
                objB = ValidationHelper.ParseMetadata(this.PropertyPath, entity);
            }
            else if (this.Target != null)
            {
                objB = ValidationHelper.ParseMetadata(this.Target, forceUpdate, out entity, out bindingExpression);
            }
            if (!ReferenceEquals(this.ValidationMetadata, objB))
            {
                this.ValidationMetadata = objB;
                if (!this._descriptionOverridden)
                {
                    string description = null;
                    if (this.ValidationMetadata != null)
                    {
                        description = this.ValidationMetadata.Description;
                    }
                    base.SetValue(DescriptionProperty, description);
                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.UpdateValidationState();
            this.UpdateDescriptionState();
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new DescriptionViewerAutomationPeer(this);

        private static void OnDataContextPropertyChanged(DescriptionViewer viewer, DependencyPropertyChangedEventArgs e)
        {
            if ((viewer != null) && (((e.OldValue == null) || (e.NewValue == null)) || !ReferenceEquals(e.OldValue.GetType(), e.NewValue.GetType())))
            {
                viewer.LoadMetadata(false);
            }
        }

        private static void OnDescriptionPropertyChanged(DescriptionViewer viewer, DependencyPropertyChangedEventArgs e)
        {
            if (viewer != null)
            {
                viewer.UpdateDescriptionState();
            }
        }

        private static void OnIsFocusedPropertyChanged(Label label, DependencyPropertyChangedEventArgs e)
        {
            if ((label != null) && !label.AreHandlersSuspended())
            {
                label.SetValueNoCallback(IsFocusedProperty, e.OldValue);
                object[] args = new object[] { "IsFocused" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer viewer = d as DescriptionViewer;
            if ((viewer != null) && !viewer.AreHandlersSuspended())
            {
                viewer.SetValueNoCallback(IsValidProperty, e.OldValue);
                object[] args = new object[] { "IsValid" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnPropertyPathPropertyChanged(DescriptionViewer viewer, DependencyPropertyChangedEventArgs e)
        {
            if ((viewer != null) && viewer.Initialized)
            {
                viewer.LoadMetadata(false);
                viewer.ParseTargetValidState();
            }
        }

        private static void OnTargetPropertyChanged(DescriptionViewer dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject != null)
            {
                bool flag = false;
                //TODO: review this logic
                flag = e.NewValue == VisualTreeHelper.GetWindow(dependencyObject)?.FocusManager?.GetFocusedElement();
                if (dependencyObject.IsFocused != flag)
                {
                    dependencyObject.IsFocused = flag;
                }
                dependencyObject.LoadMetadata(false);
                FrameworkElement oldValue = e.OldValue as FrameworkElement;
                FrameworkElement newValue = e.NewValue as FrameworkElement;

                dependencyObject._subscription?.Dispose();

                if (oldValue != null)
                {
                    oldValue.GotFocus -= dependencyObject.Target_GotFocus;
                    oldValue.LostFocus -= dependencyObject.Target_LostFocus;
                }
                if (newValue != null)
                {
                    var observable = newValue.GetPropertyChangedObservable(DataValidationErrors.HasErrorsProperty);
                    dependencyObject._subscription = observable.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(c => dependencyObject.Target_BindingValidationError(c)));

                    newValue.GotFocus += dependencyObject.Target_GotFocus;
                    newValue.LostFocus += dependencyObject.Target_LostFocus;
                }
                dependencyObject.ParseTargetValidState();
            }
        }

        private void ParseTargetValidState()
        {
            this.IsValid = !string.IsNullOrEmpty(this.PropertyPath) || ((this.Target == null) || !DataValidationErrors.GetHasErrors(this.Target));
            this.UpdateValidationState();
        }

        public virtual void Refresh()
        {
            this._descriptionOverridden = false;
            this.LoadMetadata(true);
            this.ParseTargetValidState();
        }

        private void Target_BindingValidationError(AvaloniaPropertyChangedEventArgs e)
        {
            this.ParseTargetValidState();
        }

        private void Target_GotFocus(object sender, GotFocusEventArgs e)
        {
            this.IsFocused = true;
            this.UpdateValidationState();
        }

        private void Target_LostFocus(object sender, RoutedEventArgs e)
        {
            this.IsFocused = false;
            this.UpdateValidationState();
        }

        private void UpdateCommonState()
        {
            //VisualStateManager.GoToState(this, base.IsEnabled ? "Normal" : "Disabled", true);
        }

        private void UpdateDescriptionState()
        {
            if (string.IsNullOrEmpty(this.Description))
            {
                this.Classes.Add("NoDescription");
                this.Classes.Remove("HasDescription");
            }
            else
            {
                this.Classes.Remove("NoDescription");
                this.Classes.Add("HasDescription");
            }
            //VisualStateManager.GoToState(this, string.IsNullOrEmpty(this.Description) ? "NoDescription" : "HasDescription", true);
        }

        private void UpdateValidationState()
        {
            if (!this.IsValid)
            {
                //VisualStateManager.GoToState(this, this.IsFocused ? "InvalidFocused" : "InvalidUnfocused", true);
            }
            else if (!this.IsFocused || string.IsNullOrEmpty(this.Description))
            {
                //VisualStateManager.GoToState(this, "ValidUnfocused", true);
            }
            else
            {
                //VisualStateManager.GoToState(this, "ValidFocused", true);
            }
        }

        public string Description
        {
            get => 
                (base.GetValue(DescriptionProperty) as string);
            set
            {
                this._descriptionOverridden = true;
                base.SetValue(DescriptionProperty, value);
            }
        }

        public ControlTemplate GlyphTemplate
        {
            get => 
                (base.GetValue(GlyphTemplateProperty) as ControlTemplate);
            set => 
                base.SetValue(GlyphTemplateProperty, value);
        }

        public ControlTheme ToolTipStyle
        {
            get => 
                (base.GetValue(ToolTipStyleProperty) as ControlTheme);
            set => 
                base.SetValue(ToolTipStyleProperty, value);
        }

        protected new bool IsFocused
        {
            get => 
                ((bool) base.GetValue(IsFocusedProperty));
            private set => 
                this.SetValueNoCallback(IsFocusedProperty, value);
        }

        public bool IsValid
        {
            get => 
                ((bool) base.GetValue(IsValidProperty));
            private set => 
                this.SetValueNoCallback(IsValidProperty, value);
        }

        public string PropertyPath
        {
            get => 
                (base.GetValue(PropertyPathProperty) as string);
            set => 
                base.SetValue(PropertyPathProperty, value);
        }

        public FrameworkElement Target
        {
            get => 
                (base.GetValue(TargetProperty) as FrameworkElement);
            set => 
                base.SetValue(TargetProperty, value);
        }

        internal ValidationMetadata ValidationMetadata { get; set; }

        internal new bool Initialized =>
            this._initialized;
    }
}

