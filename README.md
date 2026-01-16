# Battleship

An ASP.NET MVC web application for playing Battleship. This is the final project of [@samosk](https://github.com/samosk) and [@marcusbillman](https://github.com/marcusbillman) in the course _Databases and Web Based Systems_ (5TF048) at Umeå University.

## Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [VS Code](https://code.visualstudio.com/) with the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## Setting up development environment

### 1. Clone the repository

```bash
git clone https://github.com/samosk/Battleship.git
cd Battleship
```

### 2. Create the PostgreSQL container

```bash
docker run -d \
  --name battleship-postgres \
  -e POSTGRES_USER=battleship \
  -e POSTGRES_PASSWORD=your_password_here \
  -e POSTGRES_DB=battleship \
  -p 5432:5432 \
  postgres:14.20
```

### 3. Set up the database

Install the EF Core command line tools and run a migration.

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### 4. Configure user secrets

Initialize user secrets and set the connection string:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Battleship" "Host=localhost;Port=5432;Database=battleship;Username=battleship;Password=your_password_here"
```

### 5. Run the application

**Option A: Command line**

```bash
dotnet watch run
```

**Option B: VS Code debugger**

Press `F5` or use the Run and Debug panel with the C# Dev Kit extension.
