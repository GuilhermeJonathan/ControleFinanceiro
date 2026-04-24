using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.ResetPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IResetTokenManager _resetTokenManager;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IResetTokenManager resetTokenManager)
    {
        _userRepository = userRepository;
        _resetTokenManager = resetTokenManager;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Identificador, cancellationToken)
            ?? await _userRepository.GetByDocumentAsync(request.Identificador, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        await _resetTokenManager.GenerateAndSendAsync(user, cancellationToken);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptography _cryptography;
    private readonly IResetTokenManager _resetTokenManager;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICryptography cryptography,
        IResetTokenManager resetTokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cryptography = cryptography;
        _resetTokenManager = resetTokenManager;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByDocumentAsync(request.Document, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!await _resetTokenManager.ValidateAsync(user.Id, request.Token, cancellationToken))
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        user.ChangePassword(_cryptography.Hash(request.Password));
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class RedefinePasswordCommandHandler : IRequestHandler<RedefinePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptography _cryptography;
    private readonly IResetTokenManager _resetTokenManager;

    public RedefinePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICryptography cryptography,
        IResetTokenManager resetTokenManager)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cryptography = cryptography;
        _resetTokenManager = resetTokenManager;
    }

    public async Task Handle(RedefinePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByDocumentAsync(request.Document, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!await _resetTokenManager.ValidateAsync(user.Id, request.Token, cancellationToken))
            throw new UnauthorizedAccessException("Token inválido.");

        user.ChangePassword(_cryptography.Hash(request.Password));
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
