namespace ControleFinanceiro.Domain.Enums;

/// <summary>Tipo de ativo patrimonial (foco em alta renda).</summary>
public enum TipoAtivo
{
    Imovel        = 1,
    Veiculo       = 2,
    Embarcacao    = 3,
    Aeronave      = 4,
    Participacao  = 5,
    Investimento  = 6,
    Outro         = 99,
}

/// <summary>Tipo de investimento financeiro.</summary>
public enum TipoInvestimento
{
    Acoes        = 1,
    FII          = 2,
    ETF          = 3,
    RendaFixa    = 4,
    Multimercado = 5,
    Cripto       = 6,
    Exterior     = 7,
    Outro        = 99,
}

/// <summary>Moedas suportadas na consolidação patrimonial.</summary>
public enum MoedaPatrimonio
{
    BRL = 1,
    USD = 2,
    EUR = 3,
    CHF = 4,
    GBP = 5,
}
