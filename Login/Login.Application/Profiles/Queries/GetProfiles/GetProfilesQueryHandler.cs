using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Profiles.Queries.GetProfiles;

public class GetProfilesQueryHandler : IRequestHandler<GetProfilesQuery, IReadOnlyList<ProfileDto>>
{
    private readonly IProfileRepository _profileRepository;

    public GetProfilesQueryHandler(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyList<ProfileDto>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _profileRepository.GetAllAsync(cancellationToken);

        var filtered = profiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
            filtered = filtered.Where(p => p.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

        if (request.TypeId.HasValue)
            filtered = filtered.Where(p => (int)p.UserTypeId == request.TypeId.Value);

        return filtered
            .Select(p => new ProfileDto(p.Id, p.Name, (int)p.UserTypeId, p.IsActive))
            .ToList();
    }
}
