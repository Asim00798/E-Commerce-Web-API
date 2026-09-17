using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.DeleteMyPerson;

[AuthorizePermission(PeoplePermissions.DeleteOwn)]
public sealed record DeleteMyPersonCommand : IRequest<Result>;