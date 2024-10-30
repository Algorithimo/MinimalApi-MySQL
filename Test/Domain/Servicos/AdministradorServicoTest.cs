using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.DB;

namespace Test.Domain.Servicos;

        [TestClass]
        public class AdministradorServicoTest
        {
            private DBContexto CriarContextoTeste()
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

                var builder = new ConfigurationBuilder()
                    .SetBasePath(path ?? Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                var configuration = builder.Build();

                return new DBContexto(configuration);
            }
            [TestMethod]
            public void TestarSalvarAdministrador()
            {
                // Arrange
                var context = CriarContextoTeste();
                context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");
                
                var adm = new Administrador();
                adm.Id = 1;
                adm.Email = "teste@teste.com";
                adm.Senha = "teste";
                adm.Perfil = "Adm";
                
                var administradorServico = new AdministradorServico(context);
                
                // Act
                administradorServico.Incluir(adm);

                // Assert
                Assert.AreEqual(1, administradorServico.Todos(1).Count());
            }


        }        
