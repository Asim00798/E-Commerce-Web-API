using E_Commerce.Application.Modules.Authorization.Commands.AssignPermissionToRole;
using E_Commerce.Application.Modules.Authorization.Commands.AssignRoleToUser;
using E_Commerce.Application.Modules.Authorization.Commands.CreatePermission;
using E_Commerce.Application.Modules.Authorization.Commands.CreateRole;
using E_Commerce.Application.Modules.Authorization.Commands.DeletePermission;
using E_Commerce.Application.Modules.Authorization.Commands.DeleteRole;
using E_Commerce.Application.Modules.Authorization.Commands.RemovePermissionFromRole;
using E_Commerce.Application.Modules.Authorization.Commands.RemoveRoleFromUser;
using E_Commerce.Application.Modules.Authorization.Commands.UpdatePermission;
using E_Commerce.Application.Modules.Authorization.Commands.UpdateRole;
using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Application.Modules.Authorization.Queries.GetPermissionById;
using E_Commerce.Application.Modules.Authorization.Queries.GetRoleById;
using E_Commerce.Application.Modules.Authorization.Queries.ListPermissions;
using E_Commerce.Application.Modules.Authorization.Queries.ListPermissionsForRole;
using E_Commerce.Application.Modules.Authorization.Queries.ListRoles;
using E_Commerce.Application.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Security.Authorization;

[ApiController]
[Route("api/authorization")]
public sealed class AuthorizationController : ControllerBase
{
    private readonly ISender _sender;

    public AuthorizationController(ISender sender)
    {
        _sender = sender;
    }

    // ============================================================
    // Permissions
    // ============================================================

    [HttpPost("permissions")]
    public async Task<ActionResult<Result<Guid>>> CreatePermission(
        [FromBody] CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("permissions/{permissionId:guid}")]
    public async Task<ActionResult<Result<PermissionDto>>> GetPermissionById(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var query = new GetPermissionByIdQuery(permissionId);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<Result<IReadOnlyList<PermissionDto>>>> GetPermissions(
        CancellationToken cancellationToken)
    {
        var query = new ListPermissionsQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPut("permissions/{permissionId:guid}")]
    public async Task<ActionResult<Result>> UpdatePermission(
        Guid permissionId,
        [FromBody] UpdatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePermissionCommand(
            permissionId,
            request.Name,
            request.Description);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("permissions/{permissionId:guid}")]
    public async Task<ActionResult<Result>> DeletePermission(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var command = new DeletePermissionCommand(permissionId);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    // ============================================================
    // Roles
    // ============================================================

    [HttpPost("roles")]
    public async Task<ActionResult<Result<Guid>>> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("roles/{roleId:guid}")]
    public async Task<ActionResult<Result<RoleDto>>> GetRoleById(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery(roleId);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<Result<IReadOnlyList<RoleDto>>>> GetRoles(
        CancellationToken cancellationToken)
    {
        var query = new ListRolesQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPut("roles/{roleId:guid}")]
    public async Task<ActionResult<Result>> UpdateRole(
        Guid roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand(
            roleId,
            request.Name);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("roles/{roleId:guid}")]
    public async Task<ActionResult<Result>> DeleteRole(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(roleId);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    // ============================================================
    // Role <-> Permission
    // ============================================================

    [HttpPost("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<ActionResult<Result>> AssignPermissionToRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var command = new AssignPermissionToRoleCommand(
            roleId,
            permissionId);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<ActionResult<Result>> RemovePermissionFromRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var command = new RemovePermissionFromRoleCommand(
            roleId,
            permissionId);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("roles/{roleId:guid}/permissions")]
    public async Task<ActionResult<Result<IReadOnlyList<PermissionDto>>>> GetRolePermissions(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var query = new ListPermissionsForRoleQuery(roleId);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    // ============================================================
    // User <-> Role
    // ============================================================

    [HttpPost("users/{userId:guid}/roles/{role}")]
    public async Task<ActionResult<Result>> AssignRoleToUser(
        Guid userId,
        string role,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleToUserCommand(
            userId,
            role);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("users/{userId:guid}/roles/{role}")]
    public async Task<ActionResult<Result>> RemoveRoleFromUser(
        Guid userId,
        string role,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleFromUserCommand(
            userId,
            role);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }
}

// Later add the following to E_Commerce.Api.Controllers.Authorization.Requests
public sealed record UpdatePermissionRequest(
    string Name,
    string? Description);

public sealed record UpdateRoleRequest(
    string Name);