using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Recomendação enviada por um assessor a um cliente da carteira
/// (ex: ajuste de limite de categoria, dica, alerta). O cliente aceita ou recusa.
/// </summary>
public class Recomendacao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AssessorId { get; private set; }
    public Guid ClienteId { get; private set; }
    public TipoRecomendacao Tipo { get; private set; }
    public Guid? CategoriaId { get; private set; }      // opcional: referência a uma categoria
    public string Texto { get; private set; } = string.Empty;
    public StatusRecomendacao Status { get; private set; } = StatusRecomendacao.Pendente;
    public string? RespostaCliente { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? RespondidoEm { get; private set; }

    private Recomendacao() { }

    public Recomendacao(Guid assessorId, Guid clienteId, TipoRecomendacao tipo, string texto, Guid? categoriaId = null)
    {
        AssessorId = assessorId;
        ClienteId = clienteId;
        Tipo = tipo;
        Texto = texto;
        CategoriaId = categoriaId;
    }

    public void Responder(StatusRecomendacao resposta, string? comentario)
    {
        if (Status != StatusRecomendacao.Pendente)
            throw new InvalidOperationException("Recomendação já foi respondida.");
        if (resposta is not (StatusRecomendacao.Aceita or StatusRecomendacao.Recusada))
            throw new ArgumentException("Resposta deve ser Aceita ou Recusada.");

        Status = resposta;
        RespostaCliente = comentario;
        RespondidoEm = DateTime.UtcNow;
    }
}
