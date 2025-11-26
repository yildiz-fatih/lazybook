# Lazybook

A social networking app built with ASP.NET Core, PostgreSQL and Vite

---

## Running the Project

### 1. Database
```bash
docker compose up -d    # Start database at port 5432
```

### 2. Backend Server
```bash
cd backend/Lazybook.Api
dotnet restore              # Install dependencies
dotnet ef database update   # Apply migrations to create tables
dotnet run                  # Start server at http://localhost:5174
```

### 3. Frontend Client
```bash
cd client
npm install     # Install dependencies
npm run dev     # Start dev server at http://localhost:5173
```

---
