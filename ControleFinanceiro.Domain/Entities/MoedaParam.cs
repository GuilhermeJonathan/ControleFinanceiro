namespace ControleFinanceiro.Domain.Entities;

/// <summary>Moeda suportada na consolidação patrimonial, gerenciável pelo assessor.</summary>
public class MoedaParam
{
    private MoedaParam() { }

    public MoedaParam(string codigo, string nome, int ordem)
    {
        Codigo = codigo;
        Nome   = nome;
        Ordem  = ordem;
        Ativo  = true;
    }

    public MoedaParam(int id, string codigo, string nome, int ordem, bool isSystem)
    {
        Id       = id;
        Codigo   = codigo;
        Nome     = nome;
        Ordem    = ordem;
        IsSystem = isSystem;
        Ativo    = true;
    }

    public int    Id       { get; private set; }
    /// <summary>Ex.: "BRL", "USD", "EUR".</summary>
    public string Codigo   { get; private set; } = "";
    /// <summary>Nome de exibição: "Real Brasileiro".</summary>
    public string Nome     { get; private set; } = "";
    public int    Ordem    { get; private set; }
    public bool   Ativo    { get; private set; }
    public bool   IsSystem { get; private set; }

    public void Atualizar(string codigo, string nome, int ordem, bool ativo)
    {
        Codigo = codigo;
        Nome   = nome;
        Ordem  = ordem;
        Ativo  = ativo;
    }
}
