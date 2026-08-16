using Jellyfin.Plugin.SmartLists.Core.QueryEngine;

namespace Jellyfin.Plugin.SmartLists.Tests.Core.QueryEngine;

public class UserRatingTests
{
    [Fact]
    public void GetRatingByUser_ReturnsStoredRating()
    {
        var operand = new Operand("Track");
        operand.RatingByUser["user-a"] = 10;

        Assert.Equal(10, operand.GetRatingByUser("user-a"));
        Assert.Equal(0, operand.GetRatingByUser("user-b"));
    }

    [Fact]
    public void FieldRegistry_Rating_IsNumericUserDataField()
    {
        var field = FieldRegistry.GetField("Rating");

        Assert.NotNull(field);
        Assert.Equal(FieldType.Numeric, field.Type);
        Assert.Equal(FieldCategory.RatingsPlayback, field.Category);
        Assert.Equal(ExtractionGroup.UserData, field.ExtractionGroup);
        Assert.True(field.IsUserSpecific);
    }

    [Fact]
    public void Expression_Rating_UsesUserSpecificGetter()
    {
        var expression = new Expression("Rating", "GreaterThanOrEqual", "8")
        {
            UserId = "user-a",
        };

        Assert.True(expression.IsUserSpecific);
        Assert.Equal("GetRatingByUser", expression.UserSpecificField);
        Assert.True(Expression.IsUserSpecificField("Rating"));
    }
}
