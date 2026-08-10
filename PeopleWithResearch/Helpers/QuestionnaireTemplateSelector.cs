using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using PeopleWithResearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PeopleWithResearch
{
    public class QuestionnaireTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropdownTemplate { get; set; }
        public DataTemplate MultiTemplate { get; set; }
        public DataTemplate PictureTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }
        public DataTemplate SliderTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not QuestionAnswerJson q) return TextTemplate;

            return q.type switch
            {
                "dropdown" => DropdownTemplate,
                "multiselection" => MultiTemplate,
                "picture" => PictureTemplate,
                "yesno" => DropdownTemplate,
                "checkbox" => MultiTemplate,
                "radio" => DropdownTemplate,
                "date" => DateTemplate,
                "slider" => SliderTemplate,
                "text" => TextTemplate,
                _ => DropdownTemplate
            };
        }
    }

    public class CompletedTemplateSelector : DataTemplateSelector
    {

        public DataTemplate CompletedSingleTemplate { get; set; }
        public DataTemplate CompletedMultiTemplate { get; set; }
        public DataTemplate CompletedDateTemplate { get; set; }
        public DataTemplate CompletedTextTemplate { get; set; }
        public DataTemplate CompletedPictureTemplate { get; set; }
        public DataTemplate CompletedSliderTemplate { get; set; }

        public DataTemplate SymptomTemplate => new DataTemplate(() =>
        {
            var rootLayout = new VerticalStackLayout
            {
                Margin = new Thickness(0, 12, 0, 0),
                IsClippedToBounds = false,
                Spacing = 0
            };


            var groupsLayout = new VerticalStackLayout();
            groupsLayout.SetBinding(BindableLayout.ItemsSourceProperty, "symptom_groups");

            BindableLayout.SetItemTemplate(groupsLayout, new DataTemplate(() =>
            {
                var cardBorder = new Border
                {
                    Margin = new Thickness(0, 4),
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#EAEFF4"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    StrokeThickness = 1
                };

                var cardContent = new VerticalStackLayout
                {
                    Margin = new Thickness(0, 12, 0, 0),
                    Spacing = 0
                };

                // Group header
                var groupHeader = new Label
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    CharacterSpacing = 1,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "OpenSansSemiBold",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#62727B")
                };
                groupHeader.SetBinding(Label.TextProperty, "group");
                cardContent.Children.Add(groupHeader);

                // symptoms
                var symptomsLayout = new VerticalStackLayout { Spacing = 5 };
                symptomsLayout.SetBinding(BindableLayout.ItemsSourceProperty, "symptoms");

                BindableLayout.SetItemTemplate(symptomsLayout, new DataTemplate(() =>
                {
                    var symptomContainer = new VerticalStackLayout
                    {
                        Padding = new Thickness(12),
                        Spacing = 2
                    };
                    var symptomLabel = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "OpenSansSemiBold",
                        FontSize = 12,
                        TextColor = Colors.Black,
                        HorizontalOptions = LayoutOptions.Fill
                    };
                    symptomLabel.SetBinding(Label.TextProperty, "label");
                    symptomContainer.Children.Add(symptomLabel);

                    // Symptom Chippies
                    var chipsFlex = new FlexLayout
                    {
                        AlignContent = FlexAlignContent.Start,
                        AlignItems = FlexAlignItems.Start,
                        Direction = FlexDirection.Row,
                        JustifyContent = FlexJustify.Start,
                        MinimumHeightRequest = 36,
                        Wrap = FlexWrap.Wrap
                    };
                    chipsFlex.SetBinding(BindableLayout.ItemsSourceProperty, "SymptomData");

                    BindableLayout.SetItemTemplate(chipsFlex, new DataTemplate(() =>
                    {
                        var chipBorder = new Border
                        {
                            Margin = new Thickness(0, 3, 6, 3),
                            Padding = new Thickness(12, 6),
                            StrokeThickness = 0,
                            StrokeShape = new RoundRectangle { CornerRadius = 15 }
                        };

                        chipBorder.SetBinding(Border.BackgroundColorProperty, "colour");
                        chipBorder.SetBinding(Border.StrokeProperty, "colour");

                        var chipContent = new HorizontalStackLayout
                        {
                            Spacing = 6,
                            VerticalOptions = LayoutOptions.Center
                        };

                        // Day label
                        var dayLabel = new Label
                        {
                            FontFamily = "OpenSansRegular",
                            FontSize = 12,
                            TextColor = Colors.Gray
                        };
                        dayLabel.SetBinding(Label.TextProperty, "label");

                        // Severity
                        var intensityLabel = new Label
                        {
                            FontAttributes = FontAttributes.Bold,
                            FontFamily = "OpenSansSemiBold",
                            FontSize = 12
                        };
                        intensityLabel.SetBinding(Label.TextProperty, new Binding("text", stringFormat: "{0}"));
                        intensityLabel.SetBinding(Label.TextColorProperty, "textcolour");

                        chipContent.Children.Add(dayLabel);
                        chipContent.Children.Add(intensityLabel);

                        chipBorder.Content = chipContent;
                        return chipBorder;
                    }));

                    symptomContainer.Children.Add(chipsFlex);

                    symptomContainer.Children.Add(new BoxView
                    {
                        Background = Colors.Transparent,
                        Margin = new Thickness(5, 12, 5, -14),
                        BackgroundColor = Colors.LightGray,
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        Opacity = 0.4
                    });

                    return symptomContainer;
                }));

                cardContent.Children.Add(symptomsLayout);
                cardBorder.Content = cardContent;

                return cardBorder;
            }));

            rootLayout.Children.Add(groupsLayout);


            var rashLayout = new VerticalStackLayout
            {
                Margin = new Thickness(0, 12, 0, 0)
            };
            rashLayout.SetBinding(BindableLayout.ItemsSourceProperty, "rash_followup.questions");

            BindableLayout.SetItemTemplate(rashLayout, new DataTemplate(() =>
            {
                var border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 12),
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#EAEFF4"),
                    StrokeShape = new RoundRectangle { CornerRadius = 10 }
                };

                var content = new VerticalStackLayout
                {
                    Padding = new Thickness(10),
                    Spacing = 6
                };

                var label = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "OpenSansSemiBold",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#031926")
                };
                label.SetBinding(Label.TextProperty, "label");
                content.Children.Add(label);

                var optionsLayout = new VerticalStackLayout();
                optionsLayout.SetBinding(BindableLayout.ItemsSourceProperty, "options");

                BindableLayout.SetItemTemplate(optionsLayout, new DataTemplate(() =>
                {
                    var stack = new StackLayout();

                    var titleLabel = new Label
                    {
                        Padding = new Thickness(5),
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "OpenSansRegular",
                        FontSize = 12,
                        TextColor = Colors.Gray
                    };
                    titleLabel.SetBinding(Label.TextProperty, "title");

                    var optionBorder = new Border
                    {
                        Margin = new Thickness(0, 4),
                        BackgroundColor = Color.FromArgb("#E6F1FB"),
                        Stroke = Color.FromArgb("#009FE3"),
                        StrokeShape = new RoundRectangle { CornerRadius = 8 }
                    };

                    var optionLabel = new Label
                    {
                        Padding = new Thickness(10),
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "OpenSansSemiBold",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#031926")
                    };
                    optionLabel.SetBinding(Label.TextProperty, "text");

                    optionBorder.Content = optionLabel;

                    stack.Children.Add(titleLabel);
                    stack.Children.Add(optionBorder);

                    return stack;
                }));

                content.Children.Add(optionsLayout);
                border.Content = content;

                return border;
            }));

            rootLayout.Children.Add(rashLayout);

            var dailyLayout = new VerticalStackLayout();
            dailyLayout.SetBinding(BindableLayout.ItemsSourceProperty, "daily_impact.questions");

            BindableLayout.SetItemTemplate(dailyLayout, new DataTemplate(() =>
            {
                var border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 12),
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#EAEFF4"),
                    StrokeShape = new RoundRectangle { CornerRadius = 10 }
                };

                var content = new VerticalStackLayout
                {
                    Padding = new Thickness(10),
                    Spacing = 6
                };

                var label = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "OpenSansSemiBold",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#031926")
                };
                label.SetBinding(Label.TextProperty, "label");
                content.Children.Add(label);

                var optionsLayout = new VerticalStackLayout();
                optionsLayout.SetBinding(BindableLayout.ItemsSourceProperty, "options");

                BindableLayout.SetItemTemplate(optionsLayout, new DataTemplate(() =>
                {
                    var stack = new StackLayout();

                    var titleLabel = new Label
                    {
                        Padding = new Thickness(5),
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "OpenSansRegular",
                        FontSize = 12,
                        TextColor = Colors.Gray
                    };
                    titleLabel.SetBinding(Label.TextProperty, "title");

                    var optionBorder = new Border
                    {
                        Margin = new Thickness(0, 4),
                        BackgroundColor = Color.FromArgb("#E6F1FB"),
                        Stroke = Color.FromArgb("#009FE3"),
                        StrokeShape = new RoundRectangle { CornerRadius = 8 }
                    };

                    var optionLabel = new Label
                    {
                        Padding = new Thickness(10),
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "OpenSansSemiBold",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#031926")
                    };
                    optionLabel.SetBinding(Label.TextProperty, "text");

                    optionBorder.Content = optionLabel;

                    stack.Children.Add(titleLabel);
                    stack.Children.Add(optionBorder);

                    return stack;
                }));

                content.Children.Add(optionsLayout);
                border.Content = content;

                return border;
            }));

            rootLayout.Children.Add(dailyLayout);

            return rootLayout;
        });


        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not QuestionAnswerJson q || string.IsNullOrWhiteSpace(q.type))
                return CompletedTextTemplate;

            if (q.type.Equals("symptom_grid", StringComparison.OrdinalIgnoreCase))
                return SymptomTemplate;

            return q.type.ToLowerInvariant() switch
            {
                "dropdown" => CompletedSingleTemplate,
                "radio" => CompletedSingleTemplate,
                "yesno" => CompletedSingleTemplate,
                "slider" => CompletedSliderTemplate,
                "multiselection" => CompletedMultiTemplate,
                "checkbox" => CompletedMultiTemplate,
                "file_upload" => CompletedPictureTemplate,
                "picture" => CompletedPictureTemplate,
                "date" => CompletedDateTemplate,
                "text" => CompletedTextTemplate,
                _ => CompletedTextTemplate
            };
        }
    }

    public class HexToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexString && !string.IsNullOrWhiteSpace(hexString))
        {
            try
            {
                return Color.FromArgb(hexString);
            }
            catch
            {
                return Colors.LightGray;
            }
        }

        return Colors.LightGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

    //public class CompletedTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate DropdownTemplate { get; set; }
    //    public DataTemplate MultiTemplate { get; set; }
    //    public DataTemplate PictureTemplate { get; set; }
    //    public DataTemplate TextTemplate { get; set; }
    //    public DataTemplate DateTemplate { get; set; }
    //    public DataTemplate SliderTemplate { get; set; }
    //    public DataTemplate SymptomTemplate { get; set; }
    //    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    //    {
    //        if (item is not QuestionAnswerJson q) return TextTemplate;

    //        return q.type.ToLowerInvariant() switch
    //        {
    //            "dropdown" => DropdownTemplate,
    //            "multiselection" => MultiTemplate,
    //            "picture" => PictureTemplate,
    //            "yesno" => DropdownTemplate,
    //            "checkbox" => MultiTemplate,
    //            "radio" => DropdownTemplate,
    //            "date" => DateTemplate,
    //            "symptom_grid" => SymptomTemplate,
    //            "slider" => SliderTemplate,
    //            "text" => TextTemplate,
    //            _ => DropdownTemplate
    //        };
    //    }
    //}
}
