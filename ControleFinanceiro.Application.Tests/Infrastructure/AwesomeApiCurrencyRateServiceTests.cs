using ControleFinanceiro.Infrastructure.Services;
using FluentAssertions;

namespace ControleFinanceiro.Application.Tests.Infrastructure;

public class AwesomeApiCurrencyRateServiceTests
{
    [Fact]
    public void ParseBid_RespostaValida_ShouldReturnBid()
    {
        var json = "{\"USDBRL\":{\"code\":\"USD\",\"codein\":\"BRL\",\"bid\":\"5.4231\"}}";
        AwesomeApiCurrencyRateService.ParseBid(json).Should().Be(5.4231m);
    }

    [Fact]
    public void ParseBid_SemBid_ShouldReturnNull()
    {
        var json = "{\"USDBRL\":{\"code\":\"USD\"}}"; // bid ausente → default "0"
        AwesomeApiCurrencyRateService.ParseBid(json).Should().Be(0m);
    }

    [Fact]
    public void ParseBid_JsonVazio_ShouldReturnNull()
    {
        AwesomeApiCurrencyRateService.ParseBid("{}").Should().BeNull();
    }
}
