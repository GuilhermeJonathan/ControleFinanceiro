namespace ControleFinanceiro.Domain.Entities;

public class VinculoFamiliar
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DonoId { get; private set; }       // quem compartilha os dados
    public Guid MembroId { get; private set; }     // quem vai ver os dados do dono (null até aceitar)
    public string CodigoConvite { get; private set; }  // 6 chars alfanumérico
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AceitoEm { get; private set; }   // null = ainda não aceito
    public string? NomeMembro { get; private set; }   // guardado no aceite para exibir na UI

    private VinculoFamiliar() { }

    public static VinculoFamiliar Criar(Guid donoId, string codigo) =>
        new() { DonoId = donoId, CodigoConvite = codigo.ToUpperInvariant() };

    public void Aceitar(Guid membroId, string nomeMembro)
    {
        if (AceitoEm != null) throw new InvalidOperationException("Convite já utilizado.");
        MembroId = membroId;
        NomeMembro = nomeMembro;
        AceitoEm = DateTime.UtcNow;
    }
}
