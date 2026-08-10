using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.ListView;

namespace PeopleWithResearch
{
    class ExtendedSFListview : SfListView
    {

        public string IsVisibleList { get; set; } //Name property for the entry
        public string IDValue { get; set; }

        public static readonly BindableProperty IsVisibleListProperty = BindableProperty.Create(
                                                       propertyName: "IsVisibleList",
                                                       returnType: typeof(string),
                                                         declaringType: typeof(ExtendedSFListview),
                                                       defaultValue: "",
                                                       defaultBindingMode: BindingMode.TwoWay,
                                                       propertyChanged: NamePropertyChanged);

        public static readonly BindableProperty IDValueProperty = BindableProperty.Create(
                                                  propertyName: "IDValue",
                                                  returnType: typeof(string),
                                                    declaringType: typeof(ExtendedSFListview),
                                                  defaultValue: "",
                                                  defaultBindingMode: BindingMode.TwoWay,
                                                  propertyChanged: IDValuePropertyChanged);

        private static void NamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedSFListview)bindable;
            control.IsVisibleList = newValue.ToString();
        }


        private static void IDValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ExtendedSFListview)bindable;
            control.IDValue = newValue.ToString();
        }






    }
}
