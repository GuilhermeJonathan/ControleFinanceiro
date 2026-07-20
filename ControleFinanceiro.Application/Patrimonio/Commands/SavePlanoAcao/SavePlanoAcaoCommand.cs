using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.SavePlanoAcao;

public record EtapaPlanoInput(string Titulo, string? Descricao, string? Prazo, string? Alvo, int Status);

/// <summary>
/// Cria ou substitui o Plano de Ação do usuário efetivo (um plano por cliente).
/// As etapas são reescritas por completo, na ordem recebida. Só o assessor (view-as) chama.
/// </summary>
public record SavePlanoAcaoCommand(string Objetivo, string? Prazo, IEnumerable<EtapaPlanoInput> Etapas)
    : IRequest<Unit>;

public class SavePlanoAcaoCommandHandler(
    IPlanoAcaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SavePlanoAcaoCommand, Unit>
{
    public async Task<Unit> Handle(SavePlanoAcaoCommand request, CancellationToken cancellationToken)
    {
        var objetivo = (request.Objetivo ?? string.Empty).Trim();
        if (objetivo.Length == 0)
            throw new InvalidOperationException("Informe o objetivo do plano.");

        var etapas = (request.Etapas ?? [])
            .Where(e => !string.IsNullOrWhiteSpace(e.Titulo))
            .Select((e, i) => new EtapaPlano(
                i,
                e.Titulo.Trim(),
                string.IsNullOrWhiteSpace(e.Descricao) ? null : e.Descricao.Trim(),
                string.IsNullOrWhiteSpace(e.Prazo) ? null : e.Prazo.Trim(),
                string.IsNullOrWhiteSpace(e.Alvo) ? null : e.Alvo.Trim(),
                Enum.IsDefined(typeof(StatusEtapa), e.Status) ? (StatusEtapa)e.Status : StatusEtapa.Pendente))
            .ToList();

        var prazo = string.IsNullOrWhiteSpace(request.Prazo) ? null : request.Prazo.Trim();

        var existente = await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken);
        if (existente is null)
        {
            var novo = new PlanoAcao(currentUser.UserId, objetivo, prazo, etapas);
            await repository.AddAsync(novo, cancellationToken);
        }
        else
        {
            existente.Atualizar(objetivo, prazo, etapas);
            repository.Update(existente);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
