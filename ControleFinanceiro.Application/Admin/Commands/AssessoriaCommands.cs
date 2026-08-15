using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Admin.Commands;

// ── Criar assessoria (admin da plataforma) ───────────────────────────────────
// Provisiona um usuário assessor (userType=3) na API de Login e semeia a marca
// da consultoria, para a assessoria aparecer no painel admin.

public record CriarAssessoriaCommand(string Nome, string Email, string Senha, string? NomeConsultoria) : IRequest<Guid>;

public class CriarAssessoriaCommandHandler(
    ILoginProvisionClient provision,
    IConsultoriaConfigRepository consultoriaRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<CriarAssessoriaCommand, Guid>
{
    private const int UserTypeAssessor = 3;

    public async Task<Guid> Handle(CriarAssessoriaCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin da plataforma pode criar assessorias.");
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome do assessor.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("Informe o e-mail.");
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            throw new InvalidOperationException("A senha inicial deve ter ao menos 6 caracteres.");

        // Cria (ou reaproveita) a conta do assessor na Login como userType=3.
        var result = await provision.ProvisionAsync(
            request.Nome.Trim(), request.Email.Trim(), request.Senha, null, UserTypeAssessor, ct);

        // Semeia a marca (nome) da consultoria para aparecer no painel.
        var nomeConsultoria = string.IsNullOrWhiteSpace(request.NomeConsultoria)
            ? request.Nome.Trim()
            : request.NomeConsultoria!.Trim();
        var existente = await consultoriaRepo.GetByUsuarioAsync(result.UserId, ct);
        if (existente is null)
            await consultoriaRepo.AddAsync(
                new ConsultoriaConfig(result.UserId, nomeConsultoria, null, null, null, null), ct);
        else
            existente.Atualizar(nomeConsultoria, existente.LogoBase64, existente.CorMarca,
                existente.WhatsApp, existente.MensagemRodape);

        await uow.SaveChangesAsync(ct);
        return result.UserId;
    }
}

// ── Editar assessoria (admin) — por ora, o nome da consultoria ────────────────
// Contas/planos vivem na API de Login (fora deste painel).

public record AtualizarAssessoriaCommand(
    Guid AssessorId, string NomeConsultoria,
    string? LogoBase64, string? CorMarca, string? WhatsApp, string? MensagemRodape, string? Slug = null) : IRequest;

public class AtualizarAssessoriaCommandHandler(
    IConsultoriaConfigRepository consultoriaRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<AtualizarAssessoriaCommand>
{
    public async Task Handle(AtualizarAssessoriaCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin da plataforma pode editar assessorias.");
        if (string.IsNullOrWhiteSpace(request.NomeConsultoria))
            throw new InvalidOperationException("Informe o nome da consultoria.");

        // Slug (rota do login) precisa ser único entre as assessorias.
        var slug = ConsultoriaConfig.NormalizarSlug(request.Slug);
        if (slug is not null)
        {
            var dono = await consultoriaRepo.GetBySlugAsync(slug, ct);
            if (dono is not null && dono.UsuarioId != request.AssessorId)
                throw new InvalidOperationException($"A rota \"{slug}\" já está em uso por outra assessoria.");
        }

        var nome = request.NomeConsultoria.Trim();
        var cfg = await consultoriaRepo.GetByUsuarioAsync(request.AssessorId, ct);
        if (cfg is null)
            await consultoriaRepo.AddAsync(
                new ConsultoriaConfig(request.AssessorId, nome, request.LogoBase64, request.CorMarca, request.WhatsApp, request.MensagemRodape, slug), ct);
        else
            cfg.Atualizar(nome, request.LogoBase64, request.CorMarca, request.WhatsApp, request.MensagemRodape, slug);

        await uow.SaveChangesAsync(ct);
    }
}
