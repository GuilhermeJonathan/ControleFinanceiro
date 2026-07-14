using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;

namespace ControleFinanceiro.Application.Relatorios;

/// <summary>Identidade visual do assessor, enviada pelo app (nome + logo base64 + cor).</summary>
public record RelatorioBranding(string? NomeConsultoria, string? LogoBase64, string? CorMarca);

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
    SimulacaoDestaqueDto? SimulacaoDestaque);

/// <summary>Gera o PDF do relatório patrimonial (implementado na Infraestrutura com QuestPDF).</summary>
public interface IRelatorioPatrimonialGenerator
{
    byte[] Gerar(RelatorioPatrimonialDados dados, RelatorioBranding branding);
}
