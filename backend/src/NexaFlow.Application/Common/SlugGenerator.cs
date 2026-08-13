using System.Text;

namespace NexaFlow.Application.Common;

public static class SlugGenerator
{
    public static string Generate(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(trimmed.Length + 8);
        var previousWasDash = false;

        foreach (var c in trimmed)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                previousWasDash = false;
            }
            else if (!previousWasDash && builder.Length > 0)
            {
                builder.Append('-');
                previousWasDash = true;
            }
        }

        if (builder.Length > 0 && builder[^1] == '-')
        {
            builder.Length--;
        }

        var slug = builder.Length > 0 ? builder.ToString() : "org";
        return $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
    }
}
