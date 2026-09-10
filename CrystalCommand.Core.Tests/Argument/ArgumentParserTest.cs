using CrystalCommand.Core.Argument;

namespace CrystalCommand.Core.Tests.Argument;

public class ArgumentParserTest
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public async Task Boolean_ParseSuccess(string input, bool expected)
    {
        await AssertSuccess(
            new BooleanArgumentParser<object>(),
            input,
            expected
        );
    }

    [Theory]
    [InlineData("ok")]
    [InlineData("no")]
    public void Boolean_ParseFailure(string input)
    {
        AssertFailure(new BooleanArgumentParser<object>(), input);
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("he ll o", "he")]
    [InlineData("\"he ll o\"", "he ll o", StringArgumentType.Quoted)]
    [InlineData("\"he l\"l o", "\"he", StringArgumentType.Quoted)]
    [InlineData("\"he l\"l o", "\"he l\"l o", StringArgumentType.Greedy)]
    public async Task String_ParseSuccess(
        string input,
        string expectedStr,
        StringArgumentType type = StringArgumentType.Normal
    )
    {
        await AssertSuccess(
            new StringArgumentParser<object> { Type = type },
            input,
            expectedStr
        );
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    [InlineData("0", 1, true)]
    [InlineData("11", 10, true)]
    public async Task Number_ParseWithMinMax(string input, int expected, bool clamp = false)
    {
        var parser = new IntArgumentParser<object>
        {
            Min = 1,
            Max = 10,
            Clamp = clamp
        };
        await AssertSuccess(parser, input, expected);
    }

    [Theory]
    [InlineData("0", NumberParseException.ErrorType.TooSmall)]
    [InlineData("11", NumberParseException.ErrorType.TooLarge)]
    public void Number_ParseTooSmallOrLarge(string input, NumberParseException.ErrorType type)
    {
        var parser = new IntArgumentParser<object>
        {
            Min = 1,
            Max = 10
        };
        var exception = Assert.IsType<NumberParseException>(AssertFailure(parser, input));
        Assert.Equal(type, exception.Type);
    }

    [Theory]
    [InlineData("233", 233)]
    [InlineData("-233", -233)]
    [InlineData("0", 0)]
    public async Task Int_ParseSuccess(string input, int expected)
    {
        await AssertSuccess(new IntArgumentParser<object>(), input, expected);
    }

    [Theory]
    [InlineData("qwe")]
    [InlineData("2.33")]
    [InlineData("")]
    [InlineData("0.0")]
    public void Int_ParseFailure(string input)
    {
        var exception = Assert.IsType<NumberParseException>(
            AssertFailure(new IntArgumentParser<object>(), input)
        );
        Assert.Equal(NumberParseException.ErrorType.NotANumber, exception.Type);
    }

    [Theory]
    [InlineData("233", 233)]
    [InlineData("-233", -233)]
    [InlineData("0", 0)]
    [InlineData("2.33", 2.33)]
    [InlineData("-2.33", -2.33)]
    [InlineData("0.0", 0)]
    [InlineData("2.333", 2.33)]
    [InlineData("-2.333", -2.33)]
    public async Task Double_ParseSuccess(string input, double expected)
    {
        await AssertSuccess(new DoubleArgumentParser<object>{ DecimalPlaces = 2 }, input, expected);
    }

    [Theory]
    [InlineData("qwe")]
    [InlineData("2.3.3")]
    [InlineData("")]
    public void Double_ParseFailure(string input)
    {
        var exception = Assert.IsType<NumberParseException>(
            AssertFailure(new DoubleArgumentParser<object>(), input)
        );
        Assert.Equal(NumberParseException.ErrorType.NotANumber, exception.Type);
    }

    private static async Task AssertSuccess<T>(
        IArgumentParser<object, T> parser,
        string input,
        T expected
    )
    {
        var context = new ParseContext<object>(new object(), input);
        var result = parser.Parse(context);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, await result.GetResultAsync());
    }

    private static Exception AssertFailure<T>(
        IArgumentParser<object, T> parser,
        string input
    )
    {
        var context = new ParseContext<object>(new object(), input);
        var result = parser.Parse(context);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        return result.Exception;
    }
}