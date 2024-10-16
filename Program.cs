using MinimalApi.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO logindto) => {
    if (logindto.Email == "adm@teste.com" && logindto.Senha == "123456")
      return Results.Ok("Login com sucesso!");
    else 
        return Results.Unauthorized();
});



app.Run();

