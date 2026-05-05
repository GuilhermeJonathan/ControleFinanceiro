using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class Imovel : Entity
{
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public List<string> Pros { get; private set; } = [];
    public List<string> Contras { get; private set; } = [];
    public int Nota { get; private set; }
    public DateTime DataVisita { get; private set; }
    public string? NomeCorretor { get; private set; }
    public string? TelefoneCorretor { get; private set; }
    public string? Imobiliaria { get; private set; }
    /// <summary>"Rua" = casa de rua pública | "Condominio" = casa/apto em condomínio</summary>
    public string? Tipo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public ICollection<ImovelFoto> Fotos { get; private set; } = [];
    public ICollection<ImovelComentario> Comentarios { get; private set; } = [];

    private Imovel() : base(Guid.NewGuid()) { Descricao = ""; }

    public Imovel(string descricao, decimal valor, List<string> pros, List<string> contras,
                  int nota, DateTime dataVisita, string? nomeCorretor, string? telefoneCorretor,
                  string? imobiliaria, string? tipo, Guid usuarioId) : base(Guid.NewGuid())
    {
        Descricao = descricao; Valor = valor; Pros = pros; Contras = contras;
        Nota = nota; DataVisita = dataVisita; NomeCorretor = nomeCorretor;
        TelefoneCorretor = telefoneCorretor; Imobiliaria = imobiliaria; Tipo = tipo;
        UsuarioId = usuarioId;
    }

    public void Update(string descricao, decimal valor, List<string> pros, List<string> contras,
                       int nota, DateTime dataVisita, string? nomeCorretor,
                       string? telefoneCorretor, string? imobiliaria, string? tipo)
    {
        Descricao = descricao; Valor = valor; Pros = pros; Contras = contras;
        Nota = nota; DataVisita = dataVisita; NomeCorretor = nomeCorretor;
        TelefoneCorretor = telefoneCorretor; Imobiliaria = imobiliaria; Tipo = tipo;
        SetUpdated();
    }
}
