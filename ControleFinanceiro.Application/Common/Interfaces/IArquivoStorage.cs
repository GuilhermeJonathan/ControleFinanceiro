namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Armazenamento de arquivos (implementado sobre o Supabase Storage via protocolo S3).
/// O backend faz proxy: recebe o arquivo e grava no bucket; a secret key fica só no servidor.
/// </summary>
public interface IArquivoStorage
{
    /// <summary>true quando as credenciais/endpoint estão configurados (env vars).</summary>
    bool Configurado { get; }

    /// <summary>Grava o conteúdo na chave informada e retorna a própria chave.</summary>
    Task<string> UploadAsync(string caminho, Stream conteudo, string contentType, CancellationToken ct = default);

    /// <summary>Baixa o conteúdo do objeto (stream de leitura).</summary>
    Task<Stream> DownloadAsync(string caminho, CancellationToken ct = default);

    Task DeleteAsync(string caminho, CancellationToken ct = default);
}
