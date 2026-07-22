namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Subtipo (2º nível) de um Tipo de Investimento, gerenciável pelo admin.
/// Ex.: dentro de "Renda Fixa" → "IPCA+", "Prefixado", "CDB". Semeado por tipo.
/// </summary>
public class SubtipoInvestimentoParam
{
    private SubtipoInvestimentoParam() { }

    public SubtipoInvestimentoParam(int tipoInvestimentoId, string nome, int ordem)
    {
        TipoInvestimentoId = tipoInvestimentoId;
        Nome  = nome;
        Ordem = ordem;
        Ativo = true;
    }

    public SubtipoInvestimentoParam(int id, int tipoInvestimentoId, string nome, int ordem, bool isSystem)
    {
        Id = id;
        TipoInvestimentoId = tipoInvestimentoId;
        Nome  = nome;
        Ordem = ordem;
        IsSystem = isSystem;
        Ativo = true;
    }

    public int    Id                 { get; private set; }
    /// <summary>Id do Tipo de Investimento (classe) ao qual o subtipo pertence.</summary>
    public int    TipoInvestimentoId { get; private set; }
    public string Nome               { get; private set; } = "";
    public int    Ordem              { get; private set; }
    public bool   Ativo              { get; private set; }
    public bool   IsSystem           { get; private set; }

    public void Atualizar(string nome, int ordem, bool ativo)
    {
        Nome  = nome;
        Ordem = ordem;
        Ativo = ativo;
    }
}
