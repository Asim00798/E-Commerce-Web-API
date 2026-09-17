using E_Commerce.Application.BoundedContexts.People.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.CreateMyPerson;

[AuthorizePermission(PeoplePermissions.CreateOwn)]
public sealed record CreateMyPersonCommand(
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string PhoneNumber,
    string Email,
    AddressDto HomeAddress) : IRequest<Result<Guid>>;