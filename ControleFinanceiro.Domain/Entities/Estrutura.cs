using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Estrutura patrimonial/sucessória do cliente (trust, holding, offshore, PPLI…).
/// O VALOR da estrutura não é armazenado: é derivado dos ativos/investimentos com
/// EstruturaId apontando para ela + percentual das estruturas que ela detém.
/// </summary>
public class Estrutura
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoEstrutura Tipo { get; private set; }
    /// <summary>Jurisdição/localização. Ex: "Zurique · Suíça", "BVI", "Brasil · SP".</summary>
    public string? Jurisdicao { get; private set; }
    public DateTime? ConstituidaEm { get; private set; }
    public string? Observacoes { get; private set; }
    /// <summary>Posição manual no mapa (px). null = usa o layout automático.</summary>
    public double? PosX { get; private set; }
    public double? PosY { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private Estrutura() { }

    public Estrutura(Guid usuarioId, string nome, TipoEstrutura tipo,
        string? jurisdicao = null, DateTime? constituidaEm = null, string? observacoes = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Jurisdicao = jurisdicao;
        ConstituidaEm = constituidaEm;
        Observacoes = observacoes;
    }

    public void Atualizar(string nome, TipoEstrutura tipo, string? jurisdicao,
        DateTime? constituidaEm, string? observacoes)
    {
        Nome = nome;
        Tipo = tipo;
        Jurisdicao = jurisdicao;
        ConstituidaEm = constituidaEm;
        Observacoes = observacoes;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void DefinirPosicao(double posX, double posY)
    {
        PosX = posX;
        PosY = posY;
    }
}
