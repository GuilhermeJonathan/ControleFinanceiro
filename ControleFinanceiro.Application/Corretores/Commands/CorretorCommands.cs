using ControleFinanceiro.Application.Assessoria.Commands.AceitarConvitePublico;
using ControleFinanceiro.Application.Assessoria.Queries.ValidarConvite;
using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Application.Corretores.Commands;

// ── Gerar convite para corretor ──────────────────────────────────────────────

public record GerarConviteCorretorCommand(string? EmailConvidado = null) : IRequest<string>;

public class GerarConviteCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<GerarConviteCorretorCommand, string>
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<string> Handle(GerarConviteCorretorCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem convidar corretores.");

        string codigo;
        do
        {
            codigo = new string(Enumerable.Range(0, 6)
                .Select(_ => Chars[Random.Shared.Next(Chars.Length)])
                .ToArray());
        } while (await repo.GetByCodigoAsync(codigo, ct) != null);

        var vinculo = VinculoCorretor.Criar(currentUser.RealUserId, codigo, currentUser.RealUserName, request.EmailConvidado);
        await repo.AddAsync(vinculo, ct);
        await uow.SaveChangesAsync(ct);

        return codigo;
    }
}

// ── Enviar convite de corretor por e-mail ────────────────────────────────────

public record EnviarConviteCorretorEmailCommand(string Email) : IRequest<string>;

public class EnviarConviteCorretorEmailCommandHandler(
    ISender mediator,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConsultoriaConfigRepository consultoriaRepo,
    IConfiguration configuration) : IRequestHandler<EnviarConviteCorretorEmailCommand, string>
{
    public async Task<string> Handle(EnviarConviteCorretorEmailCommand request, CancellationToken ct)
    {
        var codigo = await mediator.Send(new GerarConviteCorretorCommand(request.Email), ct);

        var consultoria = await consultoriaRepo.GetByUsuarioAsync(currentUser.RealUserId, ct);
        var marca = consultoria?.NomeConsultoria is { Length: > 0 } n ? n : (currentUser.RealUserName ?? "Seu assessor");
        var cor = consultoria?.CorMarca is { Length: > 0 } c ? c : "#16a34a";

        var link = ConviteEmailBuilder.MontarLink(configuration, codigo, "corretor");
        var body = ConviteEmailBuilder.CorpoCorretor(marca, cor, codigo, link);

        await emailService.SendAsync(
            request.Email, request.Email,
            $"{marca} convidou você para atuar como corretor", body, ct);

        return codigo;
    }
}

// ── Validar convite de corretor (anônimo, tela /aceitar) ─────────────────────

public record ValidarConviteCorretorQuery(string Codigo) : IRequest<ConviteInfoDto>;

public class ValidarConviteCorretorQueryHandler(IVinculoCorretorRepository repo)
    : IRequestHandler<ValidarConviteCorretorQuery, ConviteInfoDto>
{
    public async Task<ConviteInfoDto> Handle(ValidarConviteCorretorQuery request, CancellationToken ct)
    {
        var vinculo = await repo.GetByCodigoAsync(request.Codigo, ct);
        if (vinculo is null || vinculo.RevogadoEm != null)
            return new ConviteInfoDto(false, null, null, false);

        return new ConviteInfoDto(
            Valido: vinculo.AceitoEm == null,
            NomeAssessor: vinculo.NomeAssessor,
            EmailConvidado: vinculo.EmailConvidado,
            JaAceito: vinculo.AceitoEm != null);
    }
}

// ── Aceite público do corretor via link do e-mail ────────────────────────────

public record AceitarConvitePublicoCorretorCommand(string Codigo, string Nome, string Senha)
    : IRequest<AceitarConvitePublicoResult>;

public class AceitarConvitePublicoCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    ILoginProvisionClient loginClient,
    IUnitOfWork uow) : IRequestHandler<AceitarConvitePublicoCorretorCommand, AceitarConvitePublicoResult>
{
    public async Task<AceitarConvitePublicoResult> Handle(AceitarConvitePublicoCorretorCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByCodigoAsync(request.Codigo, ct)
            ?? throw new KeyNotFoundException("Código de convite inválido.");
        if (vinculo.RevogadoEm != null) throw new InvalidOperationException("Este convite foi cancelado.");
        if (vinculo.AceitoEm != null)   throw new InvalidOperationException("Este convite já foi utilizado.");

        var email = vinculo.EmailConvidado
            ?? throw new InvalidOperationException("Convite sem e-mail associado. Peça um novo convite ao assessor.");

        // Cria a conta já como Corretor (elevação autorizada pelo convite válido).
        var conta = await loginClient.ProvisionAsync(
            request.Nome, email, request.Senha, document: null,
            userTypeId: (int)UserTypeConvite.Corretor, ct: ct);

        var existentes = await repo.GetByCorretorAsync(conta.UserId, ct);
        if (existentes.Any(v => v.AssessorId == vinculo.AssessorId && v.RevogadoEm == null && v.Id != vinculo.Id))
            throw new InvalidOperationException("Esta conta já é corretor deste assessor.");

        vinculo.Aceitar(conta.UserId, request.Nome);
        repo.Update(vinculo);
        await uow.SaveChangesAsync(ct);

        return new AceitarConvitePublicoResult(conta.AccessToken);
    }
}

// ── Corretor aceita convite ──────────────────────────────────────────────────

public record AceitarConviteCorretorCommand(string Codigo) : IRequest;

public class AceitarConviteCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<AceitarConviteCorretorCommand>
{
    public async Task Handle(AceitarConviteCorretorCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByCodigoAsync(request.Codigo, ct)
            ?? throw new KeyNotFoundException("Código de convite inválido ou já utilizado.");

        if (vinculo.AceitoEm != null)
            throw new InvalidOperationException("Este convite já foi utilizado.");

        // Verifica se já é corretor deste assessor
        var existentes = await repo.GetByCorretorAsync(currentUser.UserId, ct);
        if (existentes.Any(v => v.AssessorId == vinculo.AssessorId && v.RevogadoEm == null))
            throw new InvalidOperationException("Você já é corretor deste assessor.");

        vinculo.Aceitar(currentUser.UserId, currentUser.RealUserName ?? "Corretor");
        repo.Update(vinculo);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Assessor revoga corretor ─────────────────────────────────────────────────

public record RevogarCorretorCommand(Guid VinculoId) : IRequest;

public class RevogarCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RevogarCorretorCommand>
{
    public async Task Handle(RevogarCorretorCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByIdAsync(request.VinculoId, ct)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        if (vinculo.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado.");

        vinculo.Revogar();
        repo.Update(vinculo);

        // Revoga todas as delegações ativas deste corretor
        var delegacoes = await delegacaoRepo.GetByCorretorAsync(vinculo.CorretorId, ct);
        foreach (var d in delegacoes.Where(d => d.AssessorId == currentUser.RealUserId && d.Ativa))
        {
            d.Revogar();
            delegacaoRepo.Update(d);
        }

        await uow.SaveChangesAsync(ct);
    }
}
