using MediatR;

namespace Login.Application.Users.Commands.Authenticate;

public record AuthenticateCommand(
    string Email,
    string Password,
    string? Captcha,
    string? TermName
) : IRequest<AuthenticateResult>;

public record AuthenticateResult(
    string AccessToken,
    string? AvatarUrl,    
    IReadOnlyList<HierarchyDto> Hierarchies,
    IReadOnlyList<RestrictionDto> Restrictions,
    IReadOnlyList<int> SelectedCompanies
);

public record HierarchyDto(IReadOnlyList<CompanyDto> Companies);
public record CompanyDto(int ClientId);
public record RestrictionDto(Guid ModuleId, int CompanyId);
