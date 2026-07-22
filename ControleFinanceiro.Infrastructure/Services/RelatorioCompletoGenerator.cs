using ControleFinanceiro.Application.Relatorios;
using QuestPDF.Fluent;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Relatório completo: mescla o documento patrimonial + o de sucessão num único PDF
/// (reaproveita os dois geradores; cada seção mantém seu cabeçalho/rodapé com a marca).
/// </summary>
public class RelatorioCompletoGenerator : IRelatorioCompletoGenerator
{
    private readonly RelatorioPatrimonialGenerator _patrimonial = new();
    private readonly RelatorioSucessaoGenerator _sucessao = new();

    public byte[] Gerar(RelatorioPatrimonialDados patrimonial, RelatorioSucessaoDados sucessao, RelatorioBranding branding)
    {
        var docPat = _patrimonial.Build(patrimonial, branding);
        var docSuc = _sucessao.Build(sucessao, branding);
        return Document.Merge(docPat, docSuc).GeneratePdf();
    }
}
