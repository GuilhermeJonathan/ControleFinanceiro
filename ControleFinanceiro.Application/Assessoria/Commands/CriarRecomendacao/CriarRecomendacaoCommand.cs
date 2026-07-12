using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Application.Assessoria.Commands.CriarRecomendacao;

public record CriarRecomendacaoCommand(
    Guid ClienteId,
    int Tipo,
    string Texto,
    Guid? CategoriaId) : IRequest<Guid>;

public class CriarRecomendacaoCommandHandler(
    IRecomendacaoRepository repository,
    IVinculoAssessoriaRepository vinculoRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IUserNameLookup userLookup,
    IEmailService emailService,
    ILogger<CriarRecomendacaoCommandHandler> logger)
    : IRequestHandler<CriarRecomendacaoCommand, Guid>
{
    public async Task<Guid> Handle(CriarRecomendacaoCommand request, CancellationToken cancellationToken)
    {
        var assessorId = currentUser.RealUserId;

        var vinculo = await vinculoRepository.GetVinculoAtivoAsync(assessorId, request.ClienteId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Vínculo de assessoria não encontrado ou revogado.");

        if (!Enum.IsDefined(typeof(TipoRecomendacao), request.Tipo))
            throw new ArgumentException($"Tipo de recomendação inválido: {request.Tipo}.");

        var recomendacao = new Recomendacao(
            assessorId, request.ClienteId, (TipoRecomendacao)request.Tipo, request.Texto, request.CategoriaId);

        await repository.AddAsync(recomendacao, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notifica o cliente por e-mail — falha aqui nunca desfaz a recomendação
        try
        {
            await NotificarClienteAsync(vinculo, (TipoRecomendacao)request.Tipo, request.Texto, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao notificar cliente {ClienteId} sobre nova recomendação.", request.ClienteId);
        }

        return recomendacao.Id;
    }

    private async Task NotificarClienteAsync(
        VinculoAssessoria vinculo, TipoRecomendacao tipo, string texto, CancellationToken cancellationToken)
    {
        var contato = await userLookup.GetContatoAsync(vinculo.ClienteId, cancellationToken);
        if (contato?.Email is null) return;

        var tipoLabel = tipo switch
        {
            TipoRecomendacao.AjusteCategoria => "📋 Ajuste de orçamento",
            TipoRecomendacao.Alerta          => "🚨 Alerta",
            _                                => "💡 Dica",
        };
        var nomeAssessor = vinculo.NomeAssessor ?? "Seu assessor";
        var nomeCliente  = contato.Nome ?? vinculo.NomeCliente ?? "Cliente";

        var body = $"""
            <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
              <div style="background:#0f1117;padding:0;border-bottom:2px solid #16a34a">
                <a href="https://app.findog.com.br" style="display:block;line-height:0">
                  <img src="https://app.findog.com.br/og-image.png" alt="Meu FinDog" width="560"
                       style="display:block;width:100%;max-width:560px;height:auto;border:0" />
                </a>
              </div>
              <div style="padding:32px 24px">
                <p style="font-size:18px;font-weight:700;color:#f1f5f9">Olá, {nomeCliente}!</p>
                <p style="color:#94a3b8;line-height:1.6">
                  <strong style="color:#e2e8f0">{nomeAssessor}</strong> enviou uma nova recomendação para você:
                </p>
                <div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">
                  <p style="color:#94a3b8;font-size:13px;margin:0 0 8px">{tipoLabel}</p>
                  <p style="color:#e2e8f0;font-size:14px;line-height:1.6;margin:0">{texto}</p>
                </div>
                <div style="text-align:center;margin:28px 0">
                  <a href="https://app.findog.com.br"
                     style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                    Responder no Meu FinDog
                  </a>
                </div>
                <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
                  Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
                </p>
              </div>
            </div>
            """;

        await emailService.SendAsync(
            contato.Email, nomeCliente,
            $"{tipoLabel} — nova recomendação do seu assessor", body, cancellationToken);
    }
}
