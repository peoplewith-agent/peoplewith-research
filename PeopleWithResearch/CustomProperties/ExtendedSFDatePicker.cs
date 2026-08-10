using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.Picker;


namespace PeopleWithResearch
{
    /// <summary>
    /// Done entry Class to Extend Entry Class
    /// </summary>
	public class ExtendedSFDatePicker : SfDatePicker
    {
        public string IDValue { get; set; } //Name property for the entry
        public string IDRecord { get; set; }


        public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
                                                         propertyName: "IDValue",
                                                         returnType: typeof(string),
                                                           declaringType: typeof(ExtendedSFDatePicker),
                                                         defaultValue: "",
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         propertyChanged: NamePropertyChanged);

        public static readonly BindableProperty IDRecordProperty = BindableProperty.Create(
                                                  propertyName: "IDRecord",
                                                  returnType: typeof(string),
                                             declaringType: typeof(ExtendedSFDatePicker),
                                                  defaultValue: "",
                                                  defaultBindingMode: BindingMode.TwoWay,
                                                     propertyChanged: IDPropertyChanged);

        private static void NamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedSFDatePicker)bindable;
            control.IDValue = newValue.ToString();
        }

        private static void IDPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedSFDatePicker)bindable;
            control.IDRecord = newValue.ToString();
        }
    }


}
