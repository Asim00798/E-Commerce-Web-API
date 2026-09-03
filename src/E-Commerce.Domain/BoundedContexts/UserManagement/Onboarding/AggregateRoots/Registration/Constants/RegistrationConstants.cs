namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Constants
{
    public static class RegistrationConstants
    {
        public const int MaxAttemptsPerChannel = 5;
        public const int MaxResendsPerChannel = 3;

        public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan OTPLifetime = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    }
}