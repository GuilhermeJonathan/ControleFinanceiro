using Login.Application.Common.Interfaces;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, MeDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAccessor _userAccessor;

    public GetMeQueryHandler(IUserRepository userRepository, IUserAccessor userAccessor)
    {
        _userRepository = userRepository;
        _userAccessor = userAccessor;
    }

    public async Task<MeDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userAccessor.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        return new MeDto(
            user.Id,
            user.Name,
            user.Email,
            user.Document,
            user.Cellphone,
            user.AvatarUrl);
    }
}
