using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.AceitarConvitePublico;

/// <summary>
/// Aceite público (anônimo) do convite de assessoria via link do e-mail: cria/vincula a
/// conta do cliente na Login (server-to-server) e marca o vínculo aceito. Retorna o token.
/// </summary>
public record AceitarConvitePublicoAssessoriaCommand(string Codigo, string Nome, string Senha)
    : IRequest<AceitarConvitePublicoResult>;

public record AceitarConvitePublicoResult(string AccessToken);

public class AceitarConvitePublicoAssessoriaCommandValidator : AbstractValidator<AceitarConvitePublicoAssessoriaCommand>
{
    public AceitarConvitePublicoAssessoriaCommandValidator()
    {
        RuleFor(c => c.Codigo).NotEmpty();
        RuleFor(c => c.Nome).NotEmpty().WithMessage("Informe seu nome.");
        RuleFor(c => c.Senha).NotEmpty().MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.");
    }
}

public class AceitarConvitePublicoAssessoriaCommandHandler(
    IVinculoAssessoriaRepository repository,
    ILoginProvisionClient loginClient,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AceitarConvitePublicoAssessoriaCommand, AceitarConvitePublicoResult>
{
    public async Task<AceitarConvitePublicoResult> Handle(AceitarConvitePublicoAssessoriaCommand request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByCodigoAsync(request.Codigo, cancellationToken)
            ?? throw new KeyNotFoundException("Código de convite inválido.");
        if (vinculo.RevogadoEm != null) throw new InvalidOperationException("Este convite foi cancelado.");
        if (vinculo.AceitoEm != null)   throw new InvalidOperationException("Este convite já foi utilizado.");

        var email = vinculo.EmailConvidado
            ?? throw new InvalidOperationException("Convite sem e-mail associado. Peça um novo convite ao seu assessor.");

        // Cria (ou autentica, se já existir) a conta do cliente na Login.
        var conta = await loginClient.ProvisionAsync(
            request.Nome, email, request.Senha, document: null,
            userTypeId: (int)UserTypeConvite.Cliente, ct: cancellationToken);

        // Um cliente só pode ter um assessor ativo.
        var existente = await repository.GetByClienteAsync(conta.UserId, cancellationToken);
        if (existente != null && existente.Id != vinculo.Id)
            throw new InvalidOperationException("Esta conta já possui um assessor ativo.");

        vinculo.Aceitar(conta.UserId, request.Nome);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AceitarConvitePublicoResult(conta.AccessToken);
    }
}
