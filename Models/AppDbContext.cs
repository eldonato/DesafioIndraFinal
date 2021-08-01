using Microsoft.EntityFrameworkCore;

namespace DesafioIndraFinal.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ContaCorrente> ContaCorrentes { get; set; }
        public DbSet<ContaPoupanca> ContaPoupancas { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasData(
                new Cliente { Id = 1, Nome = "Leonardo Donato", Cpf = "123.456.789-10" });

        }


    }
}
