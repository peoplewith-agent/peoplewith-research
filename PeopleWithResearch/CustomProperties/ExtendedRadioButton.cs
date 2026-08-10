using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.Buttons;


namespace PeopleWithResearch
{
    /// <summary>
    /// Done entry Class to Extend Entry Class
    /// </summary>
	public class ExtendedRadioButton : SfRadioButton
    {
        public string IDValue
        {
            get => (string)GetValue(IDValueProperty);
            set => SetValue(IDValueProperty, value);
        }

        public string IDRecord
        {
            get => (string)GetValue(IDRecordProperty);
            set => SetValue(IDRecordProperty, value);
        }

        public string questionid
        {
            get => (string)GetValue(questionidProperty);
            set => SetValue(questionidProperty, value);
        }

        public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
            nameof(IDValue), typeof(string), typeof(ExtendedRadioButton), string.Empty, BindingMode.TwoWay);

        public static readonly BindableProperty IDRecordProperty = BindableProperty.Create(
            nameof(IDRecord), typeof(string), typeof(ExtendedRadioButton), string.Empty, BindingMode.TwoWay);

        public static readonly BindableProperty questionidProperty = BindableProperty.Create(
            nameof(questionid), typeof(string), typeof(ExtendedRadioButton), string.Empty, BindingMode.TwoWay);

        //Old (Incase Error)
        //public string IDValue { get; set; } //Name property for the entry
        //public string IDRecord { get; set; }


        //public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
        //                                                 propertyName: "IDValue",
        //                                                 returnType: typeof(string),
        //                                                   declaringType: typeof(ExtendedRadioButton),
        //                                                 defaultValue: "",
        //                                                 defaultBindingMode: BindingMode.TwoWay,
        //                                                 propertyChanged: NamePropertyChanged);

        //public static readonly BindableProperty IDRecordProperty = BindableProperty.Create(
        //                                          propertyName: "IDRecord",
        //                                          returnType: typeof(string),
        //                                     declaringType: typeof(ExtendedRadioButton),
        //                                          defaultValue: "",
        //                                          defaultBindingMode: BindingMode.TwoWay,
        //                                             propertyChanged: IDPropertyChanged);

        //private static void NamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    var control = (ExtendedRadioButton)bindable;
        //    control.IDValue = newValue.ToString();
        //}

        //private static void IDPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    var control = (ExtendedRadioButton)bindable;
        //    control.IDRecord = newValue.ToString();
        //}
    }


}
