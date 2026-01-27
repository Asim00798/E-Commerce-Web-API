using LibraryManagement.Domain.Entities.Security;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Entities.Security;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace E_Commerce.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        #region Repos
        IBaseRepository<Address, Guid> Addresses { get; }
        IBaseRepository<AuditLog, Guid> AuditLogs { get; }
        IBaseRepository<Author, int> Authors { get; }
        IBaseRepository<AuthorDetail, int> AuthorDetails { get; }
        IBaseRepository<BookAuthor, int> BookAuthors { get; }
        IBaseRepository<BookCopy, Guid> BookCopies { get; }
        IBaseRepository<BookTitle, int> BookTitles { get; }
        IBaseRepository<Borrowing, Guid> Borrowings { get; }
        IBaseRepository<Branch, Guid> Branches { get; }
        IBaseRepository<Category, int> Categories { get; }
        IBaseRepository<Contact, Guid> Contacts { get; }
        IBaseRepository<Country, Guid> Countries { get; }
        IBaseRepository<Event, Guid> Events { get; }
        IBaseRepository<Language, int> Languages { get; }
        IBaseRepository<Membership, Guid> Memberships { get; }
        IBaseRepository<Nationality, Guid> Nationalities { get; }
        IBaseRepository<Notification, Guid> Notifications { get; }
        IBaseRepository<PaymentLog, Guid> PaymentLogs { get; }
        IBaseRepository<Person, Guid> People { get; }
        IBaseRepository<Publisher, Guid> Publishers { get; }
        IBaseRepository<Registration, Guid> Registrations { get; }
        IBaseRepository<Reservation, Guid> Reservations { get; }
        IBaseRepository<Residency, Guid> Residencies { get; }
        IBaseRepository<Review, Guid> Reviews { get; }
        IBaseRepository<Shelf, Guid> Shelves { get; }
        IBaseRepository<Staff, Guid> Staffs { get; }
        IBaseRepository<StaffAttendance, Guid> StaffAttendances { get; }
        IBaseRepository<Subscription, Guid> Subscriptions { get; }
        IBaseRepository<User, Guid> Users { get; }
        IBaseRepository<Permission, Guid> Permissions { get; }
        IBaseRepository<Role, Guid> Roles { get; }
        IBaseRepository<RolePermission, Guid> RolePermissions { get; }
        IBaseRepository<IdentityUserRole<Guid>,Guid> UserRoles { get; } // built-in join table
        #endregion

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> CompleteAsync();
        public int Complete();
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
