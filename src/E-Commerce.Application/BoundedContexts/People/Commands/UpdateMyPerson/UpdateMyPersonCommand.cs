using E_Commerce.Application.BoundedContexts.People.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.UpdateMyPerson;

[AuthorizePermission(PeoplePermissions.UpdateOwn)]
public sealed record UpdateMyPersonCommand(
    string PhoneNumber,
    string Email,
    AddressDto HomeAddress) : IRequest<Result>;