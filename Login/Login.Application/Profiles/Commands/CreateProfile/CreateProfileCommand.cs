using MediatR;

namespace Login.Application.Profiles.Commands.CreateProfile;

public record CreateProfileCommand(
    string Name,
    int UserTypeId,
    IReadOnlyList<PermissionItem>? Permissions
) : IRequest<Guid>;

public record PermissionItem(Guid ModuleId, Guid FunctionId);
