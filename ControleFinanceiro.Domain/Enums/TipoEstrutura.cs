namespace ControleFinanceiro.Domain.Enums;

/// <summary>Tipo de estrutura patrimonial/sucessória do cliente.</summary>
public enum TipoEstrutura
{
    Trust = 1,
    HoldingPatrimonial = 2,
    HoldingParticipacoes = 3,
    Offshore = 4,
    EmpresaOperacional = 5,
    PPLI = 6,
    Outro = 99,
}

/// <summary>Natureza da ligação entre estruturas no grafo de participações.</summary>
public enum TipoRelacaoEstrutura
{
    PropriedadeDireta = 1,
    BeneficioTrust = 2,
}
