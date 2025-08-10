using System.Security.Cryptography;
using Abp.Domain.Values;
using Fiap.CRM.Domain.SeedWork.Exceptions;
using Fiap.CRM.Domain.Utils;

namespace Fiap.CRM.Domain.UserAggregate.ValueObjects;

 public class Password : ValueObject
    {
        public string Hash { get; } = string.Empty;
        public string PasswordSalt { get; private set; } = string.Empty;

        private const int KeySize = 32;
        private const int SaltSize = 16;
        private const int Iterations = 10000;

        protected Password()
        {
        }

        public Password(string hash, string salt)
        {
            Hash = hash;
            PasswordSalt = salt;
        }

        public Password(string text)
        {
            ValidatePasswordStrength(text);

            string password;

            if (Base64Utils.IsBase64String(text))
                password = Base64Utils.DecodeBase64String(text);
            else
                password = text;

            PasswordSalt = Convert.ToBase64String(GenerateSalt(16));
            Hash = Hashing(password, PasswordSalt);
        }

        public bool Challenge(string password, string saltSaved) =>
            Verify(Hash, password, saltSaved);

        private static string Hashing(
            string password,
            string passwordSalt,
            short saltSize = SaltSize,
            short keySize = KeySize,
            int iterations = Iterations,
            char splitChar = '.')
        {
            if (string.IsNullOrEmpty(password))
                throw new Exception("Password should not be null or empty");

            password += passwordSalt;

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                saltSize,
                iterations,
                HashAlgorithmName.SHA256);
            var key = Convert.ToBase64String(algorithm.GetBytes(keySize));
            var salt = Convert.ToBase64String(algorithm.Salt);

            return $"{iterations}{splitChar}{salt}{splitChar}{key}";
        }

        private static bool Verify(
            string hash,
            string password,
            string passwordSalt,
            short keySize = KeySize,
            int iterations = Iterations,
            char splitChar = '.')
        {
            password += passwordSalt;

            var parts = hash.Split(splitChar, 3);
            if (parts.Length != 3)
                return false;

            var hashIterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            if (hashIterations != iterations)
                return false;

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);
            var keyToCheck = algorithm.GetBytes(keySize);

            return keyToCheck.SequenceEqual(key);
        }

        private static byte[] GenerateSalt(int size = 16)
        {
            using RNGCryptoServiceProvider rng = new();
            var salt = new byte[size];
            rng.GetBytes(salt);
            return salt;
        }

        private void ValidatePasswordStrength(string password)
        {
            if (password.Length < 8 || password.Length > 100)
                throw new BusinessRulesException("Password must be between 8 and 100 characters.");

            if (!password.Any(char.IsLetter))
                throw new BusinessRulesException("Password must contain at least one letter.");

            if (!password.Any(char.IsDigit))
                throw new BusinessRulesException("Password must contain at least one number.");

            if (!password.Any(c => "!@#$%^&*(){}[];".Contains(c)))
                throw new BusinessRulesException("Password must contain at least one special character.");
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Hash;
            yield return PasswordSalt;
        }
    }