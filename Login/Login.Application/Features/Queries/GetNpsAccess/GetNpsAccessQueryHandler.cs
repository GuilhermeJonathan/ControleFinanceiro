using Login.Application.Common.Interfaces;
using MediatR;

namespace Login.Application.Features.Queries.GetNpsAccess;

public class GetNpsAccessQueryHandler : IRequestHandler<GetNpsAccessQuery, NpsAccessDto>
{
    // Lista estática de e-mails com acesso ao NPS (conforme regra de negócio SEC. 11)
    private static readonly HashSet<string> AllowedEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        // Popule via configuração em produção
    };

    private readonly IUserAccessor _userAccessor;

    public GetNpsAccessQueryHandler(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public Task<NpsAccessDto> Handle(GetNpsAccessQuery request, CancellationToken cancellationToken)
    {
        var email = _userAccessor.Email;
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("E-mail não encontrado no token.");

        return Task.FromResult(new NpsAccessDto(email, AllowedEmails.Contains(email)));
    }
}
