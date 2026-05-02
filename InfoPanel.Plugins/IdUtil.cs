using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoPanel.Plugins
{
    internal partial class IdUtil
    {
        [GeneratedRegex(@"[^a-z0-9-]", RegexOptions.IgnoreCase)]
        private static partial Regex AlphaNumericRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        public static string Encode(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark && (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                {
                    sb.Append(c);
                }
            }

            string cleaned = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            string dashed = WhitespaceRegex().Replace(cleaned, "-").Trim('-');
            string slug = AlphaNumericRegex().Replace(dashed, "");

            return slug;
        }
    }
}
