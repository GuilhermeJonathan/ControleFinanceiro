using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenManager _tokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenManager tokenManager,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenManager = tokenManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!existing.IsValid())
            throw new UnauthorizedAccessException("Refresh token expirado ou revogado.");

        var user = await _userRepository.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        if (!user.IsActive || user.IsBlocked)
            throw new UnauthorizedAccessException("Usuário inativo ou bloqueado.");

        // Rotação: revoga o token antigo
        existing.Revoke();
        _refreshTokenRepository.Update(existing);

        // Gera novo par de tokens
        var newAccessToken  = _tokenManager.Generate(user);
        var newRefreshToken = new RefreshToken(user.Id, GenerateToken(), DateTime.UtcNow.AddDays(30));
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshResult(newAccessToken, newRefreshToken.Token);
    }

    private static string GenerateToken()
        => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
}
