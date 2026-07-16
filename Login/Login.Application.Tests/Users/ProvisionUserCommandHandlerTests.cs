using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.ProvisionUser;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class ProvisionUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICryptography> _cryptoMock = new();
    private readonly Mock<ITokenManager> _tokenManagerMock = new();
    private readonly ProvisionUserCommandHandler _handler;

    public ProvisionUserCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _tokenManagerMock.Setup(t => t.Generate(It.IsAny<User>())).Returns("token_value");
        _cryptoMock.Setup(c => c.Hash(It.IsAny<string>())).Returns("hashed");
        _handler = new ProvisionUserCommandHandler(
            _userRepoMock.Object, _uowMock.Object, _cryptoMock.Object, _tokenManagerMock.Object);
    }

    private User BuildUser(string hash = "hashed_pass") =>
        new User(Guid.NewGuid(), "Maria", "maria@example.com", "", hash, UserType.User);

    [Fact]
    public async Task Handle_EmailNovo_CriaContaERetornaTokenComCreatedTrue()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("novo@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var cmd = new ProvisionUserCommand("Novo Cliente", "novo@example.com", "senha123", null, (int)UserType.User);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Created.Should().BeTrue();
        result.AccessToken.Should().Be("token_value");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailNovoCorretor_CriaComTipoCorretor()
    {
        User? capturado = null;
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturado = u)
            .Returns(Task.CompletedTask);

        var cmd = new ProvisionUserCommand("Corretor X", "corretor@example.com", "senha123", null, (int)UserType.Corretor);
        await _handler.Handle(cmd, CancellationToken.None);

        capturado.Should().NotBeNull();
        capturado!.UserTypeId.Should().Be(UserType.Corretor);
    }

    [Fact]
    public async Task Handle_EmailExistenteSenhaCorreta_VinculaSemCriar()
    {
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("maria@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock.Setup(c => c.VerifyAsync("senha123", "hashed_pass")).ReturnsAsync(true);

        var cmd = new ProvisionUserCommand("Maria", "maria@example.com", "senha123", null, (int)UserType.User);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Created.Should().BeFalse();
        result.UserId.Should().Be(user.Id);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailExistenteSenhaErrada_LancaUnauthorizedENaoPersiste()
    {
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("maria@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock.Setup(c => c.VerifyAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var cmd = new ProvisionUserCommand("Maria", "maria@example.com", "errada", null, (int)UserType.User);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(cmd, CancellationToken.None));
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
