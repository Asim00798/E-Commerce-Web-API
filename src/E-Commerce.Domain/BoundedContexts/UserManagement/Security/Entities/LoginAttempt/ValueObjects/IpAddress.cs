using System.Net;
using System.Net.Sockets;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects
{
    public sealed record IpAddress
    {
        public string Value { get; }

        public IpAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("IP address cannot be empty.");

            var normalized = value.Trim();
            if (!IPAddress.TryParse(normalized, out _))
                throw new BusinessRuleViolationException($"Invalid IP address format: '{normalized}'.");

            Value = normalized;
        }

        public AddressFamily AddressFamily =>
            IPAddress.Parse(Value).AddressFamily;

        public override string ToString() => Value;
    }
}
