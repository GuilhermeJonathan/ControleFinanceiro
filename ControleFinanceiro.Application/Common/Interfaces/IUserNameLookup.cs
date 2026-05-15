namespace ControleFinanceiro.Application.Common.Interfaces;

public interface IUserNameLookup
{
    Task<string?> GetNomeAsync(Guid userId, CancellationToken ct = default);
}
