# SeedApi 🌱

## Tecnologias Utilizadas 🛠️

- **ASP.NET Core 9.0**
- **Entity Framework Core**
- **JWT (JSON Web Token)**
- **MySQL**
- **OpenAPI/Swagger**

## Configuração do Projeto 🚀

### Pré-requisitos

- .NET 9.0 SDK
- MySQL

### Passos para Rodar o Projeto

1. Clone o repositório:

   ```bash
   git clone https://github.com/seed-fatec/SeedApi.git
   cd SeedApi
   ```

2. Configure a string de conexão com o banco de dados e outras variáveis seguras:

   **Linux/MacOS:**

   ```bash
   export DatabaseSettings__ConnectionString="Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb"
   export JwtSettings__Secret="your-super-secret-key-here"
   export JwtSettings__Issuer="your-issuer"
   export JwtSettings__Audience="your-audience"
   ```

   **Windows (PowerShell):**

   ```powershell
   $env:DatabaseSettings__ConnectionString="Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb"
   $env:JwtSettings__Secret="your-super-secret-key-here"
   $env:JwtSettings__Issuer="your-issuer"
   $env:JwtSettings__Audience="your-audience"
   ```

   **Windows (Command Prompt):**

   ```cmd
   set DatabaseSettings__ConnectionString=Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb
   set JwtSettings__Issuer=your-issuer
   set JwtSettings__Audience=your-audience
   set JwtSettings__Secret=your-super-secret-key-here
   ```

3. Instale a ferramenta `dotnet ef`:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Execute as migrações do Entity Framework para criar o banco de dados:

   ```bash
   dotnet ef database update
   ```

5. Execute o projeto:
   ```bash
   dotnet run
   ```

## Endpoints Principais 🔗

Para detalhes sobre os endpoints, consulte a documentação disponível na rota `/scalar`.
