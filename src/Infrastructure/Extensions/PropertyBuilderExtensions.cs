using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TEnum> HasCommentFromEnum<TEnum>(
        this PropertyBuilder<TEnum> builder)
        where TEnum : struct, Enum
    {
        var comment = string.Join(", ",
            Enum.GetValues<TEnum>()
                .Select(e => $"{Convert.ToInt32(e)} = {e}"));

        return builder.HasComment(comment);
    }
}