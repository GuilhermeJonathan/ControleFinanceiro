using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Cenário extraordinário de uma simulação (aporte ou resgate extra) aplicado em
/// uma idade única (IdadeFim nulo) ou ao longo de uma faixa de idades (recorrente mensal).
/// Owned type de <see cref="SimulacaoPatrimonial"/>.
/// </summary>
public class Cenario
{
    public string Nome { get; private set; } = string.Empty;
    public TipoCenario Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public int IdadeInicio { get; private set; }
    public int? IdadeFim { get; private set; }

    private Cenario() { }

    public Cenario(string nome, TipoCenario tipo, decimal valor, int idadeInicio, int? idadeFim)
    {
        Nome = nome;
        Tipo = tipo;
        Valor = valor;
        IdadeInicio = idadeInicio;
        IdadeFim = idadeFim;
    }
}

/// <summary>
/// Simulação de projeção patrimonial (planejamento de longo prazo — "proteção patrimonial").
/// Guarda os parâmetros de acúmulo/decumulação; o cálculo da projeção é feito no cliente.
/// Bounded context isolado do FinDog pessoal.
/// </summary>
public class SimulacaoPatrimonial
{
    private readonly List<Cenario> _cenarios = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Favorita { get; private set; }
    public int IdadeAtual { get; private set; }
    public int IdadeAlvo { get; private set; }
    /// <summary>Patrimônio inicial informado. Ignorado quando ModoAutomatico = true (o cliente usa o patrimônio consolidado).</summary>
    public decimal PatrimonioInicial { get; private set; }
    public bool ModoAutomatico { get; private set; }
    public decimal AporteMensal { get; private set; }
    public decimal TaxaRetornoRealAnualPct { get; private set; }
    public decimal RetiradaMensal { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    public IReadOnlyCollection<Cenario> Cenarios => _cenarios.AsReadOnly();

    private SimulacaoPatrimonial() { }

    public SimulacaoPatrimonial(
        Guid usuarioId, string nome, bool favorita, int idadeAtual, int idadeAlvo,
        decimal patrimonioInicial, bool modoAutomatico, decimal aporteMensal,
        decimal taxaRetornoRealAnualPct, decimal retiradaMensal, IEnumerable<Cenario>? cenarios = null)
    {
        UsuarioId = usuarioId;
        AplicarCampos(nome, favorita, idadeAtual, idadeAlvo, patrimonioInicial, modoAutomatico,
            aporteMensal, taxaRetornoRealAnualPct, retiradaMensal, cenarios);
    }

    public void Atualizar(
        string nome, bool favorita, int idadeAtual, int idadeAlvo,
        decimal patrimonioInicial, bool modoAutomatico, decimal aporteMensal,
        decimal taxaRetornoRealAnualPct, decimal retiradaMensal, IEnumerable<Cenario>? cenarios = null)
    {
        AplicarCampos(nome, favorita, idadeAtual, idadeAlvo, patrimonioInicial, modoAutomatico,
            aporteMensal, taxaRetornoRealAnualPct, retiradaMensal, cenarios);
        AtualizadoEm = DateTime.UtcNow;
    }

    private void AplicarCampos(
        string nome, bool favorita, int idadeAtual, int idadeAlvo,
        decimal patrimonioInicial, bool modoAutomatico, decimal aporteMensal,
        decimal taxaRetornoRealAnualPct, decimal retiradaMensal, IEnumerable<Cenario>? cenarios)
    {
        Nome = nome;
        Favorita = favorita;
        IdadeAtual = idadeAtual;
        IdadeAlvo = idadeAlvo;
        PatrimonioInicial = patrimonioInicial;
        ModoAutomatico = modoAutomatico;
        AporteMensal = aporteMensal;
        TaxaRetornoRealAnualPct = taxaRetornoRealAnualPct;
        RetiradaMensal = retiradaMensal;
        _cenarios.Clear();
        if (cenarios != null) _cenarios.AddRange(cenarios);
    }
}
