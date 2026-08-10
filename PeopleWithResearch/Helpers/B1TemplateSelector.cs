using Microsoft.Maui.Controls;

namespace PeopleWithResearch;

public class B1TemplateSelector : DataTemplateSelector
{
    public DataTemplate DropdownTemplate { get; set; }
    public DataTemplate MultiTemplate { get; set; }
    public DataTemplate PictureTemplate { get; set; }
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate DateTemplate { get; set; }
    public DataTemplate SliderTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not B1Question q) return null;

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