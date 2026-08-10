using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PeopleWithResearch
{
    /// <summary>
    /// Extended Entryclass which holds a name property
    /// </summary>
    public class ExtendedEntry : Entry
    {
        public string IDValue { get; set; } // Name property for the entry
        public string IDRecord { get; set; }
        public string questionid { get; set; } // Updated to lowercase
        public string TextValue { get; set; }

        // Bindable properties
        public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
            nameof(IDValue), // Using nameof for property name
            typeof(string),
            typeof(ExtendedEntry),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: IDValuePropertyChanged);

        public static readonly BindableProperty IDRecordProperty = BindableProperty.Create(
            nameof(IDRecord), // Using nameof for property name
            typeof(string),
            typeof(ExtendedEntry),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: IDRecordPropertyChanged);

        public static readonly BindableProperty questionidProperty = BindableProperty.Create(
            nameof(questionid), // Updated to lowercase
            typeof(string),
            typeof(ExtendedEntry),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: QuestionIdPropertyChanged);

        public static readonly BindableProperty textProperty = BindableProperty.Create(
           nameof(TextValue), // Updated to lowercase
           typeof(string),
           typeof(ExtendedEntry),
           string.Empty,
           BindingMode.TwoWay,
           propertyChanged: TextPropertyChanged);

        // Property changed handlers
        private static void IDValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedEntry)bindable;
            control.IDValue = newValue?.ToString(); // Use null-conditional operator
        }

        private static void IDRecordPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedEntry)bindable;
            control.IDRecord = newValue?.ToString(); // Use null-conditional operator
        }

        private static void QuestionIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedEntry)bindable;
            control.questionid = newValue?.ToString(); // Updated to lowercase
        }

        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedEntry)bindable;
            control.TextValue = newValue?.ToString();
        }

        //Old (Incase Errors) 

        //public string IDValue { get; set; }

        //public string IDValueee { get; set; }

        //public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
        //                                          propertyName: "IDValue",
        //                                          returnType: typeof(string),
        //                                            declaringType: typeof(ExtendedEntry),
        //                                          defaultValue: "",
        //                                          defaultBindingMode: BindingMode.TwoWay,
        //                                          propertyChanged: IDPropertyChanged);



        //public static readonly BindableProperty IDValueeeProperty = BindableProperty.Create(
        //                                          propertyName: "IDValueee",
        //                                          returnType: typeof(string),
        //                                            declaringType: typeof(ExtendedEntry),
        //                                          defaultValue: "",
        //                                          defaultBindingMode: BindingMode.TwoWay,
        //                                          propertyChanged: IDValueeePropertyChanged);

        //private static void IDPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    var control = (ExtendedEntry)bindable;
        //    control.IDValue = newValue.ToString();
        //}


        //private static void IDValueeePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    var control = (ExtendedEntry)bindable;
        //    control.IDValueee = newValue.ToString();
        //}
    }
}
