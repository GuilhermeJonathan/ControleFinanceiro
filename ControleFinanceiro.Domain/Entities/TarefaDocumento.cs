namespace ControleFinanceiro.Domain.Entities;

public enum StatusTarefaDocumento { Pendente = 1, Concluida = 2 }

/// <summary>
/// Tarefa genérica atribuída pelo ASSESSOR ao CLIENTE ("faça a ação X"). Vira alerta pro
/// cliente (sino + banner na Home); ao concluir, o cliente marca como feita. Pode ter um
/// ATALHO opcional para uma tela do app (ex.: "documentos", "ativos"), levando o cliente
/// direto ao lugar da ação. (Nome histórico da classe/tabela: TarefaDocumento.)
/// </summary>
public class TarefaDocumento
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    /// <summary>Assessor que criou a tarefa.</summary>
    public Guid AssessorId { get; private set; }
    /// <summary>Cliente que deve executar a ação.</summary>
    public Guid ClienteId { get; private set; }
    /// <summary>O que o cliente deve fazer (ex.: "Anexar o contrato social").</summary>
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    /// <summary>Rota do app para onde levar o cliente ao tocar (ex.: "documentos"). null = tarefa sem atalho.</summary>
    public string? AtalhoRota { get; private set; }
    public StatusTarefaDocumento Status { get; private set; } = StatusTarefaDocumento.Pendente;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? ConcluidaEm { get; private set; }

    private TarefaDocumento() { }

    public TarefaDocumento(Guid assessorId, Guid clienteId, string titulo, string? descricao = null, string? atalhoRota = null)
    {
        AssessorId = assessorId;
        ClienteId = clienteId;
        Titulo = titulo;
        Descricao = descricao;
        AtalhoRota = string.IsNullOrWhiteSpace(atalhoRota) ? null : atalhoRota;
    }

    public void Concluir()
    {
        Status = StatusTarefaDocumento.Concluida;
        ConcluidaEm = DateTime.UtcNow;
    }
}
