namespace ControleFinanceiro.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid RealUserId { get; }
    string? RealUserName { get; }
}
