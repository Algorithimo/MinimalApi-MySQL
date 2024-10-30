using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Infraestrutura.DB
{
    public class DBContexto : DbContext
    {
        public readonly IConfiguration _configuracaoAppSettings;
        public DBContexto (IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet <Administrador> Administradores { get; set; } = default!;
        public DbSet <Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador{ 
                    Id = 1,
                    Email = "administrador@teste.com", 
                    Senha = "123456",
                    Perfil = "Adm"
                 }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // String de Conexao com Banco MySql
            var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
            // Verificacao se String de Conexao esta Vazia
            if(!optionsBuilder.IsConfigured)
            {
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    // Auto deteccao da string de conexao (conectar com banco)
                    optionsBuilder.UseMySql(
                    stringConexao, 
                    ServerVersion.AutoDetect (stringConexao)
                    );
                }
            }
            
        }
    }
}