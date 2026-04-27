using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartoesCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoesCredito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorasTrabalhadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValorHora = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorasTrabalhadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceitasRecorrentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorHora = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    QuantidadeHoras = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Dia = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceitasRecorrentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaldosContas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaldosContas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParcelasCartao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartaoCreditoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: false),
                    TotalParcelas = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasCartao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelasCartao_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalTable: "CartoesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Situacao = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CartaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: true),
                    TotalParcelas = table.Column<int>(type: "integer", nullable: true),
                    GrupoParcelas = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceitaRecorrenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRecorrente = table.Column<bool>(type: "boolean", nullable: false),
                    ContaBancariaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_CartoesCredito_CartaoId",
                        column: x => x.CartaoId,
                        principalTable: "CartoesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lancamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lancamentos_ReceitasRecorrentes_ReceitaRecorrenteId",
                        column: x => x.ReceitaRecorrenteId,
                        principalTable: "ReceitasRecorrentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lancamentos_SaldosContas_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "SaldosContas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartoesCredito_UsuarioId",
                table: "CartoesCredito",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioId",
                table: "Categorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorasTrabalhadas_UsuarioId",
                table: "HorasTrabalhadas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CartaoId",
                table: "Lancamentos",
                column: "CartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CategoriaId",
                table: "Lancamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ReceitaRecorrenteId",
                table: "Lancamentos",
                column: "ReceitaRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_UsuarioId",
                table: "Lancamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelasCartao_CartaoCreditoId",
                table: "ParcelasCartao",
                column: "CartaoCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceitasRecorrentes_UsuarioId",
                table: "ReceitasRecorrentes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SaldosContas_UsuarioId",
                table: "SaldosContas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorasTrabalhadas");

            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "ParcelasCartao");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "ReceitasRecorrentes");

            migrationBuilder.DropTable(
                name: "SaldosContas");

            migrationBuilder.DropTable(
                name: "CartoesCredito");
        }
    }
}
