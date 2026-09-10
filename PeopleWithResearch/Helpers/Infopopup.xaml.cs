using System.Collections.ObjectModel;
using Mopups.Pages;
using PeopleWithResearch;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace PeopleWithResearch;

public partial class Infopopup : PopupPage
{
	public Infopopup()
	{
		InitializeComponent();
	}


    public Infopopup(string labelname, ObservableCollection<RegField> allfields)
    {
        InitializeComponent();

        BorderInfo.IsVisible = true;
        studylbltitle.Text = labelname;

        string info = GetHelpTextInfo(labelname, allfields);
        if(info != null)
        {
            //studylbl.Text = info;
            ParseTextWithLinks(info);
        }
    }

    public Infopopup(string labelname, string infotext)
    {
        InitializeComponent();
        

        BorderInfo.IsVisible = true;

        studylbltitle.Text = labelname;

        //� string info = GetHelpTextInfo(labelname, allfields);



        studylbl.Text = infotext;

    }

    public Infopopup(string labelname, householdgroupjsondetails itempassed)
    {
        InitializeComponent();


        Borderloading.IsVisible = true;

        loadinglbl.Text = "Loading Baseline Questionnaire...";

        loadingrelationlbl.Text = "You are completing the baseline questionnaire on behalf of " + itempassed.household_individual_name;

    }

    public Infopopup(string labelname)
    {
        InitializeComponent();


        Borderloading.IsVisible = true;

        loadinglbl.Text = "Switching Profile...";

        //loadingrelationlbl.Text = "You are completing the baseline questionnaire on behalf of " + itempassed.household_individual_name;

    }

    private void ParseTextWithLinks(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            studylbl.Text = string.Empty;
            return;
        }

        var formattedString = new FormattedString();

        var linkRegex = new Regex(
            @"((?:https?|ftp):\/\/|www\.)[^\s]+",
            RegexOptions.IgnoreCase);

        int lastIndex = 0;

        var matches = linkRegex.Matches(text);

        if (matches.Count == 0)
        {
            studylbl.Text = text;
            return;
        }

        foreach (Match match in matches)
        {
            if (match.Index > lastIndex)
            {
                formattedString.Spans.Add(new Span
                {
                    Text = text.Substring(lastIndex, match.Index - lastIndex),
                    TextColor = Color.FromArgb("#031926"),
                    FontFamily = "OpenSansRegular",
                    FontSize = 14
                });
            }

            string rawUrl = match.Value;

            string uriPath = rawUrl.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? $"https://{rawUrl}"
                : rawUrl;

            var linkSpan = new Span
            {
                Text = rawUrl,
                TextColor = Color.FromArgb("#009fe3"),
                TextDecorations = TextDecorations.Underline,
                FontAttributes = FontAttributes.Bold
            };

            var tapGesture = new TapGestureRecognizer();

            tapGesture.Tapped += async (s, e) =>
            {
                try
                {
                    await Browser.Default.OpenAsync(uriPath, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            };

            linkSpan.GestureRecognizers.Add(tapGesture);

            formattedString.Spans.Add(linkSpan);

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            formattedString.Spans.Add(new Span
            {
                Text = text.Substring(lastIndex)
            });
        }

        studylbl.FormattedText = formattedString;
    }

    // Ensure you update your Notification constructor to use the parser!
    public Infopopup(ObservableCollection<pushdata> NewNotification)
    {
        try
        {
            InitializeComponent();

            //Set Items 
            var InfoTitle = NewNotification.FirstOrDefault(x => (!string.IsNullOrEmpty(x.key) && x.key.ToLower() == "header"));
            var InfoBody = NewNotification.FirstOrDefault(x => (!string.IsNullOrEmpty(x.key) && x.key.ToLower() == "content"));

            if (InfoTitle != null)
            {
                studylbltitle.Text = InfoTitle.Data;
            }
            else
            {
                studylbltitle.Text = "Apologies";
            }

            if (InfoBody != null)
            {
                studylbl.Text = InfoBody.Data;
            }
            else
            {
                studylbltitle.Text = "Somethings went wrong, the data failed to load properly. Apoligies for any incoinvience";
            }

        }
        catch (Exception Ex)
        {

        }
    }

    public string GetHelpTextInfo(string helpText, ObservableCollection<RegField> config)
    {
        try
        {
            foreach (var field in config)
            {
                // Check main field
                if (!string.IsNullOrEmpty(field.HelpText) && field.HelpText == helpText)
                    return field.HelpTextInfo;

                // Check subfields (if exists)
                if (field.subFields != null)
                {
                    foreach (var sub in field.subFields)
                    {

                        if (sub.Id == "ethnicityDescription" || sub.Id == "nhsNumber")
                        {

                            var titles = sub.HelpText.Split('|');
                            var infos = sub.HelpTextInfo.Split('|');

                            // find which title matches what user clicked
                            int index = Array.IndexOf(titles, helpText);

                            if (index >= 0 && index < infos.Length)
                            {
                                return infos[index];  // return correct part
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(sub.HelpText) && sub.HelpText == helpText)
                                return sub.HelpTextInfo;
                        }
                    }
                }
            }
            return null; // Not found
        }
        catch(Exception ex)
        {
            return null;
        }
    }
}