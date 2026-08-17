namespace ControleFinanceiro.Domain.Entities;

/// <summary>A qual entidade o documento está anexado.</summary>
public enum AlvoDocumento
{
    Cliente = 1,    // documento do cliente (nível pessoa) — AlvoId null
    Ativo = 2,      // anexado a um AtivoPatrimonial
    Estrutura = 3,  // anexado a uma Estrutura (contrato social, ata…)
    Conta = 4,      // anexado a uma ContaFinanceira (extrato, contrato…)
}

/// <summary>
/// Documento anexado (armazenado no storage — Supabase). Pertence a um cliente e pode estar
/// vinculado a um Ativo/Estrutura ou ficar no nível do próprio cliente. O binário fica no
/// bucket; aqui guardamos só os metadados + a chave (<see cref="StoragePath"/>).
/// </summary>
public class Documento
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    /// <summary>Dono do documento (o cliente).</summary>
    public Guid UsuarioId { get; private set; }
    public AlvoDocumento Alvo { get; private set; }
    /// <summary>Id do Ativo/Estrutura quando <see cref="Alvo"/> não é Cliente. Null = nível cliente.</summary>
    public Guid? AlvoId { get; private set; }
    /// <summary>Nome original do arquivo (exibição).</summary>
    public string Nome { get; private set; } = string.Empty;
    /// <summary>Chave do objeto no bucket (ex.: "{usuarioId}/ativo/{id}/{guid}-nome.pdf").</summary>
    public string StoragePath { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public long Tamanho { get; private set; }
    /// <summary>Categoria/rótulo livre opcional (ex.: "Contrato social", "Escritura").</summary>
    public string? Categoria { get; private set; }
    /// <summary>Quem enviou (cliente ou assessor operando via view-as).</summary>
    public Guid EnviadoPor { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Documento() { }

    public Documento(Guid usuarioId, AlvoDocumento alvo, Guid? alvoId, string nome, string storagePath,
        string? contentType, long tamanho, Guid enviadoPor, string? categoria = null)
    {
        UsuarioId = usuarioId;
        Alvo = alvo;
        AlvoId = alvo == AlvoDocumento.Cliente ? null : alvoId;
        Nome = nome;
        StoragePath = storagePath;
        ContentType = contentType;
        Tamanho = tamanho;
        EnviadoPor = enviadoPor;
        Categoria = categoria;
    }
}
