using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;

namespace ControleFinanceiro.Application.Relatorios;

/// <summary>Identidade visual da consultoria (nome + logo base64 + cor + rodapé).</summary>
public record RelatorioBranding(string? NomeConsultoria, string? LogoBase64, string? CorMarca, string? MensagemRodape = null);

/// <summary>Resultado calculado da simulação em destaque (favorita), para o relatório.</summary>
public record SimulacaoDestaqueDto(
    string Nome,
    int IdadeAtual,
    int IdadeAlvo,
    decimal AporteMensal,
    decimal RetiradaMensal,
    decimal TaxaRetornoRealAnualPct,
    decimal PatrimonioInicial,
    decimal PatrimonioNaIdadeAlvo,
    int? IdadeExtincao);

/// <summary>Tudo que o relatório patrimonial precisa, já consolidado.</summary>
public record RelatorioPatrimonialDados(
    string ClienteNome,
    string AssessorNome,
    DateTime GeradoEm,
    ResumoPatrimonialDto Resumo,
    ProjecaoDividasDto Projecao,
    ResumoInvestimentosDto Investimentos,
    SimulacaoDestaqueDto? SimulacaoDestaque,
    IEnumerable<PlanoAcaoDto> Planos);

/// <summary>Gera o PDF do relatório patrimonial (implementado na Infraestrutura com QuestPDF).</summary>
public interface IRelatorioPatrimonialGenerator
{
    byte[] Gerar(RelatorioPatrimonialDados dados, RelatorioBranding branding);
}

/// <summary>Tudo que o relatório de sucessão precisa (estruturas, beneficiários, contas, planos).</summary>
public record RelatorioSucessaoDados(
    string ClienteNome,
    string AssessorNome,
    DateTime GeradoEm,
    GrafoEstruturasDto Grafo,
    SucessaoDto Sucessao,
    ContasResultDto Contas,
    IEnumerable<PlanoAcaoDto> Planos);

/// <summary>Gera o PDF do relatório de sucessão (QuestPDF).</summary>
public interface IRelatorioSucessaoGenerator
{
    byte[] Gerar(RelatorioSucessaoDados dados, RelatorioBranding branding);
}
