using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PeopleWithResearch;

public class LanguageChangedMessage : ValueChangedMessage<string>
{
    public LanguageChangedMessage(string languageCode) : base(languageCode) { }
}
