using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Documentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetDocumentos;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class DocumentosHandlersTests
{
    private readonly Mock<IDocumentoRepository> _repo = new();
    private readonly Mock<IArquivoStorage> _storage = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public DocumentosHandlersTests() => _user.Setup(u => u.UserId).Returns(UserId);

    private static Stream Conteudo() => new MemoryStream(new byte[] { 1, 2, 3 });

    [Fact]
    public async Task Upload_GravaNoStorage_ESalvaMetadado()
    {
        _storage.Setup(s => s.Configurado).Returns(true);
        _storage.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string p, Stream _, string __, CancellationToken ___) => p);
        Documento? cap = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()))
            .Callback<Documento, CancellationToken>((d, _) => cap = d);

        var h = new UploadDocumentoCommandHandler(_repo.Object, _storage.Object, _user.Object, _uow.Object);
        var id = await h.Handle(new UploadDocumentoCommand(AlvoDocumento.Ativo, Guid.NewGuid(), "escritura.pdf", "application/pdf", 3, Conteudo(), "Escritura"), CancellationToken.None);

        id.Should().NotBeEmpty();
        cap.Should().NotBeNull();
        cap!.UsuarioId.Should().Be(UserId);
        cap.Alvo.Should().Be(AlvoDocumento.Ativo);
        cap.Nome.Should().Be("escritura.pdf");
        cap.StoragePath.Should().Contain(UserId.ToString());
        _storage.Verify(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), "application/pdf", It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upload_StorageNaoConfigurado_ShouldThrow_SemSalvar()
    {
        _storage.Setup(s => s.Configurado).Returns(false);
        var h = new UploadDocumentoCommandHandler(_repo.Object, _storage.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new UploadDocumentoCommand(AlvoDocumento.Cliente, null, "x.pdf", "application/pdf", 3, Conteudo()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NaoEncontrado_ShouldThrow_SemEfeitos()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Documento?)null);
        var h = new DeleteDocumentoCommandHandler(_repo.Object, _storage.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new DeleteDocumentoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _repo.Verify(r => r.Remove(It.IsAny<Documento>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_DeOutroUsuario_ShouldThrow_SemEfeitos()
    {
        var doc = new Documento(Guid.NewGuid(), AlvoDocumento.Cliente, null, "x.pdf", "path/x.pdf", "application/pdf", 3, Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        var h = new DeleteDocumentoCommandHandler(_repo.Object, _storage.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new DeleteDocumentoCommand(doc.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repo.Verify(r => r.Remove(It.IsAny<Documento>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Ok_ApagaDoStorage_ERemoveMetadado()
    {
        var doc = new Documento(UserId, AlvoDocumento.Estrutura, Guid.NewGuid(), "contrato.pdf", "path/contrato.pdf", "application/pdf", 3, UserId);
        _repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _storage.Setup(s => s.Configurado).Returns(true);

        var h = new DeleteDocumentoCommandHandler(_repo.Object, _storage.Object, _user.Object, _uow.Object);
        await h.Handle(new DeleteDocumentoCommand(doc.Id), CancellationToken.None);

        _storage.Verify(s => s.DeleteAsync("path/contrato.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.Remove(doc), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
