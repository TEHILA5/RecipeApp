using System.Text.RegularExpressions;

namespace RecipeApp.Service.Validators.Helpers
{
    /// <summary>
    /// Helper class for validating Israeli phone numbers
    /// </summary>
    public static class IsraeliPhoneValidator
    {
        // Mobile prefixes in Israel (050-059)
        private static readonly string[] MobilePrefixes = new[]
        {
            "050", "051", "052", "053", "054",
            "055", "056", "057", "058", "059"
        };

        // Landline area codes in Israel
        private static readonly string[] LandlinePrefixes = new[]
        {
            "02", // Jerusalem
            "03", // Center (Tel Aviv, Ramat Gan, etc.)
            "04", // North (Haifa, Acre, etc.)
            "08", // South (Beer Sheva, Ashkelon, etc.)
            "09"  // Sharon (Netanya, Herzliya, etc.)
        };

        /// <summary>
        /// Validates Israeli phone number (mobile or landline)
        /// Accepts formats: 0501234567, 050-1234567, 050-123-4567, 02-1234567
        /// </summary>
        public static bool IsValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Clean the phone number
            phone = CleanPhoneNumber(phone);

            // Validate mobile or landline
            return IsValidMobile(phone) || IsValidLandline(phone);
        }

        /// <summary>
        /// Validates Israeli mobile number (10 digits, starts with 05X)
        /// </summary>
        public static bool IsValidMobile(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = CleanPhoneNumber(phone);

            // Must be 10 digits
            if (phone.Length != 10)
                return false;

            // Must start with valid mobile prefix
            return MobilePrefixes.Any(prefix => phone.StartsWith(prefix));
        }

        /// <summary>
        /// Validates Israeli landline number (9 digits, starts with area code)
        /// </summary>
        public static bool IsValidLandline(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = CleanPhoneNumber(phone);

            // Must be 9 digits
            if (phone.Length != 9)
                return false;

            // Must start with valid landline prefix
            return LandlinePrefixes.Any(prefix => phone.StartsWith(prefix));
        }

        /// <summary>
        /// Removes all non-digit characters from phone number
        /// </summary>
        public static string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return Regex.Replace(phone, @"[^\d]", "");
        }

        /// <summary>
        /// Formats phone number to standard Israeli format
        /// Mobile: 050-1234567
        /// Landline: 02-1234567
        /// </summary>
        public static string Format(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            phone = CleanPhoneNumber(phone);

            if (IsValidMobile(phone))
            {
                // Format: 050-1234567
                return $"{phone.Substring(0, 3)}-{phone.Substring(3)}";
            }

            if (IsValidLandline(phone))
            {
                // Format: 02-1234567
                return $"{phone.Substring(0, 2)}-{phone.Substring(2)}";
            }

            return phone; // Return as-is if invalid
        }

        /// <summary>
        /// Gets the phone type (Mobile, Landline, or Invalid)
        /// </summary>
        public static PhoneType GetPhoneType(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return PhoneType.Invalid;

            phone = CleanPhoneNumber(phone);

            if (IsValidMobile(phone))
                return PhoneType.Mobile;

            if (IsValidLandline(phone))
                return PhoneType.Landline;

            return PhoneType.Invalid;
        }

        /// <summary>
        /// Validates and returns formatted phone number
        /// Returns null if invalid
        /// </summary>
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
        Mobile,
        Landline
    }

    // ============================================
    // דוגמאות שימוש:
    // ============================================

    /*
    
    // בדיקה פשוטה
    bool isValid = IsraeliPhoneValidator.IsValid("0501234567"); // true
    bool isValid = IsraeliPhoneValidator.IsValid("12345"); // false
    
    // בדיקת סוג
    var type = IsraeliPhoneValidator.GetPhoneType("0501234567"); // Mobile
    var type = IsraeliPhoneValidator.GetPhoneType("02-1234567"); // Landline
    
    // ניקוי
    string clean = IsraeliPhoneValidator.CleanPhoneNumber("050-123-4567"); // "0501234567"
    
    // פורמט
    string formatted = IsraeliPhoneValidator.Format("0501234567"); // "050-1234567"
    
    // בדיקה + פורמט
    string? result = IsraeliPhoneValidator.ValidateAndFormat("050 123 4567"); // "050-1234567"
    string? result = IsraeliPhoneValidator.ValidateAndFormat("invalid"); // null
    
    */
}