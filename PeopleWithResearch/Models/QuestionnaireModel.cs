using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace PeopleWithResearch;

public class QuestionOptions: INotifyPropertyChanged
{
    public string answerid { get; set; }
    public string value { get; set; }
    public string text { get; set; }

    private bool _selected;
    [JsonIgnore]
    public bool selected
    {
        get => _selected;
        set { _selected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class QuestionnaireQuestions : INotifyPropertyChanged
{
    public string id { get; set; }
    public string label { get; set; }
    public string type { get; set; }
    public bool required { get; set; }
    public QuestionOptions[] options { get; set; }
    public string @default { get; set; }
    public string placeholder { get; set; }
    public int order { get; set; }
    public string questionid { get; set; }
    public string image { get; set; }
    public string branchinglogic { get; set; }
    public string usertype { get; set; }

    [JsonIgnore] public string questionnum { get; set; }

    private bool _hasAnswered;
    [JsonIgnore]
    public bool HasAnswered
    {
        get => _hasAnswered;
        set { _hasAnswered = value; OnPropertyChanged(); }
    }

    private bool _showRequired;
    [JsonIgnore]
    public bool ShowRequired
    {
        get => _showRequired;
        set { _showRequired = value; OnPropertyChanged(); }
    }

    private Color _colourBorder = Colors.White;
    [JsonIgnore]
    public Color ColourBorder
    {
        get => _colourBorder;
        set { _colourBorder = value; OnPropertyChanged(); }
    }

    private string _localImagePath;
    [JsonIgnore]
    public string LocalImagePath
    {
        get => _localImagePath;
        set { _localImagePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage)); }
    }

    private string _uploadedFileName;
    [JsonIgnore]
    public string UploadedFileName
    {
        get => _uploadedFileName;
        set { _uploadedFileName = value; OnPropertyChanged(); }
    }

    [JsonIgnore] public bool HasImage => !string.IsNullOrEmpty(LocalImagePath);

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}