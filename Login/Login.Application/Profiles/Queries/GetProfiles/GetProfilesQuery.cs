using MediatR;

namespace Login.Application.Profiles.Queries.GetProfiles;

public record GetProfilesQuery(int? TypeId, string? Name) : IRequest<IReadOnlyList<ProfileDto>>;

public record ProfileDto(Guid Id, string Name, int UserTypeId, bool IsActive);
