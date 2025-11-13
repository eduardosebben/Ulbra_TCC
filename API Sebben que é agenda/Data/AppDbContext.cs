using Microsoft.EntityFrameworkCore;
using API_Sebben_que_é_agenda.Models;

namespace API_Sebben_que_e_agenda.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Especialidade> Especialidades { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Relacionamentos já existentes =====
            modelBuilder.Entity<Especialidade>()
                .HasMany(e => e.Medicos)
                .WithOne(m => m.Especialidade)
                .HasForeignKey(m => m.EspecialidadeId)
                .OnDelete(DeleteBehavior.Cascade); // como na sua primeira migration

            modelBuilder.Entity<Medico>()
                .HasMany(m => m.Consultas)
                .WithOne(c => c.Medico)
                .HasForeignKey(c => c.MedicoId)
                .OnDelete(DeleteBehavior.Cascade); // como na sua primeira migration

            modelBuilder.Entity<Paciente>()
                .HasMany(p => p.Consultas)
                .WithOne(c => c.Paciente)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Cascade); // como na sua primeira migration

            // ===== NOVO: mapeia idUsuario/usuario nas três entidades =====
            // Medico -> Usuarios(Id)
            modelBuilder.Entity<Medico>()
                .Property(m => m.idUsuario)               // garante o nome da coluna exatamente "idUsuario"
                .HasColumnName("idUsuario")
                .IsRequired();

            modelBuilder.Entity<Medico>()
                .HasOne(m => m.usuario)                   // navigation 'usuario'
                .WithMany()                               // se tiver Usuario.Medicos, troque para .WithMany(u => u.Medicos)
                .HasForeignKey(m => m.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Medico>()
                .HasIndex(m => m.idUsuario)
                .HasDatabaseName("IX_Medicos_idUsuario");

            // Paciente -> Usuarios(Id)
            modelBuilder.Entity<Paciente>()
                .Property(p => p.idUsuario)
                .HasColumnName("idUsuario")
                .IsRequired();

            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.usuario)
                .WithMany()                               // se tiver Usuario.Pacientes, troque para .WithMany(u => u.Pacientes)
                .HasForeignKey(p => p.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Paciente>()
                .HasIndex(p => p.idUsuario)
                .HasDatabaseName("IX_Pacientes_idUsuario");

            // Consulta -> Usuarios(Id)
            modelBuilder.Entity<Consulta>()
                .Property(c => c.idUsuario)
                .HasColumnName("idUsuario")
                .IsRequired();

            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.usuario)
                .WithMany()                               // se tiver Usuario.Consultas, troque para .WithMany(u => u.Consultas)
                .HasForeignKey(c => c.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consulta>()
                .HasIndex(c => c.idUsuario)
                .HasDatabaseName("IX_Consultas_idUsuario");
        }
    }
}
