using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.SaldoContas.Queries.GetSaldos;

public record SaldoContaDto(Guid Id, string Banco, decimal Saldo, TipoConta Tipo, DateTime DataAtualizacao);
