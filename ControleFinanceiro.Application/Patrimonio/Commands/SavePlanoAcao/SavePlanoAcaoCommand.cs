using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.SavePlanoAcao;

public record EtapaPlanoInput(string Titulo, string? Descricao, string? Prazo, string? Alvo, int Status);

/// <summary>
/// Cria um novo plano (Id nulo) ou atualiza um existente (por Id) do usuário efetivo.
/// Um cliente pode ter vários planos. As etapas são reescritas por completo, na ordem recebida.
/// </summary>
public record SavePlanoAcaoCommand(Guid? Id, string Objetivo, string? Prazo, IEnumerable<EtapaPlanoInput> Etapas)
    : IRequest<Guid>;

public class SavePlanoAcaoCommandHandler(
    IPlanoAcaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SavePlanoAcaoCommand, Guid>
{
    public async Task<Guid> Handle(SavePlanoAcaoCommand request, CancellationToken cancellationToken)
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

        if (request.Id is { } id)
        {
            var existente = await repository.GetByIdAsync(id, cancellationToken);
            if (existente is null || existente.UsuarioId != currentUser.UserId)
                throw new KeyNotFoundException("Plano não encontrado.");
            existente.Atualizar(objetivo, prazo, etapas);
            repository.Update(existente);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existente.Id;
        }

        var novo = new PlanoAcao(currentUser.UserId, objetivo, prazo, etapas);
        await repository.AddAsync(novo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return novo.Id;
    }
}
