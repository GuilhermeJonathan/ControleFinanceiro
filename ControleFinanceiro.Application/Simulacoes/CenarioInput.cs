using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Simulacoes;

/// <summary>Cenário informado pelo cliente (aporte/resgate extra) numa simulação.</summary>
public record CenarioInput(string Nome, TipoCenario Tipo, decimal Valor, int IdadeInicio, int? IdadeFim)
{
    public Cenario ToEntity() => new(Nome, Tipo, Valor, IdadeInicio, IdadeFim);
}
