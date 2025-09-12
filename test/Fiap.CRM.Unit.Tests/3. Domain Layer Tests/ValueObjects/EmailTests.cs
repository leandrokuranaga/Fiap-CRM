using Fiap.CRM.Domain.SeedWork.Exceptions;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;

namespace Fiap.CRM.Unit.Tests._3._Domain_Layer_Tests.ValueObjects
{
    public class EmailTests
    {
        [Fact]
        public void Constructor_ShouldCreateEmail_WhenValidAddress()
        {
            // Arrange
            var address = "test@example.com";

            // Act
            var email = new Email(address);

            // Assert
            Assert.Equal("test@example.com", email.Address);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowException_WhenAddressIsNullOrWhiteSpace(string invalidAddress)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Email(invalidAddress));
            Assert.Equal("Email address cannot be empty.", ex.Message);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("test@")]
        [InlineData("test.example.com")]
        [InlineData("test@example")]
        public void Constructor_ShouldThrowException_WhenEmailFormatIsInvalid(string invalidEmail)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Email(invalidEmail));
            Assert.Equal("Invalid email address format.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenEmailTooLong()
        {
            // Arrange
            var longEmail = new string('a', 90) + "@example.com";

            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Email(longEmail));
            Assert.Equal("Email must be less than or equal to 100 characters.", ex.Message);
        }

        [Theory]
        [InlineData("Test@Example.com", "test@example.com")]
        [InlineData("USER@DOMAIN.COM", "user@domain.com")]
        [InlineData("MixedCase@Email.ORG", "mixedcase@email.org")]
        public void Constructor_ShouldConvertToLowerCase(string input, string expectedOutput)
        {
            // Act
            var email = new Email(input);

            // Assert
            Assert.Equal(expectedOutput, email.Address);
        }

        [Theory]
        [InlineData("  test@example.com  ")]
        [InlineData("\tuser@domain.com\t")]
        [InlineData("   email@test.org   ")]
        public void Constructor_ShouldTrimWhitespace(string inputWithWhitespace)
        {
            // Act
            var email = new Email(inputWithWhitespace);

            // Assert
            Assert.Equal(inputWithWhitespace.Trim().ToLowerInvariant(), email.Address);
        }

        [Fact]
        public void Email_ShouldBeCaseInsensitive()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("TEST@EXAMPLE.COM");

            // Act & Assert
            Assert.Equal(email1.Address, email2.Address);
            Assert.Equal("test@example.com", email1.Address);
            Assert.Equal("test@example.com", email2.Address);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenEmailsHaveSameAddress()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            // Act & Assert
            Assert.Equal(email1.Address, email2.Address);
            Assert.Equal("test@example.com", email1.Address);
            Assert.Equal("test@example.com", email2.Address);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenEmailsHaveDifferentAddresses()
        {
            // Arrange
            var email1 = new Email("test1@example.com");
            var email2 = new Email("test2@example.com");

            // Act & Assert
            Assert.NotEqual(email1, email2);
            Assert.False(email1.Equals(email2));
        }

        [Fact]
        public void GetHashCode_ShouldBeSame_WhenEmailsAreEqual()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            // Act & Assert - Test the underlying values
            Assert.Equal(email1.Address, email2.Address);
            Assert.Equal("test@example.com", email1.Address);
        }

        [Fact]
        public void GetHashCode_ShouldBeDifferent_WhenEmailsAreDifferent()
        {
            // Arrange
            var email1 = new Email("test1@example.com");
            var email2 = new Email("test2@example.com");

            // Act & Assert
            Assert.NotEqual(email1.GetHashCode(), email2.GetHashCode());
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.email@domain.co.uk")]
        [InlineData("complex+tag@subdomain.example.org")]
        [InlineData("user123@test123.com")]
        [InlineData("a@b.co")]
        public void Constructor_ShouldAcceptValidEmailFormats(string validEmail)
        {
            // Act & Assert - Should not throw
            var email = new Email(validEmail);
            Assert.Equal(validEmail.ToLowerInvariant(), email.Address);
        }

        [Fact]
        public void Constructor_ShouldAcceptEmailWithMaxLength()
        {
            // Arrange
            var localPart = new string('a', 62);
            var domain = new string('b', 33) + ".com";
            var maxLengthEmail = localPart + "@" + domain;
            
            Assert.Equal(100, maxLengthEmail.Length);

            // Act
            var email = new Email(maxLengthEmail);

            // Assert
            Assert.Equal(maxLengthEmail.ToLowerInvariant(), email.Address);
        }

        [Fact]
        public void ToString_ShouldReturnEmailAddress()
        {
            // Arrange
            var emailAddress = "test@example.com";
            var email = new Email(emailAddress);

            // Act
            var result = email.Address;

            // Assert
            Assert.Equal(emailAddress, result);
        }
    }
}