using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.UpdateImovel;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class UpdateImovelCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly UpdateImovelCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public UpdateImovelCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateImovelCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    private Imovel BuildImovel() => new(
        "Antigo", 300_000m, [], [], 5,
        new DateTime(2026, 4, 1), null, null, null, null, _userId);

    private static UpdateImovelCommand BuildCommand(Guid id) => new(
        Id: id,
        Descricao: "Novo Desc",
        Valor: 500_000m,
        Pros: ["Piscina"],
        Contras: [],
        Nota: 9,
        DataVisita: new DateTime(2026, 5, 5),
        NomeCorretor: "Maria",
        TelefoneCorretor: "11888888888",
        Imobiliaria: "ImobXYZ",
        Tipo: null);

    [Fact]
    public async Task Handle_ExistingImovel_ShouldUpdateAndSave()
    {
        var imovelId = Guid.NewGuid();
        var imovel = BuildImovel();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imovel);

        await _handler.Handle(BuildCommand(imovelId), CancellationToken.None);

        imovel.Descricao.Should().Be("Novo Desc");
        imovel.Nota.Should().Be(9);
        imovel.Valor.Should().Be(500_000m);
        _repoMock.Verify(r => r.Update(imovel), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldThrowKeyNotFoundException()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(BuildCommand(imovelId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldNotCallUpdateOrSave()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        try { await _handler.Handle(BuildCommand(imovelId), CancellationToken.None); } catch { }

        _repoMock.Verify(r => r.Update(It.IsAny<Imovel>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


