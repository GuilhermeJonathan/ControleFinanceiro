using Login.Domain.Entities;

namespace Login.Application.Common.Interfaces;

public interface ITokenManager
{
    string Generate(User user);
    bool Validate(string token);
    void Invalidate(Guid userId);
}
