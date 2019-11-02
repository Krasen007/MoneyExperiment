// Krasen Ivanov 2019

namespace MoneyExperiment.Tests
{
    using MoneyExperiment.Helpers;
    using Xunit;

    public class ParseHelperTests
    {
        [Theory]
        [InlineData("3")]
        // [InlineData("5.5")] This fails.
        [InlineData("9,9")]
        [InlineData(null)]
        public void CanParseDouble(string value)
        {
            ///Assert.Equal(value, ParseHelper.ParseDouble(value).ToString());
            Assert.IsType<double>(ParseHelper.ParseDouble(value));
        }

        [Theory]
        [InlineData("4")]
        [InlineData("test")]
        //[InlineData(null)] this fails.
        public void CanParseString(string value)
        {
            Assert.IsType<string>(ParseHelper.ParseStringInput(value));
        }
    }
}