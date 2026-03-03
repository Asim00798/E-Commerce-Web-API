using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_Commerce.Domain.BoundedContexts.Administration.Enums
{
    /// <summary>
    /// Defines the types of entities that can own documents in the system.
    /// Used for polymorphic document ownership across bounded contexts.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OwnerType
    {
        /// <summary>
        /// Document owned by a Brand aggregate in Catalog context
        /// </summary>
        [Display(Name = "Brand")]
        Brand = 1,

        /// <summary>
        /// Document owned by a Product aggregate in Catalog context
        /// </summary>
        [Display(Name = "Product")]
        Product = 2,

        /// <summary>
        /// Document owned by a Category aggregate in Catalog context
        /// </summary>
        [Display(Name = "Category")]
        Category = 3,

        /// <summary>
        /// Document owned by a Customer aggregate in Identity context
        /// </summary>
        [Display(Name = "Customer")]
        Customer = 4,

        /// <summary>
        /// Document owned by an Employee aggregate in HR context
        /// </summary>
        [Display(Name = "Employee")]
        Employee = 5,

        /// <summary>
        /// Document owned by a Supplier aggregate in Procurement context
        /// </summary>
        [Display(Name = "Supplier")]
        Supplier = 6,

        /// <summary>
        /// Document owned by an Order aggregate in Ordering context
        /// </summary>
        [Display(Name = "Order")]
        Order = 7,

        /// <summary>
        /// Document owned by an Invoice aggregate in Billing context
        /// </summary>
        [Display(Name = "Invoice")]
        Invoice = 8,

        /// <summary>
        /// Document owned by a Contract aggregate in Legal context
        /// </summary>
        [Display(Name = "Contract")]
        Contract = 9,

        /// <summary>
        /// Document owned by a User aggregate in Identity context
        /// </summary>
        [Display(Name = "User")]
        User = 10,

        /// <summary>
        /// Document owned by a Company aggregate in Administration context
        /// </summary>
        [Display(Name = "Company")]
        Company = 11,

        /// <summary>
        /// Document owned by a Project aggregate in ProjectManagement context
        /// </summary>
        [Display(Name = "Project")]
        Project = 12,

        /// <summary>
        /// Document owned by a Ticket aggregate in Support context
        /// </summary>
        [Display(Name = "Support Ticket")]
        SupportTicket = 13,

        /// <summary>
        /// Document owned by a Shipment aggregate in Logistics context
        /// </summary>
        [Display(Name = "Shipment")]
        Shipment = 14,

        /// <summary>
        /// Document owned by a Return aggregate in Ordering context
        /// </summary>
        [Display(Name = "Return")]
        Return = 15,

        /// <summary>
        /// Document owned by a Payment aggregate in Billing context
        /// </summary>
        [Display(Name = "Payment")]
        Payment = 16,

        /// <summary>
        /// Document owned by a Review aggregate in Catalog context
        /// </summary>
        [Display(Name = "Review")]
        Review = 17,

        /// <summary>
        /// Document owned by a Claim aggregate in Insurance context
        /// </summary>
        [Display(Name = "Claim")]
        Claim = 18,

        /// <summary>
        /// Document owned by a Warranty aggregate in Product context
        /// </summary>
        [Display(Name = "Warranty")]
        Warranty = 19,

        /// <summary>
        /// Document owned by a License aggregate in Compliance context
        /// </summary>
        [Display(Name = "License")]
        License = 20,

        /// <summary>
        /// Fallback for future owner types not yet defined
        /// </summary>
        [Display(Name = "Other")]
        Other = 999
    }
}
