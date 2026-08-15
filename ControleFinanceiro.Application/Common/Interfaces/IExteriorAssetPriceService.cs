namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Cotação de ativos negociados no EXTERIOR (bolsas globais: LSE, NYSE, Euronext…),
/// via provedor global (Yahoo Finance). Usada para investimentos cujo Tipo está
/// marcado como Exterior. Ativos nacionais (B3) continuam usando <see cref="IAssetPriceService"/> (brapi).
/// </summary>
public interface IExteriorAssetPriceService : IAssetPriceService;
