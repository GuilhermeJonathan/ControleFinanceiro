using Login.Application.Users.Queries.EmailExists;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class EmailExistsQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private EmailExistsQueryHandler Build() => new(_repo.Object);

    [Fact]
    public async Task Handle_EmailExistente_RetornaTrueComTipo()
    {
        var user = new User(Guid.NewGuid(), "Ana", "ana@x.com", "", "hash", UserType.Corretor);
        _repo.Setup(r => r.GetByEmailAsync("ana@x.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await Build().Handle(new EmailExistsQuery("ana@x.com"), CancellationToken.None);

        result.Exists.Should().BeTrue();
        result.UserTypeId.Should().Be((int)UserType.Corretor);
    }

    [Fact]
    public async Task Handle_EmailInexistente_RetornaFalse()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await Build().Handle(new EmailExistsQuery("nao@existe.com"), CancellationToken.None);

        result.Exists.Should().BeFalse();
        result.UserTypeId.Should().BeNull();
    }
}
