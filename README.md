# MinimalApi-MySQL

## Descrição

O **MinimalApi-MySQL** é uma aplicação .NET que implementa uma API mínima utilizando MySQL como banco de dados. Este projeto tem como objetivo fornecer uma estrutura simples e eficiente para a criação de APIs RESTful com autenticação JWT e operações CRUD.

## Tecnologias Utilizadas

- **.NET 8**
- **ASP.NET Core**
- **Entity Framework Core**
- **MySQL**
- **Swagger**
- **JWT Authentication**
- **MSTest para testes automatizados**

## Funcionalidades

- Autenticação de usuários com JWT.
- CRUD para gerenciar administradores e veículos.
- Documentação da API com Swagger.
- Validação de dados de entrada.

## Como Executar o Projeto

### Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server

### Configuração do Banco de Dados

1. Crie um banco de dados no MySQL.
2. Atualize a string de conexão no arquivo `appsettings.json`:
   ```json
   "ConnectionStrings": {
       "mysql": "server=localhost;database=seu_banco_de_dados;user=seu_usuario;password=sua_senha;"
   }

## Como Executar o Projeto
### Execução
1. Restaure as dependências:
   ```bash
   dotnet restore
2. Execute a aplicação:
    ```bash
    dotnet run --project Api
3. Acesse a API em http://localhost:5137 e a documentação do Swagger em (http://localhost:5137/swagger/index.html)

### Testes
Para executar os testes, utilize o seguinte comando:
   ```bash
   dotnet test --project Test
