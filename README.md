# Lazybook

A social networking app built with ASP.NET Core, PostgreSQL, Redis and Vite

---

## Running the Project

### 1. Database
```bash
docker compose up -d    # Start Postgres (5432) + Redis (6379)
```

### 2. Configuration
```bash
cd backend/Lazybook.Api
touch .env
```

Example `.env` content:
```ini
# Authentication
JWT_SECRET=super_secret_key

# Database
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=lazybook_dev;Username=postgres;Password=postgres"

# AWS S3 Storage
AWS__Region=eu-north-1
AWS__BucketName=your-bucket-name
AWS__AccessKey=your_access_key
AWS__SecretKey=your_secret_key
```

### 3. Backend Server
```bash
cd backend/Lazybook.Api
dotnet restore              # Install dependencies
dotnet ef database update   # Apply migrations to create tables
dotnet run                  # Start server at http://localhost:5174
```

### 4. Frontend Client
```bash
cd client
npm install     # Install dependencies
npm run dev     # Start dev server at http://localhost:5173
```

---

## Technical Deep Dive

### Caching

**Problem**

`GET /api/feeds/explore` is a shared (non-personalized), read-heavy endpoint. Every request runs the same query which creates unnecessary load on the database.

**Solution**

I implemented Redis caching for the Explore feed using a **cache-aside pattern**:

1. **Cache Check**: The backend first checks Redis for the data
2. **Cache Hit**: If found, return the cached response immediately (bypassing the database)
3. **Cache Miss**: If not found, query the database, store the result in Redis, then return it

The database now typically executes ~1 query per TTL window (30 seconds), while ~999 requests are served from Redis. Database reads scale with cache refresh frequency, not request volume.

**Tradeoff**

**Eventual consistency** is used to achieve **high availability**: the Explore feed data can be "out of date" for up to the TTL window (30s) but this significantly reduces the load on the database.

---

### Object Storage

**Problem**

Where to store binary files?

- **Application server**: Limited disk space, files are lost on server crashes and multiple server instances can't share the same local filesystem without a complex setup. Makes the application "stateful".
- **Database**: Slows down queries and wastes database resources on non-relational data.

**Solution**

I switched from local file storage to AWS S3 object storage. S3 provides storage that scales automatically. The backend uploads files directly to S3 and only stores the URL in the database. This keeps the application stateless.

---
