using Microsoft.Maui.Controls;
using System;
using System.Linq;

namespace PeopleWithResearch
{
    public class NhsNumberMaskBehavior : Behavior<Entry>
    {
        private bool _isUpdating;

        public static readonly BindableProperty IsValidProperty =
            BindableProperty.Create(
                nameof(IsValid),
                typeof(bool),
                typeof(NhsNumberMaskBehavior),
                false,
                BindingMode.TwoWay);

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || sender is not Entry entry)
                return;

            // Guard against nulls (common on Android clearing)
            var oldText = e.OldTextValue ?? string.Empty;
            var newText = e.NewTextValue ?? string.Empty;

            // If the change was just adding a space at the end from our own formatting, stop.
            if (newText.Length < oldText.Length && oldText.Trim() == newText)
                return;

            try
            {
                _isUpdating = true;

                string digitsOnly = new string(newText.Where(char.IsDigit).Take(10).ToArray());
                string formatted = FormatNhsNumber(digitsOnly);
                if (entry.Text != formatted)
                {
#if ANDROID
                    var handler = entry.Handler as Microsoft.Maui.Handlers.EntryHandler;
                    var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;

                    if (editText != null)
                    {
                        editText.EmojiCompatEnabled = false;
                        editText.SetTextKeepState(formatted);
                    }
                    else
                    {
                        entry.Text = formatted;
                    }
#else
            entry.Text = formatted;
#endif
                }

                IsValid = digitsOnly.Length == 10 && CheckNhsValid(digitsOnly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Format Error: {ex.Message}");
                IsValid = false;
            }
            finally
            {
                _isUpdating = false;
            }
        }
        private static string FormatNhsNumber(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return string.Empty;

            if (digits.Length > 6)
            {
                return $"{digits[..3]} {digits.Substring(3, 3)} {digits[6..]}";
            }

            if (digits.Length > 3)
            {
                return $"{digits[..3]} {digits[3..]}";
            }

            return digits;
        }

        private static bool CheckNhsValid(string num)
        {
            if (string.IsNullOrWhiteSpace(num))
                return false;

            if (num.Length != 10 || !num.All(char.IsDigit))
                return false;

            int[] weights = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += (num[i] - '0') * weights[i];
            }

            int remainder = sum % 11;
            int checkDigit = 11 - remainder;

            if (checkDigit == 11)
                checkDigit = 0;

            if (checkDigit == 10)
                return false;

            return checkDigit == (num[9] - '0');
        }
    }
}