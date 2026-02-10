using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerActivityProfileReset : DomainEvent
    {
        public Guid CustomerActivityProfileResetId { get; }

        public CustomerActivityProfileReset(Guid customerActivityProfileResetId)
        {
            CustomerActivityProfileResetId = customerActivityProfileResetId;
        }
    }
}