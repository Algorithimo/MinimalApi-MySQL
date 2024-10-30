using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.DB;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace minimal_api;

public class Startup
{
    private readonly string key;
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration.GetSection("Jwt:Key")?.Value ?? "";
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT desta maneira: Bearer {Seu token}"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        services.AddDbContext<DBContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion
            #region Administradores
            string GerarTokenJwt(Administrador administrador){
            if(string.IsNullOrEmpty(key)) return string.Empty;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>() 
            { 
                new Claim("Email", administrador.Email),
                new Claim("Perfil", administrador.Perfil),
                new Claim(ClaimTypes.Role, administrador.Perfil),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO logindto, IAdministradorServico administradorServico) => {
                var adm = administradorServico.Login(logindto);
                if (adm != null)
                {
                string token = GerarTokenJwt(adm);
                return Results.Ok(new AdministradorLogado{
                    Email = adm.Email,
                    Perfil = adm.Perfil,
                    Token = token
                });
                }
                
                else 
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrador");

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
                var validacao = new ErrosDeValidacao{
                Mensagens = new List<string>()
                };
                if(string.IsNullOrEmpty(administradorDTO.Email))
                {
                validacao.Mensagens.Add("Email nao pode ser vazio");
                }
                if(string.IsNullOrEmpty(administradorDTO.Senha))
                {
                validacao.Mensagens.Add("Senha nao pode ser vazio");
                }
                if(administradorDTO.Perfil == null)
                {
                validacao.Mensagens.Add("Perfil nao pode ser vazio");
                }
                
                var administrador = new Administrador{
                Email = administradorDTO.Email,
                Senha = administradorDTO.Senha,
                Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
                };

                administradorServico.Incluir(administrador);
                
                return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView{
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil});

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administrador");

            endpoints.MapGet("/admnistrador", ([FromQuery]int? pagina, IAdministradorServico administradorServico) => {
                var adms = new List<AdministradorModelView>();
                var admnistradores = administradorServico.Todos(pagina);

                foreach(var adm in admnistradores)
                {
                adms.Add(new AdministradorModelView{
                    Id = adm.Id,
                    Email = adm.Email,
                    Perfil = adm.Perfil
                });
                }


                return Results.Ok(admnistradores);

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administrador");


            #endregion
            #region Veiculos
            ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
            {
            var validacao = new ErrosDeValidacao
            {
                Mensagens = new List<string> ()
            };

                if(string.IsNullOrEmpty(veiculoDTO.Nome))
                {
                validacao.Mensagens.Add("O Nome nao pode ser vazio");
                }
            
                if(string.IsNullOrEmpty(veiculoDTO.Marca))
                {
                validacao.Mensagens.Add("O Marca nao pode ser vazio");
                }

                if(veiculoDTO.Ano > 2025)
                {
                validacao.Mensagens.Add("O Ano nao pode ser superior a 2024");
                }
            return validacao;
            
            }

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculodto, IVeiculoServico veiculoServico) => {
                var validacao = validaDTO(veiculodto);
                if(validacao.Mensagens.Count > 0)
                {
                return Results.BadRequest(validacao);
                }
                
                var veiculos = new Veiculo{
                Nome = veiculodto.Nome,
                Marca = veiculodto.Marca,
                Ano = veiculodto.Ano
                };

                veiculoServico.Inculuir(veiculos);

                return Results.Created($"/veiculo/{veiculos.Id}", veiculos);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veiculo");

            endpoints.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoServico veiculoServico) => {
                var veiculos = veiculoServico.Todos(pagina);


                return Results.Ok(veiculos);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veiculo");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute]int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);

                if(veiculo == null) return Results.NotFound();
            
                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veiculo");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute]int id, VeiculoDTO veiculoDTO,IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                var validacao = validaDTO(veiculoDTO);
                if(validacao.Mensagens.Count > 0)
                {
                return Results.BadRequest(validacao);
                }

                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculo");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute]int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);

                if(veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculo");

            #endregion    
        });
    }
}
