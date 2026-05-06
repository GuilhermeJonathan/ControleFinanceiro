using Login.Application.Common.Interfaces;

namespace Login.Infrastructure.Services;

public class BcryptCryptography : ICryptography
{
    // Work factor 10 → ~100-200ms; seguro e não bloqueia desnecessariamente.
    // O padrão do BCrypt.Net (11) dobra o custo sem ganho prático para este app.
    private const int WorkFactor = 10;

    public string Hash(string plainText)
        => BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);

    // BCrypt.Verify é CPU-bound — roda fora da thread do pool para não travar o ASP.NET.
    public Task<bool> VerifyAsync(string plainText, string hash)
        => Task.Run(() => BCrypt.Net.BCrypt.Verify(plainText, hash));

    // Mantém o síncrono por compatibilidade com a interface atual
    public bool Verify(string plainText, string hash)
        => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
