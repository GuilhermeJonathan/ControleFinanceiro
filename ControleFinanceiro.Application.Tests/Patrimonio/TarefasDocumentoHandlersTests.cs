using System.Runtime.CompilerServices;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Documentos;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class TarefasDocumentoHandlersTests
{
    private readonly Mock<ITarefaDocumentoRepository> _repo = new();
    private readonly Mock<IVinculoAssessoriaRepository> _vinculo = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserNameLookup> _lookup = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<IConfiguration> _config = new();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ClienteId = Guid.NewGuid();

    public TarefasDocumentoHandlersTests() => _user.Setup(u => u.RealUserId).Returns(UserId);

    private CriarTarefaDocumentoCommandHandler CriarHandler() =>
        new(_repo.Object, _vinculo.Object, _user.Object, _uow.Object, _lookup.Object, _email.Object,
            _consultoria.Object, _config.Object, NullLogger<CriarTarefaDocumentoCommandHandler>.Instance);

    [Fact]
    public async Task Criar_ComVinculoAtivo_Cria()
    {
        var vinc = (VinculoAssessoria)RuntimeHelpers.GetUninitializedObject(typeof(VinculoAssessoria));
        _vinculo.Setup(v => v.GetVinculoAtivoAsync(UserId, ClienteId, It.IsAny<CancellationToken>())).ReturnsAsync(vinc);
        TarefaDocumento? cap = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<TarefaDocumento>(), It.IsAny<CancellationToken>()))
            .Callback<TarefaDocumento, CancellationToken>((x, _) => cap = x);

        var h = CriarHandler();
        await h.Handle(new CriarTarefaDocumentoCommand(ClienteId, "Anexar contrato social", null, "documentos"), CancellationToken.None);

        cap.Should().NotBeNull();
        cap!.AssessorId.Should().Be(UserId);
        cap.ClienteId.Should().Be(ClienteId);
        cap.Status.Should().Be(StatusTarefaDocumento.Pendente);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_SemVinculo_ShouldThrow_SemEfeitos()
    {
        _vinculo.Setup(v => v.GetVinculoAtivoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);
        var h = CriarHandler();

        var act = () => h.Handle(new CriarTarefaDocumentoCommand(ClienteId, "X", null, null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<TarefaDocumento>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Concluir_MarcaConcluida()
    {
        var tarefa = new TarefaDocumento(Guid.NewGuid(), UserId, "Anexe RG");
        _repo.Setup(r => r.GetByIdAsync(tarefa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tarefa);

        var h = new ConcluirTarefaDocumentoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        await h.Handle(new ConcluirTarefaDocumentoCommand(tarefa.Id), CancellationToken.None);

        tarefa.Status.Should().Be(StatusTarefaDocumento.Concluida);
        tarefa.ConcluidaEm.Should().NotBeNull();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Concluir_NaoEncontrado_ShouldThrow()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TarefaDocumento?)null);
        var h = new ConcluirTarefaDocumentoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new ConcluirTarefaDocumentoCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_DeOutroAssessor_ShouldThrow_SemEfeitos()
    {
        var tarefa = new TarefaDocumento(Guid.NewGuid(), ClienteId, "X"); // AssessorId != UserId
        _repo.Setup(r => r.GetByIdAsync(tarefa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tarefa);
        var h = new DeleteTarefaDocumentoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new DeleteTarefaDocumentoCommand(tarefa.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repo.Verify(r => r.Remove(It.IsAny<TarefaDocumento>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
