using Login.Domain.Entities;
using MediatR;

namespace Login.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    int UserTypeId,
    string Document,
    string Name,
    string Email,
    string? Address,
    string? Cellphone,
    string? Phone,
    string? Occupation,
    Guid? ProfileId,
    Guid? HierarchyId,
    Guid? FreightForwarderId,
    string? CompanyDocument,
    string? CompanyName,
    bool IsBlocked,
    int? CountryId,
    string? Region,
    IReadOnlyList<RestrictionItem>? Restrictions
) : IRequest<Guid>;

public record RestrictionItem(Guid ModuleId, int CompanyId);
