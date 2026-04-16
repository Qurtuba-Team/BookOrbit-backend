
## 🚦 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Quick Start
1. **Clone**:
   ```bash
   git clone https://github.com/Qurtuba-Team/BookOrbit-backend.git
   ```
2. **Navigate**
   ```bash
   cd BookOrbit-backend/Code
   ```
3. **Launch Stack**:
   ```bash
   docker compose up --build
   ```
4. **Verify Health**: Visit `http://localhost:7240/health`. It should return `Healthy`.

---

## 📊 Service Registry & Credentials

| Service | URL | Credentials (User / Pass) |
| :--- | :--- | :--- |
| **API (HTTP)** | `http://localhost:7240` | - |
| **Grafana** | `http://localhost:3000` | From .env File |
| **Seq** | `http://localhost:8081` | From .env File |
| **Prometheus** | `http://localhost:9090` | - |
| **Jaeger (UI)**| `http://localhost:16686`| - |
| **SQL Server** | `localhost, 1433` | From .env File |

### 🔑 Seeded Test Users
- **Admin**: `admin@bookorbit.com` / `Admin@123456`
- **Student**: `student1@std.mans.edu.eg` / `sa123456`

---

## 📝 Development Notes

### API Testing
- **Postman**: A collection is provided in `Tests/BookOrbit API.postman_collection.json`.

### Infrastructure
- **Observability**: The API exports telemetry via OTLP to the container stack. Use Grafana for dashboards and Jaeger for distributed tracing.

### .env File Template
```
JWT_KEY=super-secret-key
EMAIL=youremail@gmail.com
EMAIL_PASSWORD=super-secret-password

SA_PASSWORD=VeryStrongPass123!

SEQ_ADMIN_PASSWORD=AnotherStrongPass
GRAFANA_ADMIN_PASSWORD=StrongGrafanaPass

DB_CONNECTION=Server=sqlserver,1433;Database=BookOrbitDb;User=sa;Password=VeryStrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=True
```

- **Database**: To reset the environment completely, use `docker compose down -v`.
- **Images**: A default student image is bundled at `BookOrbit.Api/uploads/Students/DefaultStudentImage.png`.

---
