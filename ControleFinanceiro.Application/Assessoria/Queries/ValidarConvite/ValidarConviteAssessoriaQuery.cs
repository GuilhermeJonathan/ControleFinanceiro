using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.ValidarConvite;

public record ConviteInfoDto(bool Valido, string? NomeAssessor, string? EmailConvidado, bool JaAceito);

/// <summary>Valida (anônimo) um código de convite de assessoria para a tela pública /aceitar.</summary>
public record ValidarConviteAssessoriaQuery(string Codigo) : IRequest<ConviteInfoDto>;

public class ValidarConviteAssessoriaQueryHandler(IVinculoAssessoriaRepository repository)
    : IRequestHandler<ValidarConviteAssessoriaQuery, ConviteInfoDto>
{
    public async Task<ConviteInfoDto> Handle(ValidarConviteAssessoriaQuery request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (vinculo is null || vinculo.RevogadoEm != null)
            return new ConviteInfoDto(false, null, null, false);

        return new ConviteInfoDto(
            Valido: vinculo.AceitoEm == null,
            NomeAssessor: vinculo.NomeAssessor,
            EmailConvidado: vinculo.EmailConvidado,
            JaAceito: vinculo.AceitoEm != null);
    }
}
