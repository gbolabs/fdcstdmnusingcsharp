using Microsoft.Extensions.Logging;

namespace lib.Utils;

public static class Accelerators
{
    public static string? Or(this string? source, string? alternate)
    {
        return !string.IsNullOrEmpty(source) ? source : alternate;
    }

    public static ICollection<T> AddAll<T>(this ICollection<T> collection, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            collection.Add(value);
        }

        return collection;
    }

    public static string? LogAndThrowWhenNull(this string? source, string message, ILogger? logger)
    {
        if (string.IsNullOrEmpty(source))
        {
            logger.LogCritical(message);
            throw new ArgumentNullException(message);
        }

        return source;
    }

    public static string IsNullThrow(this string source, string message)
    {
        if (string.IsNullOrEmpty(source))
            throw new ArgumentNullException(message);
        return source;
    }
}