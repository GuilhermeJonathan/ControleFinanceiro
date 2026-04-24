using Login.Application.Users.Queries.GetUsers;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Document,
            (int)user.UserTypeId,
            user.IsActive,
            user.IsBlocked,
            user.AvatarUrl,
            user.ProfileId,
            user.HierarchyId,
            user.CreatedAt);
    }
}
