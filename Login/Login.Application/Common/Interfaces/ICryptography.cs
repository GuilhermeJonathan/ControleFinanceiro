namespace Login.Application.Common.Interfaces;

public interface ICryptography
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
    Task<bool> VerifyAsync(string plainText, string hash);
}
