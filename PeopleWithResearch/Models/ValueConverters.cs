using System.Globalization;

namespace PeopleWithResearch.Converters;

/// <summary>
/// Drives SfTextInputLayout.HasError (and similar) from an error-message string property, so
/// each field only needs one bound string instead of a separate bool + string pair.
/// Bind ErrorText="{Binding XError}" and HasError="{Binding XError, Converter={StaticResource StringNotEmptyToBoolConverter}}".
/// </summary>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Was: UpdateOpacity(tickImage, textLabel, isValid) in Imperial.xaml.cs — same 1.0 / 0.2 rule,
/// as a converter so the password-strength ticks can bind directly to
/// PasswordHasMinLength / PasswordHasSpecialChar / PasswordHasCapital / PasswordHasNumber.
/// </summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? 1.0 : 0.2;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
