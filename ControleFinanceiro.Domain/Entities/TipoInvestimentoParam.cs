namespace ControleFinanceiro.Domain.Entities;

/// <summary>Tipo de investimento financeiro gerenciável pelo assessor.</summary>
public class TipoInvestimentoParam
{
    private TipoInvestimentoParam() { }

    public TipoInvestimentoParam(string nome, int ordem, string? icone = null)
    {
        Nome  = nome;
        Ordem = ordem;
        Icone = icone;
        Ativo = true;
    }

    public TipoInvestimentoParam(int id, string nome, int ordem, bool isSystem, string? icone = null)
    {
        Id       = id;
        Nome     = nome;
        Ordem    = ordem;
        IsSystem = isSystem;
        Icone    = icone;
        Ativo    = true;
    }

    public int     Id       { get; private set; }
    public string  Nome     { get; private set; } = "";
    /// <summary>Emoji ou código de ícone exibido no app. Ex: "📈".</summary>
    public string? Icone    { get; private set; }
    public int     Ordem    { get; private set; }
    public bool    Ativo    { get; private set; }
    public bool    IsSystem { get; private set; }

    public void Atualizar(string nome, int ordem, bool ativo, string? icone = null)
    {
        Nome  = nome;
        Ordem = ordem;
        Ativo = ativo;
        Icone = icone;
    }
}
