using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class RenumberDepartmentsIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Разовая нормализация: Id отделения приводится к его алиасу.
            // Эта миграция идёт ПЕРЕД DepartmentSettings, поэтому FK на Departments.Id
            // на момент выполнения ещё не существует — переписывание PK безопасно.
            // Данные отчётов привязаны по алиасу (depNumber), а он равен новому Id,
            // поэтому история сходится без правок.
            migrationBuilder.Sql(@"
-- защита: алиасы обязаны быть уникальны; при дубликатах исключение прервёт миграцию
CREATE UNIQUE INDEX ""UX_Departments_Allias_Guard"" ON ""Departments"" (""Allias"");
DROP INDEX ""UX_Departments_Allias_Guard"";

-- фаза 1: увести все Id в безопасный диапазон (защита от коллизии PRIMARY KEY)
UPDATE ""Departments"" SET ""Id"" = ""Id"" + 100000;

-- фаза 2: присвоить Id = Allias
UPDATE ""Departments"" SET ""Id"" = ""Allias"";

-- синхронизировать счётчик AUTOINCREMENT, иначе новые вставки займут занятые Id
INSERT INTO ""sqlite_sequence"" (""name"", ""seq"")
SELECT 'Departments', (SELECT MAX(""Id"") FROM ""Departments"")
WHERE NOT EXISTS (SELECT 1 FROM ""sqlite_sequence"" WHERE ""name"" = 'Departments');

UPDATE ""sqlite_sequence"" SET ""seq"" = (SELECT MAX(""Id"") FROM ""Departments"")
WHERE ""name"" = 'Departments';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Нормализация Id := Allias необратима: исходные суррогатные Id (с пропусками)
            // из данных не восстанавливаются. Для отката восстановите БД из резервной копии,
            // снятой до применения этой миграции.
            throw new NotSupportedException(
                "RenumberDepartmentsIds не поддерживает откат. Восстановите БД из резервной копии.");
        }
    }
}
