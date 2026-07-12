using Login.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Infrastructure.Migrations
{
    /// <summary>
    /// Recuperação de DDL aplicado manualmente em prod sem migration (12/07/2026):
    /// colunas de plano em Users e a tabela MercadoPagoSubscriptions existiam no
    /// Supabase mas em nenhuma migration — bancos novos nasciam sem elas.
    /// SQL idempotente: no-op em prod, cria em banco novo.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260712150000_AddPlanColumnsRecovery")]
    public partial class AddPlanColumnsRecovery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "PlanType" integer NOT NULL DEFAULT 0;
                ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TrialStartedAt" timestamp with time zone;
                ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "PlanExpiresAt" timestamp with time zone;

                CREATE TABLE IF NOT EXISTS "MercadoPagoSubscriptions" (
                    "Id" uuid NOT NULL,
                    "UserId" uuid NOT NULL,
                    "MpSubscriptionId" character varying(100) NOT NULL,
                    "PlanType" integer NOT NULL,
                    "Status" character varying(30) NOT NULL,
                    "LastPaymentId" character varying(100),
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone,
                    CONSTRAINT "PK_MercadoPagoSubscriptions" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_MercadoPagoSubscriptions_MpSubscriptionId"
                    ON "MercadoPagoSubscriptions" ("MpSubscriptionId");
                CREATE INDEX IF NOT EXISTS "IX_MercadoPagoSubscriptions_UserId"
                    ON "MercadoPagoSubscriptions" ("UserId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" DROP COLUMN IF EXISTS "PlanType";
                ALTER TABLE "Users" DROP COLUMN IF EXISTS "TrialStartedAt";
                ALTER TABLE "Users" DROP COLUMN IF EXISTS "PlanExpiresAt";
                DROP TABLE IF EXISTS "MercadoPagoSubscriptions";
                """);
        }
    }
}
