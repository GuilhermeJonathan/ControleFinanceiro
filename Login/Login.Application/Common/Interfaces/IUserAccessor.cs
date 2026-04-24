namespace Login.Application.Common.Interfaces;

public interface IUserAccessor
{
    Guid UserId { get; }
    string Email { get; }
    string UserType { get; }
}
