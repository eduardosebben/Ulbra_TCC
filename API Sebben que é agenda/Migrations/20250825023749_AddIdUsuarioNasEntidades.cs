using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Sebben_que_é_agenda.Migrations
{
    /// <inheritdoc />
    public partial class AddIdUsuarioNasEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Adiciona as colunas como NULLABLE (evita falha com dados existentes)
            migrationBuilder.AddColumn<int>(
                name: "idUsuario",
                table: "Pacientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "idUsuario",
                table: "Medicos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "idUsuario",
                table: "Consultas",
                type: "int",
                nullable: true);

            // 2) Backfill: garante que não há nulos/órfãos antes de travar NOT NULL + FK
            migrationBuilder.Sql(@"
                DECLARE @adminId INT;

                SELECT TOP 1 @adminId = Id FROM dbo.Usuarios ORDER BY Id;

                IF @adminId IS NULL
                BEGIN
                    -- Crie um usuário mínimo (ajuste conforme seu schema real)
                    INSERT INTO dbo.Usuarios (Nome, Email, SenhaHash)
                    VALUES ('Admin', 'admin@local', 'hash-placeholder');
                    SET @adminId = SCOPE_IDENTITY();
                END

                -- Pacientes
                UPDATE p SET p.idUsuario = ISNULL(p.idUsuario, @adminId)
                FROM dbo.Pacientes p
                LEFT JOIN dbo.Usuarios u ON u.Id = p.idUsuario
                WHERE p.idUsuario IS NULL OR u.Id IS NULL;

                -- Medicos
                UPDATE m SET m.idUsuario = ISNULL(m.idUsuario, @adminId)
                FROM dbo.Medicos m
                LEFT JOIN dbo.Usuarios u ON u.Id = m.idUsuario
                WHERE m.idUsuario IS NULL OR u.Id IS NULL;

                -- Consultas
                UPDATE c SET c.idUsuario = ISNULL(c.idUsuario, @adminId)
                FROM dbo.Consultas c
                LEFT JOIN dbo.Usuarios u ON u.Id = c.idUsuario
                WHERE c.idUsuario IS NULL OR u.Id IS NULL;
            ");

            // 3) Torna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "idUsuario",
                table: "Pacientes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "idUsuario",
                table: "Medicos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "idUsuario",
                table: "Consultas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 4) Índices
            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_idUsuario",
                table: "Pacientes",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_idUsuario",
                table: "Medicos",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_idUsuario",
                table: "Consultas",
                column: "idUsuario");

            // 5) FKs (Restrict)
            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Usuarios_idUsuario",
                table: "Consultas",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicos_Usuarios_idUsuario",
                table: "Medicos",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Usuarios_idUsuario",
                table: "Pacientes",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Derruba FKs
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Usuarios_idUsuario",
                table: "Consultas");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicos_Usuarios_idUsuario",
                table: "Medicos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Usuarios_idUsuario",
                table: "Pacientes");

            // Derruba índices
            migrationBuilder.DropIndex(
                name: "IX_Pacientes_idUsuario",
                table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Medicos_idUsuario",
                table: "Medicos");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_idUsuario",
                table: "Consultas");

            // Remove colunas
            migrationBuilder.DropColumn(
                name: "idUsuario",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "idUsuario",
                table: "Medicos");

            migrationBuilder.DropColumn(
                name: "idUsuario",
                table: "Consultas");
        }
    }
}
