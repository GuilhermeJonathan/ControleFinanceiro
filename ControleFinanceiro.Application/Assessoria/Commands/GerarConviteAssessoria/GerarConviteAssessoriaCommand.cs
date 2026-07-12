using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;

public record GerarConviteAssessoriaCommand : IRequest<string>;

public class GerarConviteAssessoriaCommandHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GerarConviteAssessoriaCommand, string>
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>Carteira máxima sem o plano Assessor pago (F4). Com plano: ilimitada.</summary>
    public const int MaxClientesSemPlano = 10;

    public async Task<string> Handle(GerarConviteAssessoriaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem gerar convites de assessoria.");

        if (!currentUser.TemPlanoAssessor)
        {
            var vinculos = await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken);
            var emUso = vinculos.Count(v => v.RevogadoEm == null); // ativos + convites pendentes
            if (emUso >= MaxClientesSemPlano)
                throw new InvalidOperationException(
                    $"Limite de {MaxClientesSemPlano} clientes atingido. Assine o plano Assessor para carteira ilimitada.");
        }

        string codigo;
        do
        {
            codigo = new string(Enumerable.Range(0, 6)
                .Select(_ => Chars[Random.Shared.Next(Chars.Length)])
                .ToArray());
        } while (await repository.GetByCodigoAsync(codigo, cancellationToken) != null);

        var vinculo = VinculoAssessoria.Criar(currentUser.RealUserId, codigo, currentUser.RealUserName);
        await repository.AddAsync(vinculo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return codigo;
    }
}
