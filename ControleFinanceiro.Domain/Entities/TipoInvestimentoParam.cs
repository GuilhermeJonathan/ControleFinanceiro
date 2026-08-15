namespace ControleFinanceiro.Domain.Entities;

/// <summary>Tipo de investimento financeiro gerenciável pelo assessor.</summary>
public class TipoInvestimentoParam
{
    private TipoInvestimentoParam() { }

    public TipoInvestimentoParam(string nome, int ordem, string? icone = null, Guid? assessorId = null, bool exterior = false)
    {
        Nome       = nome;
        Ordem      = ordem;
        Icone      = icone;
        Ativo      = true;
        AssessorId = assessorId;
        Exterior   = exterior;
    }

    public TipoInvestimentoParam(int id, string nome, int ordem, bool isSystem, string? icone = null, bool exterior = false)
    {
        Id       = id;
        Nome     = nome;
        Ordem    = ordem;
        IsSystem = isSystem;
        Icone    = icone;
        Ativo    = true;
        Exterior = exterior;
    }

    public int     Id       { get; private set; }
    public string  Nome     { get; private set; } = "";
    /// <summary>Emoji ou código de ícone exibido no app. Ex: "📈".</summary>
    public string? Icone    { get; private set; }
    public int     Ordem    { get; private set; }
    public bool    Ativo    { get; private set; }
    public bool    IsSystem { get; private set; }
    /// <summary>null = tipo global (catálogo do admin). Preenchido = tipo custom de uma assessoria.</summary>
    public Guid?   AssessorId { get; private set; }
    /// <summary>true = classe de investimento negociada no exterior (usa cotação global/Yahoo); false = nacional (B3/brapi).</summary>
    public bool    Exterior { get; private set; }

    public void Atualizar(string nome, int ordem, bool ativo, string? icone = null, bool exterior = false)
    {
        Nome     = nome;
        Ordem    = ordem;
        Ativo    = ativo;
        Icone    = icone;
        Exterior = exterior;
    }
}
