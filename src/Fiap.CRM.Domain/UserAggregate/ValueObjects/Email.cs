using System.Text.RegularExpressions;
using Abp.Domain.Values;
using Fiap.CRM.Domain.SeedWork.Exceptions;

namespace Fiap.CRM.Domain.UserAggregate.ValueObjects;

public class Email : ValueObject
{
    public string Address { get; }

    public Email(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessRulesException("Email address cannot be empty.");

        address = address.Trim().ToLowerInvariant();

        if (address.Length > 100)
            throw new BusinessRulesException("Email must be less than or equal to 100 characters.");

        if (!IsValidEmail(address))
            throw new BusinessRulesException("Invalid email address format.");

        Address = address;
    }

    private static bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Address;
    }
}