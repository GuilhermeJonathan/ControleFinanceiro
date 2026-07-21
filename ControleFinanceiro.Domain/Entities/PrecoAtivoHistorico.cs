namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Registro histórico do preço de um ativo (ticker) em relação ao momento da consulta.
/// Criado pelo job diário / atualização manual de preços (fonte: brapi.dev).
/// </summary>
public class PrecoAtivoHistorico
{
    private PrecoAtivoHistorico() { }

    public PrecoAtivoHistorico(string ticker, decimal preco, string fonte)
    {
        Ticker   = ticker;
        Preco    = preco;
        Fonte    = fonte;
        DataHora = DateTime.UtcNow;
    }

    public int      Id       { get; private set; }
    /// <summary>Ex.: "PETR4", "IVVB11".</summary>
    public string   Ticker   { get; private set; } = "";
    public decimal  Preco    { get; private set; }
    public string   Fonte    { get; private set; } = "";
    public DateTime DataHora { get; private set; }
}
