using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Reactive;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Styling;
using SLToolkit.DataForm.WPF.Automation;
using SLToolkit.DataForm.WPF.Common;
using SLToolkit.DataForm.WPF.Controls.Common;

namespace SLToolkit.DataForm.WPF.Controls
{
    [TemplatePart(Name = "SummaryListBox", Type = typeof(ListBox))]
#if false
    [TemplateVisualState(Name="Normal", GroupName="CommonStates")]
    [TemplateVisualState(Name="Disabled", GroupName="CommonStates")]
    [TemplateVisualState(Name="HasErrors", GroupName="ValidationStates")]
    [TemplateVisualState(Name="Empty", GroupName="ValidationStates")]
    [StyleTypedProperty(Property = "SummaryListBoxStyle", StyleTargetType = typeof(ListBox))]
    [StyleTypedProperty(Property="ErrorStyle", StyleTargetType=typeof(ListBoxItem))]
#endif
    public class ValidationSummary : ContentControl
    {
        private const string PART_SummaryListBox = "SummaryListBox";
        private const string PART_HeaderContentControl = "HeaderContentControl";
        private ValidationSummaryItemSource _currentValidationSummaryItemSource;
        private ValidationItemCollection _displayedErrors;
        private ValidationItemCollection _errors;
        private ListBox _errorsListBox;
        private ContentControl _headerContentControl;
        private Dictionary<string, ValidationSummaryItem> _validationSummaryItemDictionary;
        private IDisposable? _subscription;

        public static readonly StyledProperty<bool> ShowErrorsInSummaryProperty =
            DependencyProperty.RegisterAttached<ValidationSummary, UIElement, bool>("ShowErrorsInSummary",
                true);
        public static readonly StyledProperty<ControlTheme> ErrorStyleProperty =
            DependencyProperty.Register<ValidationSummary, ControlTheme>("ErrorStyle");
        public static readonly StyledProperty<ValidationSummaryFilters> FilterProperty =
            DependencyProperty.Register<ValidationSummary, ValidationSummaryFilters>("Filter",
                ValidationSummaryFilters.All);
        public static readonly StyledProperty<bool> FocusControlsOnClickProperty =
            DependencyProperty.Register<ValidationSummary, bool>("FocusControlsOnClick",
                true);
        public static readonly StyledProperty<bool> HasErrorsProperty =
            DependencyProperty.Register<ValidationSummary, bool>("HasErrors",
                false);
        public static readonly StyledProperty<bool> HasDisplayedErrorsProperty =
            DependencyProperty.Register<ValidationSummary, bool>("HasDisplayedErrors",
                false);
        public static readonly StyledProperty<object> HeaderProperty =
            DependencyProperty.Register<ValidationSummary, object>("Header");
        public static readonly StyledProperty<DataTemplate> HeaderTemplateProperty =
            DependencyProperty.Register<ValidationSummary, DataTemplate>("HeaderTemplate");
        public static readonly StyledProperty<ControlTheme> SummaryListBoxStyleProperty =
            DependencyProperty.Register<ValidationSummary, ControlTheme>("SummaryListBoxStyle");
        public static readonly StyledProperty<UIElement> TargetProperty =
            DependencyProperty.Register<ValidationSummary, UIElement>("Target");

        public event EventHandler<FocusingInvalidControlEventArgs> FocusingInvalidControl;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;    

        static ValidationSummary()
        {
            ShowErrorsInSummaryProperty.Changed.AddClassHandler<ValidationSummary>(OnShowErrorsInSummaryPropertyChanged);
            FilterProperty.Changed.AddClassHandler<ValidationSummary>(OnFilterPropertyChanged);
            HasErrorsProperty.Changed.AddClassHandler<ValidationSummary>(OnHasErrorsPropertyChanged);
            HasDisplayedErrorsProperty.Changed.AddClassHandler<ValidationSummary>(OnHasDisplayedErrorsPropertyChanged);
            HeaderProperty.Changed.AddClassHandler<ValidationSummary>(OnHasHeaderPropertyChanged);
            TargetProperty.Changed.AddClassHandler<ValidationSummary>(OnTargetPropertyChanged);
            IsEnabledProperty.Changed.AddClassHandler<ValidationSummary>(ValidationSummary_IsEnabledChanged);
        }

        protected override Type StyleKeyOverride => typeof(ValidationSummary);

        public ValidationSummary()
        {
            this._errors = new ValidationItemCollection();
            this._validationSummaryItemDictionary = new Dictionary<string, ValidationSummaryItem>();
            this._displayedErrors = new ValidationItemCollection();
            this._errors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(this.Errors_CollectionChanged);
            if (Design.IsDesignMode)
            {
                this.Errors.Add(new ValidationSummaryItem(Avalonia.Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
                this.Errors.Add(new ValidationSummaryItem(Avalonia.Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
                this.Errors.Add(new ValidationSummaryItem(Avalonia.Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
            }
        }

        internal static int CompareValidationSummaryItems(ValidationSummaryItem x, ValidationSummaryItem y)
        {
            int num;
            if (ReferencesAreValid(x, y, out num))
            {
                if (TryCompareReferences(x.ItemType, y.ItemType, out num))
                {
                    return num;
                }
                var objA = (x.Sources.Count > 0) ? x.Sources[0].Control : null;
                var objB = (y.Sources.Count > 0) ? y.Sources[0].Control : null;
                if (!ReferenceEquals(objA, objB))
                {
                    if (!ReferencesAreValid(objA, objB, out num))
                    {
                        return num;
                    }
                    if (objA.TabIndex != objB.TabIndex)
                    {
                        return objA.TabIndex.CompareTo(objB.TabIndex);
                    }
                    num = SortByVisualTreeOrdering(objA, objB);
                    if (num != 0)
                    {
                        return num;
                    }
                    if (TryCompareReferences(objA.Name, objB.Name, out num))
                    {
                        return num;
                    }
                }
                if (!TryCompareReferences(x.MessageHeader, y.MessageHeader, out num))
                {
                    TryCompareReferences(x.Message, y.Message, out num);
                }
            }
            return num;
        }

        private void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ValidationSummaryItem item in e.OldItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= new PropertyChangedEventHandler(this.ValidationSummaryItem_PropertyChanged);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (ValidationSummaryItem item2 in e.NewItems)
                {
                    if (item2 != null)
                    {
                        item2.PropertyChanged += new PropertyChangedEventHandler(this.ValidationSummaryItem_PropertyChanged);
                    }
                }
            }
            this.HasErrors = this._errors.Count > 0;
            this.UpdateDisplayedErrors();
        }

        private void ErrorsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == global::Avalonia.Input.Key.Enter)
            {
                this.ExecuteClick(sender);
            }
        }

        private void ErrorsListBox_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
        {
            this.ExecuteClick(sender);
        }

        private void ErrorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectionChanged = this.SelectionChanged;
            selectionChanged?.Invoke(this, e);
        }

        private void ExecuteClick(object sender)
        {
            var box = sender as ListBox;
            if (box != null)
            {
                var selectedItem = box.SelectedItem as ValidationSummaryItem;
                if ((selectedItem != null) && this.FocusControlsOnClick)
                {
                    if (selectedItem.Sources.Count == 0)
                    {
                        this._currentValidationSummaryItemSource = null;
                    }
                    else if (FindMatchingErrorSource(selectedItem.Sources, this._currentValidationSummaryItemSource) < 0)
                    {
                        this._currentValidationSummaryItemSource = selectedItem.Sources[0];
                    }
                    var e = new FocusingInvalidControlEventArgs(selectedItem, this._currentValidationSummaryItemSource);
                    this.OnFocusingInvalidControl(e);
                    var peer = ControlAutomationPeer.FromElement(this) as ValidationSummaryAutomationPeer;
#if false
                    if ((peer != null) && AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
                    {
                        peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
                    }
#endif
                    if ((!e.Handled && (e.Target != null)) && (e.Target.Control != null))
                    {
                        e.Target.Control.Focus();
                    }
                    if (selectedItem.Sources.Count > 0)
                    {
                        var num = FindMatchingErrorSource(selectedItem.Sources, e.Target);
                        num = (num < 0) ? 0 : (++num % selectedItem.Sources.Count);
                        this._currentValidationSummaryItemSource = selectedItem.Sources[num];
                    }
                }
            }
        }

        internal void ExecuteClickInternal()
        {
            this.ExecuteClick(this.ErrorsListBoxInternal);
        }

        private static int FindMatchingErrorSource(IList<ValidationSummaryItemSource> sources, ValidationSummaryItemSource sourceToFind)
        {
            if (sources != null)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i].Equals(sourceToFind))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal string GetHeaderString()
        {
            string validationSummaryHeaderError;
            if (this._displayedErrors.Count == 1)
            {
                validationSummaryHeaderError = Avalonia.Properties.Resources.ValidationSummaryHeaderError;
            }
            else
            {
                var args = new object[] { this._displayedErrors.Count };
                validationSummaryHeaderError = string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.ValidationSummaryHeaderErrors, args);
            }
            return validationSummaryHeaderError;
        }

        public static bool GetShowErrorsInSummary(DependencyObject inputControl)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            return (bool) inputControl.GetValue(ShowErrorsInSummaryProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (this._errorsListBox != null)
            {
                this._errorsListBox.PointerReleased -= this.ErrorsListBox_MouseLeftButtonUp;
                this._errorsListBox.KeyDown -= this.ErrorsListBox_KeyDown;
                this._errorsListBox.SelectionChanged -= this.ErrorsListBox_SelectionChanged;
            }
            this._errorsListBox = e.NameScope.Find<ListBox>("SummaryListBox");
            if (this._errorsListBox != null)
            {
                this._errorsListBox.PointerReleased += this.ErrorsListBox_MouseLeftButtonUp;
                this._errorsListBox.KeyDown += this.ErrorsListBox_KeyDown;
                this._errorsListBox.ItemsSource = this.DisplayedErrors;
                this._errorsListBox.SelectionChanged += this.ErrorsListBox_SelectionChanged;
            }
            this._headerContentControl = e.NameScope.Find<ContentControl>("HeaderContentControl");
            this.UpdateDisplayedErrors();
            this.UpdateCommonState(false);
            this.UpdateValidationState(false);
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new ValidationSummaryAutomationPeer(this);

        private static void OnFilterPropertyChanged(ValidationSummary summary, DependencyPropertyChangedEventArgs e)
        {
            summary.UpdateDisplayedErrors();
        }

        protected virtual void OnFocusingInvalidControl(FocusingInvalidControlEventArgs e)
        {
            var focusingInvalidControl = this.FocusingInvalidControl;
            focusingInvalidControl?.Invoke(this, e);
        }

        private static void OnHasDisplayedErrorsPropertyChanged(ValidationSummary summary, DependencyPropertyChangedEventArgs e)
        {
            if (!summary.AreHandlersSuspended())
            {
                summary.SetValueNoCallback(HasDisplayedErrorsProperty, e.OldValue);
                var args = new object[] { "HasDisplayedErrors" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnHasErrorsPropertyChanged(ValidationSummary summary, DependencyPropertyChangedEventArgs e)
        {
            if ((summary != null) && !summary.AreHandlersSuspended())
            {
                summary.SetValueNoCallback(HasErrorsProperty, e.OldValue);
                var args = new object[] { "HasErrors" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Avalonia.Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnHasHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var summary = d as ValidationSummary;
            if (summary != null)
            {
                summary.UpdateHeaderText();
            }
        }

        private static void OnShowErrorsInSummaryPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var parent = (Application.Current != null) ? (TopLevel.GetTopLevel(o as Visual) as FrameworkElement) : null;

            if (parent != null)
            {
                UpdateDisplayedErrorsOnAllValidationSummaries(parent);
            }
        }

        private static void OnTargetPropertyChanged(ValidationSummary summary, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = e.OldValue as FrameworkElement;

            summary._subscription?.Dispose();

            var newValue = e.NewValue as FrameworkElement;
            if (newValue != null)
            {
                var observable = newValue.GetPropertyChangedObservable(DataValidationErrors.ErrorsProperty);
                summary._subscription = observable.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(c => summary.Target_BindingValidationError(c)));
            }

            summary._errors.ClearErrors(ValidationSummaryItemType.PropertyError);
            summary.UpdateDisplayedErrors();
        }

        private static bool ReferencesAreValid(object x, object y, out int val)
        {
            if (x == null)
            {
                val = (y == null) ? 0 : -1;
                return false;
            }
            if (y == null)
            {
                val = 1;
                return false;
            }
            val = 0;
            return true;
        }

        public static void SetShowErrorsInSummary(DependencyObject inputControl, bool value)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            inputControl.SetValue(ShowErrorsInSummaryProperty, value);
        }

        internal static int SortByVisualTreeOrdering(DependencyObject controlX, DependencyObject controlY)
        {
            if (((controlX != null) && (controlY != null)) && !ReferenceEquals(controlX, controlY))
            {
                var list = new List<DependencyObject>();
                var item = controlX;
                list.Add(item);
                while ((item = VisualTreeHelper.GetParent(item)) != null)
                {
                    list.Add(item);
                }
                item = controlY;
                var objB = item;
                while (true)
                {
                    item = VisualTreeHelper.GetParent(item);
                    if (item == null)
                    {
                        return 0;
                    }
                    var index = list.IndexOf(item);
                    if (index == 0)
                    {
                        return -1;
                    }
                    if (index > 0)
                    {
                        var obj4 = list[index - 1];
                        if (obj4 == null)
                        {
                            break;
                        }
                        if (objB == null)
                        {
                            break;
                        }
                        var childrenCount = VisualTreeHelper.GetChildrenCount(item);
                        for (var i = 0; i < childrenCount; i++)
                        {
                            var child = VisualTreeHelper.GetChild(item, i);
                            if (ReferenceEquals(child, objB))
                            {
                                return 1;
                            }
                            if (ReferenceEquals(child, obj4))
                            {
                                return -1;
                            }
                        }
                    }
                    objB = item;
                }
            }
            return 0;
        }

        private void Target_BindingValidationError(AvaloniaPropertyChangedEventArgs e)
        {
            var originalSource = e.Sender as FrameworkElement;
            
            if ((e != null) && (originalSource != null))
            {
                var currentValue = DataValidationErrors.GetErrors(originalSource);
                if (currentValue == null || !currentValue.Any())
                {
                    this._errors.Clear();
                    this._validationSummaryItemDictionary.Clear();
                }
                else
                {
                    var updatedKeys = new HashSet<string>();

                    // add new keys or touch existing ones
                    foreach (var error in currentValue.Cast<ValidationResult>())
                    {
                        string name;
                        var message = error.ErrorMessage;
                        if (!string.IsNullOrEmpty(originalSource.Name))
                        {
                            name = originalSource.Name;
                        }
                        else
                        {
                            name = originalSource.GetHashCode().ToString(CultureInfo.InvariantCulture);
                        }
                        var key = name + message;

                        if (this._validationSummaryItemDictionary.ContainsKey(key))
                        {
                            var item = this._validationSummaryItemDictionary[key];
                            this._errors.Remove(item);
                            this._validationSummaryItemDictionary.Remove(key);
                        }

                        if (GetShowErrorsInSummary(originalSource))
                        {
                            // TODO: support single error affecting multiple fields
                            string messageHeader = error.MemberNames.First();
                            var metadata = ValidationHelper.ParseMetadata(messageHeader, originalSource.DataContext);
                            if (metadata != null)
                            {
                                messageHeader = metadata.Caption;
                            }
                            var item = new ValidationSummaryItem(message, messageHeader, ValidationSummaryItemType.PropertyError, new ValidationSummaryItemSource(messageHeader, originalSource as Control), null);
                            this._errors.Add(item);
                            this._validationSummaryItemDictionary[key] = item;
                            updatedKeys.Add(key);
                        }
                    }

                    // deleted unused keys
                    foreach (var key in this._validationSummaryItemDictionary.Keys)
                    {
                        if (!updatedKeys.Contains(key))
                        {
                            var item = this._validationSummaryItemDictionary[key];
                            this._errors.Remove(item);
                            this._validationSummaryItemDictionary.Remove(key);
                        }
                    }
                }
            }
        }

        private static bool TryCompareReferences(object x, object y, out int returnVal)
        {
            if (((x == null) && (y == null)) || ((x != null) && x.Equals(y)))
            {
                returnVal = 0;
                return false;
            }
            if (ReferencesAreValid(x, y, out returnVal))
            {
                var comparable = x as IComparable;
                var comparable2 = y as IComparable;
                if ((comparable == null) || (comparable2 == null))
                {
                    returnVal = 0;
                    return false;
                }
                returnVal = comparable.CompareTo(comparable2);
            }
            return true;
        }

        private void UpdateCommonState(bool useTransitions)
        {
#if false
            if (base.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
#endif
        }

        private void UpdateDisplayedErrors()
        {
            var list = new List<ValidationSummaryItem>();
            foreach (var item in this.Errors)
            {
                if (item == null)
                {
                    continue;
                }
                if (((item.ItemType == ValidationSummaryItemType.ObjectError) && ((this.Filter & ValidationSummaryFilters.ObjectErrors) != ValidationSummaryFilters.None)) || ((item.ItemType == ValidationSummaryItemType.PropertyError) && ((this.Filter & ValidationSummaryFilters.PropertyErrors) != ValidationSummaryFilters.None)))
                {
                    list.Add(item);
                }
            }
            list.Sort(new Comparison<ValidationSummaryItem>(ValidationSummary.CompareValidationSummaryItems));
            this._displayedErrors.Clear();
            foreach (var item2 in list)
            {
                this._displayedErrors.Add(item2);
            }
            this.UpdateValidationState(true);
            this.UpdateHeaderText();
        }

        private static void UpdateDisplayedErrorsOnAllValidationSummaries(DependencyObject parent)
        {
            if (parent != null)
            {
                var summary = parent as ValidationSummary;
                if (summary != null)
                {
                    summary.UpdateDisplayedErrors();
                }
                else
                {
                    for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                    {
                        var child = VisualTreeHelper.GetChild(parent, i);
                        UpdateDisplayedErrorsOnAllValidationSummaries(child as AvaloniaObject);
                    }
                }
            }
        }

        private void UpdateHeaderText()
        {
            if (this._headerContentControl != null)
            {
                if (this.Header != null)
                {
                    this._headerContentControl.Content = this.Header;
                }
                else
                {
                    this._headerContentControl.Content = this.GetHeaderString();
                }
            }
        }

        private void UpdateValidationState(bool useTransitions)
        {
            this.HasDisplayedErrors = this._displayedErrors.Count > 0;
#if false
            VisualStateManager.GoToState(this, this.HasDisplayedErrors ? "HasErrors" : "Empty", useTransitions);
#endif
        }

        private static void ValidationSummary_IsEnabledChanged(ValidationSummary sender, DependencyPropertyChangedEventArgs e)
        {
            sender.UpdateCommonState(true);
        }

        private void ValidationSummaryItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemType")
            {
                this.UpdateDisplayedErrors();
            }
        }

        public ControlTheme ErrorStyle
        {
            get => 
                (base.GetValue(ErrorStyleProperty) as ControlTheme);
            set => 
                base.SetValue(ErrorStyleProperty, value);
        }

        public ValidationSummaryFilters Filter
        {
            get => 
                ((ValidationSummaryFilters) base.GetValue(FilterProperty));
            set => 
                base.SetValue(FilterProperty, value);
        }

        public bool FocusControlsOnClick
        {
            get => 
                ((bool) base.GetValue(FocusControlsOnClickProperty));
            set => 
                base.SetValue(FocusControlsOnClickProperty, value);
        }

        public bool HasErrors
        {
            get => 
                ((bool) base.GetValue(HasErrorsProperty));
            internal set => 
                this.SetValueNoCallback(HasErrorsProperty, value);
        }

        public bool HasDisplayedErrors
        {
            get => 
                ((bool) base.GetValue(HasDisplayedErrorsProperty));
            internal set => 
                this.SetValueNoCallback(HasDisplayedErrorsProperty, value);
        }

        public object Header
        {
            get => 
                base.GetValue(HeaderProperty);
            set => 
                base.SetValue(HeaderProperty, value);
        }

        public DataTemplate HeaderTemplate
        {
            get => 
                (base.GetValue(HeaderTemplateProperty) as DataTemplate);
            set => 
                base.SetValue(HeaderTemplateProperty, value);
        }

        public ControlTheme SummaryListBoxStyle
        {
            get => 
                (base.GetValue(SummaryListBoxStyleProperty) as ControlTheme);
            set => 
                base.SetValue(SummaryListBoxStyleProperty, value);
        }

        public UIElement Target
        {
            get => 
                (base.GetValue(TargetProperty) as UIElement);
            set => 
                base.SetValue(TargetProperty, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<ValidationSummaryItem> Errors =>
            this._errors;

        public System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationSummaryItem> DisplayedErrors =>
            new System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationSummaryItem>(this._displayedErrors);

        internal new bool Initialized =>
            true;

        internal ListBox ErrorsListBoxInternal =>
            this._errorsListBox;

        internal ContentControl HeaderContentControlInternal =>
            this._headerContentControl;
    }
}

