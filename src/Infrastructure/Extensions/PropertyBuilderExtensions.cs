using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TEnum> HasCommentFromEnum<TEnum>(
        this PropertyBuilder<TEnum> builder)
        where TEnum : struct, Enum
    {
        return builder.HasComment(EnumComment<TEnum>());
    }

    public static PropertyBuilder<TEnum?> HasCommentFromEnum<TEnum>(
        this PropertyBuilder<TEnum?> builder)
        where TEnum : struct, Enum
    {
        return builder.HasComment(EnumComment<TEnum>());
    }

    private static string EnumComment<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ",
            Enum.GetValues<TEnum>()
                .Select(e => $"{Convert.ToInt32(e)} = {e}"));
}