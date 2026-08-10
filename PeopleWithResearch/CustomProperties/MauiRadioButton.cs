using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.Buttons;


namespace PeopleWithResearch
{
	public class MauiRadioButton : SfRadioButton
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
            nameof(IDValue), typeof(string), typeof(MauiRadioButton), string.Empty, BindingMode.TwoWay);

        public static readonly BindableProperty IDRecordProperty = BindableProperty.Create(
            nameof(IDRecord), typeof(string), typeof(MauiRadioButton), string.Empty, BindingMode.TwoWay);

        public static readonly BindableProperty questionidProperty = BindableProperty.Create(
            nameof(questionid), typeof(string), typeof(MauiRadioButton), string.Empty, BindingMode.TwoWay);
    }


    }
