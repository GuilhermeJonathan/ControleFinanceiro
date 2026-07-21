namespace ControleFinanceiro.Domain.Entities;

/// <summary>Categoria de parâmetro global que uma assessoria pode ocultar do seu catálogo.</summary>
public enum TipoParametroCatalogo
{
    TipoAtivo = 1,
    TipoInvestimento = 2,
}

/// <summary>
/// Marca que uma assessoria optou por ocultar um parâmetro <b>global</b> (default) do seu
/// próprio catálogo. Não remove o global — apenas some para essa assessoria e seus clientes.
/// </summary>
public class ParametroOculto
{
    private ParametroOculto() { }

    public ParametroOculto(Guid assessorId, TipoParametroCatalogo tipo, int parametroId)
    {
        AssessorId  = assessorId;
        Tipo        = tipo;
        ParametroId = parametroId;
    }

    public int                   Id          { get; private set; }
    public Guid                  AssessorId  { get; private set; }
    public TipoParametroCatalogo Tipo        { get; private set; }
    /// <summary>Id do parâmetro global ocultado (TipoAtivoParam.Id ou TipoInvestimentoParam.Id).</summary>
    public int                   ParametroId { get; private set; }
}
