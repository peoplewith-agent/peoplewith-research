using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PeopleWithResearch;

public class LanguageOption : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Name { get; set; } = string.Empty;
    public string FlagImage { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageTitle { get; set; } = string.Empty;
    public string LanguageDescription { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedColour));
            OnPropertyChanged(nameof(SelectedBackground));
        }
    }
    public Color SelectedColour => IsSelected ? Color.FromArgb("#009FE3") : Colors.White;
    public Color SelectedBackground => IsSelected ? Color.FromArgb("#E6F1FB") : Colors.White;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}