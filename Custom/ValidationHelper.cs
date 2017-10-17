using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LonestarShowdown.Custom
{
    /// <summary>
    /// </summary>
    public sealed class ValidationRegex
    {
        public static string Name
        {
            get { return @"^([ \u00c0-\u01ffa-zA-Z'\-])+$"; }
        }

        public static string Address
        {
            get { return @"^([ \u00c0-\u01ffa-zA-Z0-9'\-\.])+$"; }
        }

        public static string Phone
        {
            get { return @"^(\+0?1\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$"; }
        }

        public static string Email
        {
            get
            {
                return
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
            }
        }
    }

    public class EndsWithValidationTextRule : ValidationRule
    {
        public string ValueType { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var inputString = value as string;
            if (inputString == null || inputString.Trim() == string.Empty)
            {
                return new ValidationResult(false, string.Empty);
            }
            switch (ValueType)
            {
                case "Name":
                    if (!Regex.IsMatch(inputString, ValidationRegex.Name))
                    {
                        return new ValidationResult(false, "Enter a valid name.");
                    }
                    break;
                case "Email":
                    if (!Regex.IsMatch(inputString.ToLower(), ValidationRegex.Email))
                    {
                        return new ValidationResult(false, "Enter a valid email.");
                    }
                    break;
                case "Phone":
                    if (!Regex.IsMatch(inputString, ValidationRegex.Phone))
                    {
                        return new ValidationResult(false, "Enter a valid phone.");
                    }
                    break;
                case "Address":
                    if (!Regex.IsMatch(inputString, ValidationRegex.Address))
                    {
                        return new ValidationResult(false, "Enter a valid address.");
                    }
                    break;
                case "City":
                    if (!Regex.IsMatch(inputString, ValidationRegex.Name))
                    {
                        return new ValidationResult(false, "Enter a valid city name.");
                    }
                    break;
            }

            return new ValidationResult(true, null);
        }
    }
}