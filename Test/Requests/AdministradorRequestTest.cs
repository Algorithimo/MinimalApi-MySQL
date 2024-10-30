using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class AdministradorRequestTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);  
    }
    
        [TestMethod]
        public async Task TestarGetSetPropriedades()
        {
            // Arrange
            var loginDTO = new LoginDTO{
                Email = "adm@teste.com",
                Senha = "adm"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");
            
            // Act
            var response = await Setup.client.PostAsync("/administradores/login", content);
            
            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);


            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado.Token);

            Console.WriteLine(admLogado?.Token);
        }     
}
