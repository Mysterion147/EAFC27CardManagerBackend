using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAFC27.Infrastructure.Data;

public static class EnumPropertyExtensions
{
    // keeps the column as lowercase text ("buy", "sell", "manual") so existing rows stay valid
    public static PropertyBuilder<T> HasLowercaseStringConversion<T>(this PropertyBuilder<T> builder)
        where T : struct, Enum
        => builder.HasConversion(
            v => v.ToString().ToLowerInvariant(),
            v => Enum.Parse<T>(v, true));
}