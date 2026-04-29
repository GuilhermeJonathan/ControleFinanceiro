namespace Login.Application.Users.Queries.GetUsers;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Document,
    int UserTypeId,
    bool IsActive,
    bool IsBlocked,
    string? AvatarUrl,
    Guid? ProfileId,
    Guid? HierarchyId,
    DateTime CreatedAt,
    DateTime? UltimoLogin
);
