using System.Text.RegularExpressions;

namespace Tadbeer.BLL.Utilities;

public static class StringNormalization
{
    public static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.Trim();
        value = Regex.Replace(value, @"\s+", " ");
        return value.ToUpperInvariant();
    }
}