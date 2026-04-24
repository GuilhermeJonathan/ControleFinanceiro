using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class HorasTrabalhadas : Entity
{
    public string Descricao { get; private set; }
    public decimal ValorHora { get; private set; }
    public decimal Quantidade { get; private set; }
    public int Mes { get; private set; }
    public int Ano { get; private set; }

    public decimal ValorTotal => ValorHora * Quantidade;

    private HorasTrabalhadas() : base(Guid.NewGuid()) { Descricao = string.Empty; }

    public HorasTrabalhadas(string descricao, decimal valorHora, decimal quantidade, int mes, int ano)
        : base(Guid.NewGuid())
    {
        Descricao = descricao;
        ValorHora = valorHora;
        Quantidade = quantidade;
        Mes = mes;
        Ano = ano;
    }

    public void Update(string descricao, decimal valorHora, decimal quantidade)
    {
        Descricao = descricao;
        ValorHora = valorHora;
        Quantidade = quantidade;
        SetUpdated();
    }
}
