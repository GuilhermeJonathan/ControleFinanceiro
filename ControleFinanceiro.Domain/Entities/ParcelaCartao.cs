using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class ParcelaCartao : Entity
{
    public Guid CartaoCreditoId { get; private set; }
    public string Descricao { get; private set; }
    public decimal ValorParcela { get; private set; }
    public int ParcelaAtual { get; private set; }
    public int TotalParcelas { get; private set; }
    public DateTime DataInicio { get; private set; }

    public CartaoCredito? CartaoCredito { get; private set; }

    private ParcelaCartao() : base(Guid.NewGuid()) { Descricao = string.Empty; }

    public ParcelaCartao(Guid cartaoCreditoId, string descricao, decimal valorParcela,
        int parcelaAtual, int totalParcelas, DateTime dataInicio)
        : base(Guid.NewGuid())
    {
        CartaoCreditoId = cartaoCreditoId;
        Descricao = descricao;
        ValorParcela = valorParcela;
        ParcelaAtual = parcelaAtual;
        TotalParcelas = totalParcelas;
        DataInicio = dataInicio;
    }

    public void Update(string descricao, decimal valorParcela, int parcelaAtual, int totalParcelas)
    {
        Descricao = descricao;
        ValorParcela = valorParcela;
        ParcelaAtual = parcelaAtual;
        TotalParcelas = totalParcelas;
        SetUpdated();
    }
}
