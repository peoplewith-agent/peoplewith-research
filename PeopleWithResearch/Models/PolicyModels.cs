using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PeopleWithResearch;

public class PolicyDocumentRoot
{
    public PolicyDocumentInfo Document { get; set; } = new();
    public List<PolicySection> Sections { get; set; } = new();
}

public class PolicyDocumentInfo
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}

public class PolicySection
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Title { get; set; }
    public PolicyStyle Style { get; set; } = new();

    [JsonIgnore]
    public List<string> Items =>
        (Text ?? "")
            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

    [JsonIgnore]
    public List<PolicyKeyValue> KeyValueItems =>
        (Text ?? "")
            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .Select(l =>
            {
                int idx = l.IndexOf(':');
                return idx > 0
                    ? new PolicyKeyValue { Key = l[..idx].Trim(), Value = l[(idx + 1)..].Trim() }
                    : new PolicyKeyValue { Key = "", Value = l };
            })
            .ToList();

    [JsonIgnore]
    public Thickness MarginThickness => ParseThickness(Style?.Margin);

    [JsonIgnore]
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private static Thickness ParseThickness(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return new Thickness(0);
        var p = s.Split(',');
        double D(int i) => i < p.Length && double.TryParse(p[i], out var v) ? v : 0;
        return p.Length switch
        {
            1 => new Thickness(D(0)),
            2 => new Thickness(D(0), D(1)),
            4 => new Thickness(D(0), D(1), D(2), D(3)),
            _ => new Thickness(0)
        };
    }
}

public class PolicyKeyValue
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class PolicyStyle
{
    public double FontSize { get; set; } = 14;
    public string FontAttributes { get; set; } = "None";
    public string Margin { get; set; } = "0";
    public bool IsAccentColor { get; set; }
}