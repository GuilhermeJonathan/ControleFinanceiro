using Login.Application.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        // Filtros em memória — em produção, mover para query SQL com Dapper ou EF Core IQueryable
        var filtered = users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
            filtered = filtered.Where(u => u.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Email))
            filtered = filtered.Where(u => u.Email.Contains(request.Email, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Document))
            filtered = filtered.Where(u => u.Document == request.Document);

        if (request.UserTypeId.HasValue)
            filtered = filtered.Where(u => (int)u.UserTypeId == request.UserTypeId.Value);

        if (request.ProfileId.HasValue)
            filtered = filtered.Where(u => u.ProfileId == request.ProfileId.Value);

        var totalCount = filtered.Count();
        var items = filtered
            .Skip((request.CurrentPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(
                u.Id,
                u.Name,
                u.Email,
                u.Document,
                (int)u.UserTypeId,
                u.IsActive,
                u.IsBlocked,
                u.AvatarUrl,
                u.ProfileId,
                null,
                u.CreatedAt))
            .ToList();

        return new PagedResult<UserDto>(items, totalCount, request.CurrentPage, request.PageSize);
    }
}
