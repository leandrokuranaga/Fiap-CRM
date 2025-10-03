using Fiap.CRM.Domain.SeedWork.Exceptions;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;

namespace Fiap.CRM.Unit.Tests._3._Domain_Layer_Tests.ValueObjects
{
    public class PasswordTests
    {
        [Fact]
        public void Constructor_ShouldCreatePassword_WhenValidPassword()
        {
            // Arrange
            var plainPassword = "Password123!";

            // Act
            var password = new Password(plainPassword);

            // Assert
            Assert.NotNull(password.Hash);
            Assert.NotNull(password.PasswordSalt);
            Assert.NotEmpty(password.Hash);
            Assert.NotEmpty(password.PasswordSalt);
        }

        [Theory]
        [InlineData("short")]
        [InlineData("1234567")]  // 7 characters
        public void Constructor_ShouldThrowException_WhenPasswordTooShort(string shortPassword)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(shortPassword));
            Assert.Equal("Password must be between 8 and 100 characters.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenPasswordTooLong()
        {
            // Arrange
            var longPassword = new string('a', 101); // 101 characters

            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(longPassword));
            Assert.Equal("Password must be between 8 and 100 characters.", ex.Message);
        }

        [Theory]
        [InlineData("12345678!")]  // No letter
        public void Constructor_ShouldThrowException_WhenPasswordMissingLetter(string passwordWithoutLetter)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(passwordWithoutLetter));
            Assert.Equal("Password must contain at least one letter.", ex.Message);
        }

        [Theory]
        [InlineData("PASSWORD!")]  // No digit - has uppercase letter
        public void Constructor_ShouldThrowException_WhenPasswordMissingDigit_ButHasLetter(string passwordWithoutDigit)
        {
            // Act & Assert - This password has uppercase letters and special char but missing digit
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(passwordWithoutDigit));
            Assert.Equal("Password must contain at least one number.", ex.Message);
        }

        [Theory]
        [InlineData("password!")]  // No digit
        [InlineData("PASSWORD!")]  // No digit
        public void Constructor_ShouldThrowException_WhenPasswordMissingDigit(string passwordWithoutDigit)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(passwordWithoutDigit));
            Assert.Equal("Password must contain at least one number.", ex.Message);
        }

        [Theory]
        [InlineData("Password123")]  // No special character
        [InlineData("password123")]  // No special character
        public void Constructor_ShouldThrowException_WhenPasswordMissingSpecialCharacter(string passwordWithoutSpecial)
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => new Password(passwordWithoutSpecial));
            Assert.Equal("Password must contain at least one special character.", ex.Message);
        }

        [Fact]
        public void Challenge_ShouldReturnTrue_WhenPasswordMatches()
        {
            // Arrange
            var plainPassword = "MyPassword123!";
            var password = new Password(plainPassword);

            // Act
            var result = password.Challenge(plainPassword, password.PasswordSalt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Challenge_ShouldReturnFalse_WhenPasswordDoesNotMatch()
        {
            // Arrange
            var plainPassword = "MyPassword123!";
            var wrongPassword = "WrongPassword456@";
            var password = new Password(plainPassword);

            // Act
            var result = password.Challenge(wrongPassword, password.PasswordSalt);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Challenge_ShouldReturnFalse_WhenSaltIsIncorrect()
        {
            // Arrange
            var plainPassword = "MyPassword123!";
            var password = new Password(plainPassword);
            var wrongSalt = "WrongSalt123";

            // Act
            var result = password.Challenge(plainPassword, wrongSalt);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("Password123!")]
        [InlineData("MySecure@Pass1")]
        [InlineData("Complex#Password9")]
        public void Constructor_ShouldAcceptValidPasswords(string validPassword)
        {
            // Act & Assert - Should not throw
            var password = new Password(validPassword);
            Assert.NotNull(password.Hash);
            Assert.NotNull(password.PasswordSalt);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueSalts()
        {
            // Arrange
            var plainPassword = "SamePassword123!";

            // Act
            var password1 = new Password(plainPassword);
            var password2 = new Password(plainPassword);

            // Assert
            Assert.NotEqual(password1.PasswordSalt, password2.PasswordSalt);
            Assert.NotEqual(password1.Hash, password2.Hash);
        }

        [Fact]
        public void Constructor_ShouldGenerateConsistentHashForSameSalt()
        {
            // Arrange
            var plainPassword = "TestPassword123!";
            var password1 = new Password(plainPassword);
            var existingSalt = password1.PasswordSalt;

            // Act
            var password2 = new Password(password1.Hash, existingSalt);

            // Assert
            Assert.Equal(password1.Hash, password2.Hash);
            Assert.Equal(password1.PasswordSalt, password2.PasswordSalt);
        }

        [Fact]
        public void ParameterlessConstructor_ShouldCreateEmptyPassword()
        {
            // Act
            var password = new Password();

            // Assert
            Assert.Equal(string.Empty, password.Hash);
            Assert.Equal(string.Empty, password.PasswordSalt);
        }

        [Fact]
        public void Constructor_WithHashAndSalt_ShouldSetProperties()
        {
            // Arrange
            var hash = "TestHash";
            var salt = "TestSalt";

            // Act
            var password = new Password(hash, salt);

            // Assert
            Assert.Equal(hash, password.Hash);
            Assert.Equal(salt, password.PasswordSalt);
        }

        [Theory]
        [InlineData("!@#$%^&*")]
        [InlineData("(){}[];")] 
        public void Constructor_ShouldAcceptSpecialCharacters(string specialChars)
        {
            // Arrange
            var validPassword = "Password123" + specialChars.Substring(0, 1);

            // Act & Assert - Should not throw
            var password = new Password(validPassword);
            Assert.NotNull(password.Hash);
        }

        [Fact]
        public void Password_ShouldBeValueObject()
        {
            // Arrange
            var plainPassword = "TestPassword123!";
            var password1 = new Password(plainPassword);
            var password2 = new Password(password1.Hash, password1.PasswordSalt);

            // Act & Assert
            Assert.Equal(password1.Hash, password2.Hash);
            Assert.Equal(password1.PasswordSalt, password2.PasswordSalt);
            Assert.NotNull(password1.Hash);
            Assert.NotNull(password1.PasswordSalt);
        }

        [Fact]
        public void Password_ShouldHaveDifferentEquality_WhenDifferent()
        {
            // Arrange
            var password1 = new Password("Password1!");
            var password2 = new Password("Password2@");

            // Act & Assert
            Assert.NotEqual(password1, password2);
        }

        [Fact]
        public void Challenge_ShouldBeCaseSensitive()
        {
            // Arrange
            var plainPassword = "MyPassword123!";
            var password = new Password(plainPassword);

            // Act
            var resultLower = password.Challenge("mypassword123!", password.PasswordSalt);
            var resultUpper = password.Challenge("MYPASSWORD123!", password.PasswordSalt);

            // Assert
            Assert.False(resultLower);
            Assert.False(resultUpper);
        }
    }
}