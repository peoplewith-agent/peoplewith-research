namespace PeopleWithResearch;

public class PolicySectionTemplateSelector : DataTemplateSelector
{
    public DataTemplate HeadingTemplate { get; set; } = null!;
    public DataTemplate SubheadingTemplate { get; set; } = null!;
    public DataTemplate ParagraphTemplate { get; set; } = null!;
    public DataTemplate CalloutTemplate { get; set; } = null!;
    public DataTemplate BulletListTemplate { get; set; } = null!;
    public DataTemplate KeyValueListTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var type = (item as PolicySection)?.Type;
        return type switch
        {
            "heading" => HeadingTemplate,
            "subheading" => SubheadingTemplate,
            "callout" => CalloutTemplate,
            "bullet_list" => BulletListTemplate,
            "keyvalue" => KeyValueListTemplate,
            _ => ParagraphTemplate,
        };
    }
}