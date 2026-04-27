using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlockUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        if (request.Block)
            user.Block();
        else
            user.Unblock();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
