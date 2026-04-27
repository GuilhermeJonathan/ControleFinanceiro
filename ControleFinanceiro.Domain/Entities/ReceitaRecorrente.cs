using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class ReceitaRecorrente : Entity
{
    public string Nome { get; private set; }
    public TipoReceita Tipo { get; private set; }
    public decimal Valor { get; private set; }          // Calculado: fixo direto, horista = ValorHora * QuantidadeHoras
    public decimal? ValorHora { get; private set; }     // Apenas para Horista
    public decimal? QuantidadeHoras { get; private set; } // Apenas para Horista
    public int Dia { get; private set; }                // Dia do mês (1-28)
    public DateTime DataInicio { get; private set; }
    public Guid UsuarioId { get; private set; }

    private ReceitaRecorrente() : base(Guid.NewGuid()) { Nome = string.Empty; }

    public ReceitaRecorrente(string nome, TipoReceita tipo, int dia, DateTime dataInicio,
        decimal? valor = null, decimal? valorHora = null, decimal? quantidadeHoras = null,
        Guid usuarioId = default)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Dia = Math.Clamp(dia, 1, 28);
        DataInicio = dataInicio;
        AplicarValores(tipo, valor, valorHora, quantidadeHoras);
    }

    public void Update(string nome, TipoReceita tipo, int dia,
        decimal? valor = null, decimal? valorHora = null, decimal? quantidadeHoras = null)
    {
        Nome = nome;
        Tipo = tipo;
        Dia = Math.Clamp(dia, 1, 28);
        AplicarValores(tipo, valor, valorHora, quantidadeHoras);
        SetUpdated();
    }

    private void AplicarValores(TipoReceita tipo, decimal? valor, decimal? valorHora, decimal? quantidadeHoras)
    {
        if (tipo == TipoReceita.Horista)
        {
            ValorHora = valorHora ?? 0;
            QuantidadeHoras = quantidadeHoras ?? 0;
            Valor = ValorHora.Value * QuantidadeHoras.Value;
        }
        else
        {
            Valor = valor ?? 0;
            ValorHora = null;
            QuantidadeHoras = null;
        }
    }
}
