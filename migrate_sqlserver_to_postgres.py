"""
Migração SQL Server → PostgreSQL (Supabase)
==========================================
Lê os dados do SQL Server e gera um arquivo .sql com INSERTs compatíveis
com PostgreSQL. Rode o arquivo gerado direto no DBeaver.

Pré-requisito:
    pip install pyodbc

Preencha as variáveis de conexão abaixo antes de rodar.
"""

import pyodbc
import uuid
import decimal
from datetime import datetime, timezone

# ──────────────────────────────────────────────
#  CONFIGURAÇÃO – PREENCHA AQUI
# ──────────────────────────────────────────────
SQL_SERVER_CONN = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=localhost;"
    "Database=ControleFinanceiroDB;"
    "Trusted_Connection=yes;"
    "TrustServerCertificate=yes;"
)

OUTPUT_FILE = "dados_para_postgres.sql"
# ──────────────────────────────────────────────

def esc(val):
    """Converte qualquer valor Python para literal SQL do PostgreSQL."""
    if val is None:
        return "NULL"
    if isinstance(val, bool):
        return "true" if val else "false"
    if isinstance(val, int):
        return str(val)
    if isinstance(val, float):
        return str(val)
    if isinstance(val, decimal.Decimal):
        return str(val)
    if isinstance(val, datetime):
        # garante UTC com timezone
        if val.tzinfo is None:
            val = val.replace(tzinfo=timezone.utc)
        return f"'{val.strftime('%Y-%m-%d %H:%M:%S+00')}'"
    if isinstance(val, uuid.UUID):
        return f"'{val}'"
    # strings: escapa aspas simples
    s = str(val).replace("'", "''")
    return f"'{s}'"

def fetch_table(cursor, table_name):
    cursor.execute(f'SELECT * FROM "{table_name}"')
    cols = [col[0] for col in cursor.description]
    rows = cursor.fetchall()
    return cols, rows

def gen_inserts(table_name, cols, rows):
    if not rows:
        return f"-- Tabela {table_name}: sem dados\n"

    lines = [f"-- {table_name} ({len(rows)} registros)"]
    cols_sql = ", ".join(f'"{c}"' for c in cols)

    for row in rows:
        values_sql = ", ".join(esc(v) for v in row)
        lines.append(
            f'INSERT INTO "{table_name}" ({cols_sql}) VALUES ({values_sql}) '
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
    lines.append("")
    return "\n".join(lines)

# Ordem de inserção respeitando FKs
TABLES = [
    "Categorias",
    "CartoesCredito",       # DbSet: CartoesCredito
    "SaldosContas",         # DbSet: SaldosContas
    "ReceitasRecorrentes",
    "HorasTrabalhadas",
    "ParcelasCartao",       # FK → CartoesCredito
    "Lancamentos",          # FK → todas as outras
]

def main():
    print(f"Conectando ao SQL Server...")
    try:
        conn = pyodbc.connect(SQL_SERVER_CONN)
    except Exception as e:
        print(f"\n❌ Falha ao conectar: {e}")
        print("\nVerifique:")
        print("  - A string de conexão SQL_SERVER_CONN está correta?")
        print("  - O driver ODBC está instalado? (pip install pyodbc)")
        print("  - O SQL Server está rodando?")
        return

    cursor = conn.cursor()
    output = []

    output.append("-- ============================================================")
    output.append("-- Migração SQL Server → PostgreSQL")
    output.append(f"-- Gerado em: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    output.append("-- Execute este arquivo no DBeaver conectado ao Supabase")
    output.append("-- ============================================================")
    output.append("")
    output.append("BEGIN;")
    output.append("")

    total_rows = 0

    for table in TABLES:
        print(f"  Lendo {table}...", end="")
        try:
            cols, rows = fetch_table(cursor, table)
            print(f" {len(rows)} registros")
            output.append(gen_inserts(table, cols, rows))
            total_rows += len(rows)
        except Exception as e:
            print(f" ERRO: {e}")
            output.append(f"-- ⚠ ERRO ao ler {table}: {e}\n")

    output.append("COMMIT;")
    output.append("")
    output.append(f"-- Total de registros exportados: {total_rows}")

    conn.close()

    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("\n".join(output))

    print(f"\n✅ Arquivo gerado: {OUTPUT_FILE}")
    print(f"   Total de registros: {total_rows}")
    print(f"\nPróximo passo:")
    print(f"  1. Abra o DBeaver conectado ao Supabase")
    print(f"  2. Abra o arquivo '{OUTPUT_FILE}'")
    print(f"  3. Execute (F5 ou Ctrl+Enter)")

if __name__ == "__main__":
    main()
