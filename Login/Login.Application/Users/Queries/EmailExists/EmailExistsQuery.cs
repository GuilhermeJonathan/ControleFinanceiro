using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Queries.EmailExists;

/// <summary>Verifica (server-to-server) se já existe conta com o e-mail informado.</summary>
public record EmailExistsQuery(string Email) : IRequest<EmailExistsResult>;

public record EmailExistsResult(bool Exists, int? UserTypeId);

public class EmailExistsQueryHandler(IUserRepository userRepository)
    : IRequestHandler<EmailExistsQuery, EmailExistsResult>
{
    public async Task<EmailExistsResult> Handle(EmailExistsQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        return user is null
            ? new EmailExistsResult(false, null)
            : new EmailExistsResult(true, (int)user.UserTypeId);
    }
}
