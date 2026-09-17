using E_Commerce.Application.BoundedContexts.People.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Queries.GetMyPerson;

[AuthorizePermission(PeoplePermissions.ReadOwn)]
public sealed record GetMyPersonQuery : IRequest<Result<PersonDto>>;