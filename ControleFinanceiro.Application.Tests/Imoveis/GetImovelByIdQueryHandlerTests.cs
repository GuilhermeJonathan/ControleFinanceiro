using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Queries.GetImovelById;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class GetImovelByIdQueryHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetImovelByIdQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public GetImovelByIdQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _handler = new GetImovelByIdQueryHandler(_repoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingImovel_ReturnsMappedDto()
    {
        var imovelId = Guid.NewGuid();
        var imovel = new Imovel("Sobrado", 750_000m, ["Garagem"], [],
            7, new DateTime(2026, 5, 2), null, null, null, null, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imovel);

        var result = await _handler.Handle(new GetImovelByIdQuery(imovelId), CancellationToken.None);

        result.Descricao.Should().Be("Sobrado");
        result.Valor.Should().Be(750_000m);
        result.Nota.Should().Be(7);
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldThrowKeyNotFoundException()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new GetImovelByIdQuery(imovelId), CancellationToken.None));
    }
}


