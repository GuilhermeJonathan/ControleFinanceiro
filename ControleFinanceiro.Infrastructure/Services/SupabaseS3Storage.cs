using Amazon.S3;
using Amazon.S3.Model;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Armazenamento sobre o Supabase Storage via protocolo S3 (AWS SDK). As credenciais vêm de
/// env vars (nunca em código): SUPABASE_STORAGE_ENDPOINT / _REGION / _BUCKET, SUPABASE_S3_ACCESS_KEY /
/// _SECRET_KEY. Se faltarem, <see cref="Configurado"/> = false e as operações lançam (o controller
/// responde 503) — sem quebrar o resto da API.
/// </summary>
public class SupabaseS3Storage : IArquivoStorage
{
    private readonly IAmazonS3? _s3;
    private readonly string _bucket;

    public SupabaseS3Storage(IConfiguration cfg)
    {
        // Preferimos a seção "Storage" do appsettings (Render sobrescreve via Storage__AccessKey etc.);
        // com fallback para nomes flat SUPABASE_* (caso já configurados assim no ambiente).
        var endpoint = cfg["Storage:Endpoint"] ?? cfg["SUPABASE_STORAGE_ENDPOINT"];
        var region = cfg["Storage:Region"] ?? cfg["SUPABASE_STORAGE_REGION"] ?? "sa-east-1";
        _bucket = cfg["Storage:Bucket"] ?? cfg["SUPABASE_STORAGE_BUCKET"] ?? "patrimonio";
        var accessKey = cfg["Storage:AccessKey"] ?? cfg["SUPABASE_S3_ACCESS_KEY"];
        var secretKey = cfg["Storage:SecretKey"] ?? cfg["SUPABASE_S3_SECRET_KEY"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
        {
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true,           // Supabase exige path-style
                AuthenticationRegion = region,
            };
            _s3 = new AmazonS3Client(accessKey, secretKey, config);
        }
    }

    public bool Configurado => _s3 != null;

    private IAmazonS3 Cliente => _s3
        ?? throw new InvalidOperationException("Storage não configurado (defina SUPABASE_STORAGE_* / SUPABASE_S3_* no ambiente).");

    public async Task<string> UploadAsync(string caminho, Stream conteudo, string contentType, CancellationToken ct = default)
    {
        await Cliente.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = caminho,
            InputStream = conteudo,
            ContentType = contentType,
            AutoCloseStream = false,
            DisablePayloadSigning = true,   // Supabase S3 não aceita streaming-signature (aws-chunked)
        }, ct);
        return caminho;
    }

    public async Task<Stream> DownloadAsync(string caminho, CancellationToken ct = default)
    {
        var resp = await Cliente.GetObjectAsync(_bucket, caminho, ct);
        return resp.ResponseStream;
    }

    public Task DeleteAsync(string caminho, CancellationToken ct = default) =>
        Cliente.DeleteObjectAsync(_bucket, caminho, ct);
}
