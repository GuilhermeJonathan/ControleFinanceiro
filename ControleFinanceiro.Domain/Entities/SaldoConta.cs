using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class SaldoConta : Entity
{
    public string Banco { get; private set; }
    public decimal Saldo { get; private set; }
    public TipoConta Tipo { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    private SaldoConta() : base(Guid.NewGuid()) { Banco = string.Empty; }

    public SaldoConta(string banco, decimal saldo, TipoConta tipo) : base(Guid.NewGuid())
    {
        Banco = banco;
        Saldo = saldo;
        Tipo = tipo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Atualizar(string banco, decimal saldo, TipoConta tipo)
    {
        Banco = banco;
        Saldo = saldo;
        Tipo = tipo;
        DataAtualizacao = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>Movimenta o saldo. Valor positivo = crédito, negativo = débito.</summary>
    public void Movimentar(decimal valor)
    {
        Saldo += valor;
        DataAtualizacao = DateTime.UtcNow;
        SetUpdated();
    }

    // Mantido para compatibilidade com código legado
    public void AtualizarSaldo(decimal saldo)
    {
        Saldo = saldo;
        DataAtualizacao = DateTime.UtcNow;
        SetUpdated();
    }
}
