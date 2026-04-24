using Login.Application.Common.Interfaces;
using MediatR;

namespace Login.Application.Integration.Commands.Authorize;

public class AuthorizeIntegrationCommandHandler : IRequestHandler<AuthorizeIntegrationCommand, string>
{
    private readonly ICryptography _cryptography;
    private readonly ITokenManager _tokenManager;

    public AuthorizeIntegrationCommandHandler(ICryptography cryptography, ITokenManager tokenManager)
    {
        _cryptography = cryptography;
        _tokenManager = tokenManager;
    }

    public Task<string> Handle(AuthorizeIntegrationCommand request, CancellationToken cancellationToken)
    {
        // Validação da integração machine-to-machine via ClientId/Secret
        // Em produção: buscar integração no banco e verificar hash
        throw new UnauthorizedAccessException("Credenciais de integração inválidas.");
    }
}
