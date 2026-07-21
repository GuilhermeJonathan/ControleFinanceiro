using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>Uma distribuição da família (histórico de distribuições do cliente).</summary>
public class Distribuicao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    /// <summary>Estrutura de origem (opcional) — de qual trust/holding veio.</summary>
    public Guid? EstruturaId { get; private set; }
    public DateTime Data { get; private set; }
    public decimal Valor { get; private set; }
    public MoedaPatrimonio Moeda { get; private set; }
    /// <summary>Beneficiário que recebeu (opcional).</summary>
    public Guid? BeneficiarioId { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Distribuicao() { }

    public Distribuicao(Guid usuarioId, DateTime data, decimal valor, MoedaPatrimonio moeda,
        Guid? estruturaId = null, Guid? beneficiarioId = null, string? descricao = null)
    {
        UsuarioId = usuarioId;
        EstruturaId = estruturaId;
        Data = data;
        Valor = valor;
        Moeda = moeda;
        BeneficiarioId = beneficiarioId;
        Descricao = descricao;
    }

    public void Atualizar(DateTime data, decimal valor, MoedaPatrimonio moeda,
        Guid? estruturaId, Guid? beneficiarioId, string? descricao)
    {
        Data = data;
        Valor = valor;
        Moeda = moeda;
        EstruturaId = estruturaId;
        BeneficiarioId = beneficiarioId;
        Descricao = descricao;
    }
}
