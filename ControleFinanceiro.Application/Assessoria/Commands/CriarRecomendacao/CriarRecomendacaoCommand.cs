using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
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
    IConsultoriaConfigRepository consultoriaRepo,
    IConfiguration configuration,
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
        var nomeCliente  = contato.Nome ?? vinculo.NomeCliente ?? "Cliente";

        var consultoria = await consultoriaRepo.GetByUsuarioAsync(vinculo.AssessorId, cancellationToken);
        var marca = consultoria?.NomeConsultoria is { Length: > 0 } n ? n : (vinculo.NomeAssessor ?? "Seu assessor");
        var cor = consultoria?.CorMarca is { Length: > 0 } c ? c : "#16a34a";
        var link = $"{ConviteEmailBuilder.BaseUrl(configuration)}/home";
        var logo = ConviteEmailBuilder.LogoUrl(configuration, vinculo.AssessorId, !string.IsNullOrWhiteSpace(consultoria?.LogoBase64));

        var body = ConviteEmailBuilder.CorpoRecomendacao(
            marca, cor, logo, nomeCliente, tipoLabel, texto, link);

        await emailService.SendAsync(
            contato.Email, nomeCliente,
            $"{tipoLabel} — nova recomendação de {marca}", body, cancellationToken, marca);
    }
}
