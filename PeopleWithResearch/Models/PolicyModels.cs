using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class PolicyDocumentRoot
    {
        [JsonPropertyName("document")]
        public DocumentInfo Document { get; set; }

        [JsonPropertyName("sections")]
        public List<PolicySection> Sections { get; set; }
    }

    public class DocumentInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }
    }

    public class PolicySection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("calloutType")]
        public string CalloutType { get; set; }

        [JsonPropertyName("items")]
        public List<string> Items { get; set; }

        [JsonPropertyName("key_value_items")]
        public List<KeyValueItem> KeyValueItems { get; set; }

        [JsonPropertyName("style")]
        public SectionStyle Style { get; set; }
    }

    public class KeyValueItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class SectionStyle
    {
        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; } = 14;

        [JsonPropertyName("fontAttributes")]
        public string FontAttributes { get; set; } = "None";

        [JsonPropertyName("margin")]
        public string Margin { get; set; } = "0,0,0,8";

        [JsonPropertyName("isAccentColor")]
        public bool IsAccentColor { get; set; }
    }
}