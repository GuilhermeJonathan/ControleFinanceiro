using ControleFinanceiro.Application.Assessoria.Commands.EnviarConviteEmail;
using ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;
using ControleFinanceiro.Application.Assessoria.Queries.GetConvitesHistorico;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class ConviteEmailHandlersTests
{
    private static readonly Guid AssessorId = Guid.NewGuid();

    // ── EnviarConviteEmail ───────────────────────────────────────────────────

    [Fact]
    public async Task EnviarConvite_ShouldGenerateCodeAndSendEmail()
    {
        var mediatorMock = new Mock<ISender>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GerarConviteAssessoriaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("ABC123");
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.RealUserName).Returns("Assessor Teste");
        var emailMock = new Mock<IEmailService>();
        var consultoriaMock = new Mock<IConsultoriaConfigRepository>();
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var handler = new EnviarConviteEmailCommandHandler(
            mediatorMock.Object, currentUserMock.Object, emailMock.Object, consultoriaMock.Object, configMock.Object);
        var codigo = await handler.Handle(new EnviarConviteEmailCommand("novo@cliente.com"), CancellationToken.None);

        codigo.Should().Be("ABC123");
        emailMock.Verify(e => e.SendAsync(
            "novo@cliente.com", It.IsAny<string>(),
            It.Is<string>(s => s.Contains("convidou")),
            It.Is<string>(b => b.Contains("ABC123")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarConvite_LimiteAtingido_ShouldPropagateAndNotSendEmail()
    {
        var mediatorMock = new Mock<ISender>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GerarConviteAssessoriaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Limite de clientes atingido."));
        var emailMock = new Mock<IEmailService>();

        var handler = new EnviarConviteEmailCommandHandler(
            mediatorMock.Object, new Mock<ICurrentUser>().Object, emailMock.Object,
            new Mock<IConsultoriaConfigRepository>().Object,
            new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new EnviarConviteEmailCommand("novo@cliente.com"), CancellationToken.None));
        emailMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── GetConvitesHistorico ─────────────────────────────────────────────────

    [Fact]
    public async Task Historico_ShouldClassifyStatusCorrectly()
    {
        var pendente = VinculoAssessoria.Criar(AssessorId, "AAA111");
        var aceito = VinculoAssessoria.Criar(AssessorId, "BBB222");
        aceito.Aceitar(Guid.NewGuid(), "Cliente A");
        var revogado = VinculoAssessoria.Criar(AssessorId, "CCC333");
        revogado.Aceitar(Guid.NewGuid(), "Cliente B");
        revogado.Revogar();

        var repoMock = new Mock<IVinculoAssessoriaRepository>();
        repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { pendente, aceito, revogado });
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);

        var handler = new GetConvitesHistoricoQueryHandler(repoMock.Object, currentUserMock.Object);
        var result = (await handler.Handle(new GetConvitesHistoricoQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(3);
        result.Single(c => c.CodigoConvite == "AAA111").Status.Should().Be("Pendente");
        result.Single(c => c.CodigoConvite == "BBB222").Status.Should().Be("Aceito");
        result.Single(c => c.CodigoConvite == "CCC333").Status.Should().Be("Revogado");
    }
}
