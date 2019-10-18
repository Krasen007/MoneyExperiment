using MoneyExperiment.Helpers;
using Xunit;

namespace MoneyExperiment.Tests
{
    public class EncryptionTests
    {
        private readonly string password = "test----------------------------";
        private string result = "";

        [Fact]
        public void TestSamePasswordEveryTime_ReturnSameKeyEncrypted()
        {
            this.result = Encryption.EncryptString(this.password, "to be encrypted");

            Assert.Equal(Encryption.EncryptString(this.password, "to be encrypted"), this.result);
        }

        [Fact]
        public void TestSamePasswordEveryTime_ReturnSameKeyDecrypted()
        {
            Assert.Equal(Encryption.DecryptString(this.password, this.result), Encryption.DecryptString(this.password, this.result));
        }
    }
}