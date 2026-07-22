using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;

/// <summary>
/// Monta e gera o PDF do relatório de SUCESSÃO do usuário efetivo (cliente sob view-as):
/// estrutura patrimonial, beneficiários (planejado × distribuído), contas e planos de ação.
/// </summary>
public record GerarRelatorioSucessaoQuery(string? ClienteNome, RelatorioBranding Branding)
    : IRequest<byte[]>;

public class GerarRelatorioSucessaoQueryHandler(
    IMediator mediator,
    ICurrentUser currentUser,
    IConsultoriaConfigRepository consultoriaRepository,
    IRelatorioSucessaoGenerator generator)
    : IRequestHandler<GerarRelatorioSucessaoQuery, byte[]>
{
    public async Task<byte[]> Handle(GerarRelatorioSucessaoQuery request, CancellationToken ct)
    {
        var grafo    = await mediator.Send(new GetEstruturasQuery(), ct);
        var sucessao = await mediator.Send(new GetSucessaoQuery(), ct);
        var contas   = await mediator.Send(new GetContasQuery(), ct);
        var planos   = (await mediator.Send(new GetPlanosAcaoQuery(), ct)).ToList();

        var dados = new RelatorioSucessaoDados(
            ClienteNome: string.IsNullOrWhiteSpace(request.ClienteNome) ? "Cliente" : request.ClienteNome!,
            AssessorNome: currentUser.RealUserName ?? "Assessor",
            GeradoEm: DateTime.UtcNow,
            Grafo: grafo,
            Sucessao: sucessao,
            Contas: contas,
            Planos: planos);

        var config = await consultoriaRepository.GetByUsuarioAsync(currentUser.RealUserId, ct);
        var branding = config is not null
            ? new RelatorioBranding(config.NomeConsultoria, config.LogoBase64, config.CorMarca, config.MensagemRodape)
            : request.Branding;

        return generator.Gerar(dados, branding);
    }
}
