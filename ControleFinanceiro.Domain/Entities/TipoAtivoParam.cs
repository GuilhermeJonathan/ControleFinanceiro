namespace ControleFinanceiro.Domain.Entities;

/// <summary>Tipo de ativo patrimonial gerenciável pelo assessor.</summary>
public class TipoAtivoParam
{
    private TipoAtivoParam() { }

    public TipoAtivoParam(string nome, int ordem, string? icone = null)
    {
        Nome   = nome;
        Ordem  = ordem;
        Icone  = icone;
        Ativo  = true;
    }

    /// <summary>Usado pelo seed/migrations para inserir itens do sistema com Id fixo.</summary>
    public TipoAtivoParam(int id, string nome, int ordem, bool isSystem, string? icone = null)
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
    /// <summary>Emoji ou código de ícone exibido no app. Ex: "🏠".</summary>
    public string? Icone    { get; private set; }
    public int     Ordem    { get; private set; }
    public bool    Ativo    { get; private set; }
    /// <summary>Itens do sistema não podem ser excluídos.</summary>
    public bool    IsSystem { get; private set; }

    public void Atualizar(string nome, int ordem, bool ativo, string? icone = null)
    {
        Nome  = nome;
        Ordem = ordem;
        Ativo = ativo;
        Icone = icone;
    }
}
