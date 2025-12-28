namespace CleanArchitecture.Extensions.Multitenancy.Providers;

internal static class TenantValueParser
{
    private static readonly char[] Separators = [',', ';'];

    public static IReadOnlyList<string> Split(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<string>();
        }

        var values = rawValue.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return values.Length == 0 ? Array.Empty<string>() : values;
    }
}
