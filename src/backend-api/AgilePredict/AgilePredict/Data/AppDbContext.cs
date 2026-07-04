using AgilePredict.Models;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Data
{
    public class AppDbContext : DbContext
    {
        // Construtor obrigatório para o EF Core receber as configurações de conexão do banco
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 2. Mapeamento das tabelas. O EF Core vai criar tabelas no banco com esses nomes
        public DbSet<Project> Projects { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }

        // 3. O método OnModelCreating é criado AQUI, dentro da sua classe de contexto
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Executa as configurações padrão do EF Core primeiro
            base.OnModelCreating(modelBuilder);

            // Aplica a nossa configuração de GUID Sequencial para o SQL Server
            modelBuilder.Entity<Project>()
                .Property(p => p.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Sprint>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        }
    }
}
