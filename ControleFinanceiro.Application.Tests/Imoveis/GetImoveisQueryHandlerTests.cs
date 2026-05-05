using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Queries.GetImoveis;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class GetImoveisQueryHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetImoveisQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public GetImoveisQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _handler = new GetImoveisQueryHandler(_repoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsImoveisMappedToDto()
    {
        var imovel = new Imovel("Casa", 600_000m, ["Quintal"], ["Longe"],
            9, new DateTime(2026, 5, 1), "Carlos", "11777777777", "ImobABC", null, _userId);

        _repoMock
            .Setup(r => r.GetAllAsync(_userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([imovel]);

        var result = (await _handler.Handle(new GetImoveisQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Descricao.Should().Be("Casa");
        result[0].Nota.Should().Be(9);
        result[0].Pros.Should().ContainSingle(p => p == "Quintal");
    }

    [Fact]
    public async Task Handle_NoImoveis_ReturnsEmptyList()
    {
        _repoMock
            .Setup(r => r.GetAllAsync(_userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetImoveisQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}


