using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Reactive;
using SLToolkit.DataForm.WPF.Controls.Common;
using SLToolkit.DataForm.Avalonia.Properties;
using LabelAutomationPeer = SLToolkit.DataForm.WPF.Automation.LabelAutomationPeer;

namespace SLToolkit.DataForm.WPF.Controls
{
    public class Label : ContentControl
    {
        private bool _initialized;
        private bool _isRequiredOverridden;
        private bool _canContentUseMetaData;
        private bool _isContentBeingSetInternally;
        private IDisposable? _subscription;
        private bool _targetHasErrors;

        public static readonly StyledProperty<bool> IsRequiredProperty
            = DependencyProperty.Register<Label, bool>("IsRequired");
        public static readonly StyledProperty<bool> IsValidProperty
            = DependencyProperty.Register<Label, bool>("IsValid", true);
        public static readonly StyledProperty<string> PropertyPathProperty
            = DependencyProperty.Register<Label, string>("PropertyPath");
        public static readonly StyledProperty<FrameworkElement> TargetProperty
            = DependencyProperty.Register<Label, FrameworkElement>("Target");

        protected override Type StyleKeyOverride => typeof(Label);

        static Label()
        {
            DataContextProperty.Changed.AddClassHandler<Label>(OnDataContextPropertyChanged);
            IsRequiredProperty.Changed.AddClassHandler<Label>(OnIsRequiredPropertyChanged);
            IsValidProperty.Changed.AddClassHandler<Label>(OnIsValidPropertyChanged);
            PropertyPathProperty.Changed.AddClassHandler<Label>(OnPropertyPathPropertyChanged);
            TargetProperty.Changed.AddClassHandler<Label>(OnTargetPropertyChanged);
            IsEnabledProperty.Changed.AddClassHandler<Label>(Label_IsEnabledChanged);
            ContentProperty.Changed.AddClassHandler<Label>(OnContentChanged);
        }

        public Label()
        {
            this._targetHasErrors = false;
            this[DataContextProperty] = new Binding();
            base.Loaded += new RoutedEventHandler(this.Label_Loaded);
            this._canContentUseMetaData = base.Content == null;
            if (Design.IsDesignMode)
            {
                this.SetContentInternally(typeof(Label).Name);
            }
        }

        private static void Label_IsEnabledChanged(Label sender, DependencyPropertyChangedEventArgs e)
        {
            sender.UpdateCommonState();
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._initialized)
            {
                this.LoadMetadata(false);
                this._initialized = true;
                base.Loaded -= new RoutedEventHandler(this.Label_Loaded);
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
                if (this.ValidationMetadata == null)
                {
                    if (this._canContentUseMetaData)
                    {
                        this.SetContentInternally(null);
                    }
                }
                else
                {
                    string caption = this.ValidationMetadata.Caption;
                    if ((caption != null) && this._canContentUseMetaData)
                    {
                        this.SetContentInternally(caption);
                    }
                }
                if (!this._isRequiredOverridden)
                {
                    bool flag = (this.ValidationMetadata != null) && this.ValidationMetadata.IsRequired;
                    base.SetValue(IsRequiredProperty, flag);
                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.UpdateValidationState();
            this.UpdateRequiredState();
        }

        private static void OnContentChanged(Label label, AvaloniaPropertyChangedEventArgs args)
        {
            var oldContent = args.OldValue as Control;
            var newContent = args.NewValue as Control;

            if (Design.IsDesignMode && (newContent == null))
            {
                label.SetContentInternally(typeof(Label).Name);
            }
            label._canContentUseMetaData = label._isContentBeingSetInternally || (newContent == null);
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new LabelAutomationPeer(this);

        private static void OnDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Label label = d as Label;
            if ((label != null) && (((e.OldValue == null) || (e.NewValue == null)) || !ReferenceEquals(e.OldValue.GetType(), e.NewValue.GetType())))
            {
                label.LoadMetadata(false);
            }
        }

        private static void OnIsRequiredPropertyChanged(Label label, DependencyPropertyChangedEventArgs e)
        {
            if (label != null)
            {
                label.UpdateRequiredState();
            }
        }

        private static void OnIsValidPropertyChanged(Label label, DependencyPropertyChangedEventArgs e)
        {
            if ((label != null) && !label.AreHandlersSuspended())
            {
                label.SetValueNoCallback(IsValidProperty, e.OldValue);
                object[] args = new object[] { "IsValid" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnPropertyPathPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            Label label = depObj as Label;
            if ((label != null) && label.Initialized)
            {
                label.LoadMetadata(false);
                label.ParseTargetValidState();
            }
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Label label))
                return;

            label.LoadMetadata(false);
            label._targetHasErrors = false;
            label._subscription?.Dispose();
            var oldValue = e.OldValue as FrameworkElement;
            var newValue = e.NewValue as FrameworkElement;
            if (newValue != null)
            {
                var observable = newValue.GetPropertyChangedObservable(DataValidationErrors.HasErrorsProperty);
                label._subscription = observable.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(c => label.Target_BindingValidationError(c)));

                label._targetHasErrors = DataValidationErrors.GetHasErrors(newValue);
            }
            label.ParseTargetValidState();
        }

        private void ParseTargetValidState()
        {
            this.IsValid = !this._targetHasErrors;
            this.UpdateValidationState();
        }

        public virtual void Refresh()
        {
            this._isRequiredOverridden = false;
            this.LoadMetadata(true);
            this.ParseTargetValidState();
        }

        private void SetContentInternally(object value)
        {
            try
            {
                this._isContentBeingSetInternally = true;
                base.Content = value;
            }
            finally
            {
                this._isContentBeingSetInternally = false;
            }
        }

        private void Target_BindingValidationError(AvaloniaPropertyChangedEventArgs e)
        {
            this._targetHasErrors = (bool)e.NewValue;
            this.ParseTargetValidState();
        }

        private void UpdateCommonState()
        {
        }

        private void UpdateRequiredState()
        {
        }

        private void UpdateValidationState()
        {
        }

        public bool IsRequired
        {
            get => 
                ((bool) base.GetValue(IsRequiredProperty));
            set
            {
                this._isRequiredOverridden = true;
                base.SetValue(IsRequiredProperty, value);
            }
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

