using Microsoft.Maui.Controls;

namespace PeopleWithResearch;

public class PolicySectionTemplateSelector : DataTemplateSelector
{
    public DataTemplate HeadingTemplate { get; set; }
    public DataTemplate SubheadingTemplate { get; set; }
    public DataTemplate ParagraphTemplate { get; set; }
    public DataTemplate CalloutTemplate { get; set; }
    public DataTemplate BulletListTemplate { get; set; }
    public DataTemplate KeyValueListTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is PolicySection section)
        {
            return section.Type switch
            {
                "heading" => HeadingTemplate,
                "subheading" => SubheadingTemplate,
                "paragraph" => ParagraphTemplate,
                "callout" => CalloutTemplate,
                "bullet_list" => BulletListTemplate,
                "key_value_list" => KeyValueListTemplate,
                _ => ParagraphTemplate
            };
        }
        return ParagraphTemplate;
    }
}
