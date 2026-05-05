using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.CreateImovel;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class CreateImovelCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateImovelCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateImovelCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Imovel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateImovelCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    private static CreateImovelCommand BuildCommand() => new(
        Descricao: "Apartamento Centro",
        Valor: 450_000m,
        Pros: ["LocalizaÃ§Ã£o", "Luminoso"],
        Contras: ["Sem vaga"],
        Nota: 8,
        DataVisita: new DateTime(2026, 5, 1),
        NomeCorretor: "JoÃ£o",
        TelefoneCorretor: "11999999999",
        Imobiliaria: "ImobXP",
        Tipo: null);

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateImovelAndReturnId()
    {
        var id = await _handler.Handle(BuildCommand(), CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Imovel>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetCurrentUserAsOwner()
    {
        Imovel? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Imovel>(), It.IsAny<CancellationToken>()))
            .Callback<Imovel, CancellationToken>((i, _) => captured = i)
            .Returns(Task.CompletedTask);

        await _handler.Handle(BuildCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.UsuarioId.Should().Be(_userId);
        captured.Nota.Should().Be(8);
        captured.Valor.Should().Be(450_000m);
    }
}


