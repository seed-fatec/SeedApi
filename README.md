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

2. Configure as variáveis de ambiente necessárias:

   **Linux/MacOS:**

   ```bash
   export CorsSettings__FrontEndUrl="http://localhost:5173"
   export CorsSettings__AdminFrontEndUrl="http://localhost:5174"
   export Admin__Email="admin@email.com"
   export Admin__Password="adminpassword"
   export MySqlSettings__ConnectionString="Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb"
   export MongoSettings__ConnectionString="mongodb://localhost:27017"
   export MongoSettings_DatabaseName="SeedApiDb"
   export JwtSettings__Issuer="your-issuer"
   export JwtSettings__Audience="your-audience"
   export JwtSettings__Secret="your-super-base32str-secret-key-here"
   export AzureSettings__BlobStorageConnectionString="DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net"
   ```

   **Windows (PowerShell):**

   ```powershell
   $env:CorsSettings__FrontEndUrl="http://localhost:5173"
   $env:CorsSettings__AdminFrontEndUrl="http://localhost:5174"
   $env:Admin__Email="admin@email.com"
   $env:Admin__Password="adminpassword"
   $env:MySqlSettings__ConnectionString="Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb"
   $env:MongoSettings__ConnectionString="mongodb://localhost:27017"
   $env:MongoSettings__DatabaseName="SeedApiDb"
   $env:JwtSettings__Issuer="your-issuer"
   $env:JwtSettings__Audience="your-audience"
   $env:JwtSettings__Secret="your-super-base32str-secret-key-here"
   $env:AzureSettings__BlobStorageConnectionString="DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net"
   ```

   **Windows (Command Prompt):**

   ```cmd
   set CorsSettings__FrontEndUrl=http://localhost:5173
   set CorsSettings__AdminFrontEndUrl=http://localhost:5174
   set Admin__Email=admin@email.com
   set Admin__Password=adminpassword
   set MySqlSettings__ConnectionString=Server=localhost;Port=3306;Uid=root;Pwd=secret;Database=SeedApiDb
   set MongoSettings__ConnectionString=mongodb://localhost:27017
   set MongoSettings__DatabaseName=SeedApiDb
   set JwtSettings__Issuer=your-issuer
   set JwtSettings__Audience=your-audience
   set JwtSettings__Secret=your-super-base32str-secret-key-here
   set AzureSettings__BlobStorageConnectionString=DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net
   ```

3. Instale a ferramenta `dotnet ef`:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Execute as migrações do Entity Framework para criar o banco de dados:

   ```bash
   dotnet ef database update --context ApplicationDbContext --project ./SeedApi/SeedApi.Infrastructure --startup-project ./SeedApi/SeedApi.API
   ```

5. Execute o projeto:
   ```bash
   dotnet run --project ./SeedApi/SeedApi.API
   ```

## Endpoints Principais 🔗

Para detalhes sobre os endpoints, consulte a documentação disponível na rota `/scalar`.
