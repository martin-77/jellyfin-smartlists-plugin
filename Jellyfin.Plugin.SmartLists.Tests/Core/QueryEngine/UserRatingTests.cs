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
    [Theory]
    [InlineData("GreaterThan", "8", 10, true)]
    [InlineData("GreaterThan", "8", 8, false)]
    [InlineData("GreaterThanOrEqual", "8", 8, true)]
    [InlineData("LessThan", "8", 7.5, true)]
    [InlineData("LessThanOrEqual", "8", 8, true)]
    [InlineData("Equal", "8.5", 8.5, true)]
    [InlineData("NotEqual", "8.5", 8, true)]
    public void CompileRule_Rating_SupportsNumericOperators(string op, string target, double rating, bool expected)
    {
        const string userId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        var normalizedUserId = Guid.Parse(userId).ToString("N");
        var operand = new Operand("Track");
        operand.RatingByUser[normalizedUserId] = rating;

        var rule = new Expression("Rating", op, target)
        {
            UserId = userId,
        };

        var compiled = Engine.CompileRule<Operand>(rule, string.Empty);
        Assert.Equal(expected, compiled(operand));
    }

    [Fact]
    public void CompileRule_Rating_InvalidNumericValueThrows()
    {
        var rule = new Expression("Rating", "GreaterThan", "not-a-number")
        {
            UserId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        };

        Assert.Throws<ArgumentException>(() => Engine.CompileRule<Operand>(rule, string.Empty));
    }

}
