namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Registro histórico de uma cotação de moeda em relação ao BRL.
/// Criado automaticamente pelo job diário de atualização de câmbio.
/// </summary>
public class CotacaoHistorico
{
    private CotacaoHistorico() { }

    public CotacaoHistorico(string moedaCodigo, decimal cotacaoBRL, string fonte)
    {
        MoedaCodigo  = moedaCodigo;
        CotacaoBRL   = cotacaoBRL;
        Fonte        = fonte;
        DataHora     = DateTime.UtcNow;
    }

    public int     Id           { get; private set; }
    /// <summary>Ex.: "USD", "EUR".</summary>
    public string  MoedaCodigo  { get; private set; } = "";
    /// <summary>Valor de 1 unidade desta moeda em BRL na data/hora da consulta.</summary>
    public decimal CotacaoBRL   { get; private set; }
    /// <summary>Fonte da cotação: "AwesomeAPI", "Manual", etc.</summary>
    public string  Fonte        { get; private set; } = "";
    /// <summary>Data/hora UTC da cotação.</summary>
    public DateTime DataHora    { get; private set; }
}
