using ControleFinanceiro.Application.Consultoria.Queries.GetConsultoriaLogo;
using FluentAssertions;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class ConsultoriaLogoParseTests
{
    // 1x1 PNG (base64)
    private const string PngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

    [Fact]
    public void Parse_DataUrl_ExtraiContentTypeEBytes()
    {
        var dto = GetConsultoriaLogoQueryHandler.Parse($"data:image/png;base64,{PngBase64}");
        dto.Should().NotBeNull();
        dto!.ContentType.Should().Be("image/png");
        dto.Bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Parse_Base64Puro_AssumePng()
    {
        var dto = GetConsultoriaLogoQueryHandler.Parse(PngBase64);
        dto.Should().NotBeNull();
        dto!.ContentType.Should().Be("image/png");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-base64-!!!")]
    public void Parse_Invalido_RetornaNull(string? entrada)
    {
        GetConsultoriaLogoQueryHandler.Parse(entrada).Should().BeNull();
    }
}
