using SharedLib;
using Xunit;
using System.Linq;

namespace SharedLib.Tests
{
    public class OtpGeneratorTests
    {
        [Fact]
        public void GenerateOtp_Returns6DigitString()
        {
            // Act
            var otp = OtpGenerator.GenerateOtp();

            // Assert
            Assert.NotNull(otp);
            Assert.Equal(6, otp.Length);
            Assert.True(otp.All(char.IsDigit));
        }
    }
}
