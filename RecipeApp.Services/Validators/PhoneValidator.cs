using System.Text.RegularExpressions;

namespace RecipeApp.Service.Validators.Helpers
{
    public static class PhoneValidator
    { 
        private static readonly string[] IsraeliMobilePrefixes =
        {
            "050", "051", "052", "053", "054",
            "055", "056", "057", "058", "059"
        };
         
        private static readonly string[] IsraeliLandlinePrefixes =
        {
            "02", "03", "04", "08", "09"
        };

        public static bool IsValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return IsValidIsraeli(phone) || IsValidAmerican(phone);
        } 

        public static bool IsValidIsraeli(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = CleanPhoneNumber(phone);
            return IsValidIsraeliMobile(phone) || IsValidIsraeliLandline(phone);
        }

        private static bool IsValidIsraeliMobile(string phone)
        {
            return phone.Length == 10 &&
                   IsraeliMobilePrefixes.Any(p => phone.StartsWith(p)) &&
                   phone.All(char.IsDigit);
        }

        private static bool IsValidIsraeliLandline(string phone)
        {
            return phone.Length == 9 &&
                   IsraeliLandlinePrefixes.Any(p => phone.StartsWith(p)) &&
                   phone.All(char.IsDigit);
        } 

        public static bool IsValidAmerican(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            string cleaned = CleanPhoneNumber(phone);

            if (cleaned.Length == 11 && cleaned.StartsWith("1"))
                cleaned = cleaned.Substring(1);

            if (cleaned.Length != 10)
                return false;
             
            if (cleaned[0] == '0' || cleaned[0] == '1')
                return false;
             
            if (cleaned[3] == '0' || cleaned[3] == '1')
                return false;

            return cleaned.All(char.IsDigit);
        } 

        public static string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return Regex.Replace(phone, @"[^\d]", "");
        }

        public static PhoneType GetPhoneType(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return PhoneType.Invalid;

            string cleaned = CleanPhoneNumber(phone);

            if (IsValidIsraeliMobile(cleaned)) return PhoneType.IsraeliMobile;
            if (IsValidIsraeliLandline(cleaned)) return PhoneType.IsraeliLandline;
            if (IsValidAmerican(phone)) return PhoneType.American;

            return PhoneType.Invalid;
        }

        public static string Format(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var type = GetPhoneType(phone);
            string cleaned = CleanPhoneNumber(phone);

            return type switch
            {
                PhoneType.IsraeliMobile => $"{cleaned[..3]}-{cleaned[3..]}",
                PhoneType.IsraeliLandline => $"{cleaned[..2]}-{cleaned[2..]}",
                PhoneType.American => FormatAmerican(cleaned),
                _ => phone
            };
        }

        private static string FormatAmerican(string cleaned)
        { 
            if (cleaned.Length == 11 && cleaned.StartsWith("1"))
                cleaned = cleaned.Substring(1);
             
            return $"({cleaned[..3]}) {cleaned[3..6]}-{cleaned[6..]}";
        }

        public static string? ValidateAndFormat(string phone)
        {
            if (!IsValid(phone))
                return null;

            return Format(phone);
        }
    }

    public enum PhoneType
    {
        Invalid,
        IsraeliMobile,
        IsraeliLandline,
        American
    }
}