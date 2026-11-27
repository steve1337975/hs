namespace NChecker
{
    internal static class Utils
    {
        internal static string CountryCodeToFlag(string countryCode) // ru
        {
            if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
                return string.Empty;
            countryCode = countryCode.ToUpperInvariant();
            int firstChar = char.ConvertToUtf32(countryCode, 0) - 0x41 + 0x1F1E6;
            int secondChar = char.ConvertToUtf32(countryCode, 1) - 0x41 + 0x1F1E6;
            return char.ConvertFromUtf32(firstChar) + char.ConvertFromUtf32(secondChar);
        }
    }
}
