namespace ControleFinanceiro.Domain.Entities;

/// <summary>Moeda suportada na consolidação patrimonial, gerenciável pelo assessor.</summary>
public class MoedaParam
{
    private MoedaParam() { }

    public MoedaParam(string codigo, string nome, int ordem, decimal cotacaoBRL = 1m)
    {
        Codigo     = codigo;
        Nome       = nome;
        Ordem      = ordem;
        CotacaoBRL = cotacaoBRL;
        Ativo      = true;
    }

    public MoedaParam(int id, string codigo, string nome, int ordem, bool isSystem, decimal cotacaoBRL = 1m)
    {
        Id         = id;
        Codigo     = codigo;
        Nome       = nome;
        Ordem      = ordem;
        IsSystem   = isSystem;
        CotacaoBRL = cotacaoBRL;
        Ativo      = true;
    }

    public int    Id       { get; private set; }
    public string Codigo   { get; private set; } = "";
    public string Nome     { get; private set; } = "";
    public decimal CotacaoBRL { get; private set; } = 1m;
    /// <summary>Data/hora UTC da última atualização automática de cotação.</summary>
    public DateTime? CotacaoAtualizadaEm { get; private set; }
    public int    Ordem    { get; private set; }
    public bool   Ativo    { get; private set; }
    public bool   IsSystem { get; private set; }

    public void Atualizar(string codigo, string nome, int ordem, bool ativo, decimal cotacaoBRL)
    {
        Codigo     = codigo;
        Nome       = nome;
        Ordem      = ordem;
        Ativo      = ativo;
        CotacaoBRL = cotacaoBRL;
    }

    /// <summary>Atualiza apenas a cotação BRL (usado pelo job automático de câmbio).</summary>
    public void AtualizarCotacao(decimal cotacaoBRL)
    {
        CotacaoBRL = cotacaoBRL;
        CotacaoAtualizadaEm = DateTime.UtcNow;
    }
}
