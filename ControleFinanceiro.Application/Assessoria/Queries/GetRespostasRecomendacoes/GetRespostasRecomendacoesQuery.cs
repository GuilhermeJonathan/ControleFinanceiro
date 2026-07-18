using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetRespostasRecomendacoes;

public record RespostaRecomendacaoDto(
    Guid Id,
    string NomeCliente,
    int Tipo,
    string Texto,
    int Status,              // 2=Aceita 3=Recusada
    string? RespostaCliente,
    DateTime? RespondidoEm,
    bool Vista);

public record RespostasRecomendacoesDto(int NaoVistas, IEnumerable<RespostaRecomendacaoDto> Itens);

/// <summary>Respostas dos clientes às recomendações do assessor (para o sino de notificações).</summary>
public record GetRespostasRecomendacoesQuery : IRequest<RespostasRecomendacoesDto>;

public class GetRespostasRecomendacoesQueryHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser,
    IUserNameLookup userLookup)
    : IRequestHandler<GetRespostasRecomendacoesQuery, RespostasRecomendacoesDto>
{
    public async Task<RespostasRecomendacoesDto> Handle(GetRespostasRecomendacoesQuery request, CancellationToken cancellationToken)
    {
        var todas = await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken);
        var respondidas = todas.Where(r => r.Status != StatusRecomendacao.Pendente).ToList();

        var nomes = new Dictionary<Guid, string>();
        var itens = new List<RespostaRecomendacaoDto>();
        foreach (var r in respondidas.Take(50))
        {
            if (!nomes.TryGetValue(r.ClienteId, out var nome))
            {
                nome = await userLookup.GetNomeAsync(r.ClienteId, cancellationToken) ?? "Cliente";
                nomes[r.ClienteId] = nome;
            }
            itens.Add(new RespostaRecomendacaoDto(
                r.Id, nome, (int)r.Tipo, r.Texto, (int)r.Status,
                r.RespostaCliente, r.RespondidoEm, !r.RespostaNaoVista));
        }

        return new RespostasRecomendacoesDto(respondidas.Count(r => r.RespostaNaoVista), itens);
    }
}
