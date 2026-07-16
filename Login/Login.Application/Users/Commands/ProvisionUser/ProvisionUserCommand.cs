using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.ProvisionUser;

/// <summary>
/// Provisiona uma conta a partir de um convite validado por outra API (server-to-server,
/// protegido por service key no controller). Se já existe conta com o e-mail, autentica
/// com a senha informada; senão, cria a conta com o tipo indicado (cliente/corretor) e,
/// para cliente, inicia o trial. Retorna o token e o id do usuário.
/// </summary>
public record ProvisionUserCommand(
    string Name,
    string Email,
    string Password,
    string? Document,
    int UserTypeId
) : IRequest<ProvisionUserResult>;

public record ProvisionUserResult(string AccessToken, Guid UserId, bool Created);

public class ProvisionUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICryptography cryptography,
    ITokenManager tokenManager)
    : IRequestHandler<ProvisionUserCommand, ProvisionUserResult>
{
    public async Task<ProvisionUserResult> Handle(ProvisionUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existing is not null)
        {
            // Conta já existe: só vincula se a senha conferir (evita sequestro de conta via convite).
            if (existing.IsBlocked || !existing.IsActive)
                throw new UnauthorizedAccessException("Conta inativa ou bloqueada.");
            if (!await cryptography.VerifyAsync(request.Password, existing.PasswordHash))
                throw new UnauthorizedAccessException("Já existe uma conta com este e-mail. A senha informada não confere.");

            var tokenExistente = tokenManager.Generate(existing);
            return new ProvisionUserResult(tokenExistente, existing.Id, Created: false);
        }

        var tipo = (UserType)request.UserTypeId;
        var user = new User(
            id: Guid.NewGuid(),
            name: request.Name,
            email: request.Email,
            document: request.Document ?? string.Empty,
            passwordHash: cryptography.Hash(request.Password),
            userTypeId: tipo);

        // Cliente ganha trial de 30 dias; corretor é profissional (sem trial).
        if (tipo == UserType.User)
            user.StartTrial();

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = tokenManager.Generate(user);
        return new ProvisionUserResult(token, user.Id, Created: true);
    }
}
