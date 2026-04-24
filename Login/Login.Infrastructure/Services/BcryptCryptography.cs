using Login.Application.Common.Interfaces;

namespace Login.Infrastructure.Services;

public class BcryptCryptography : ICryptography
{
    public string Hash(string plainText)
        => BCrypt.Net.BCrypt.HashPassword(plainText);

    public bool Verify(string plainText, string hash)
        => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
