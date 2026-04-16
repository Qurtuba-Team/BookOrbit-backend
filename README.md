
## 🚦 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Quick Start
1. **Clone & Navigate**:
   ```bash
   git clone https://github.com/Qurtuba-Team/BookOrbit-backend.git
   cd BookOrbit/Code
   ```
2. **Launch Stack**:
   ```bash
   docker compose up --build
   ```
3. **Verify Health**: Visit `http://localhost:7240/health`. It should return `Healthy`.

---

## 📊 Service Registry & Credentials

| Service | URL | Credentials (User / Pass) |
| :--- | :--- | :--- |
| **API (HTTP)** | `http://localhost:7240` | - |
| **Grafana** | `http://localhost:3000` | `admin` / `sa123456` |
| **Seq** | `http://localhost:8081` | `admin` / `MyStrongPass123!` |
| **Prometheus** | `http://localhost:9090` | - |
| **Jaeger (UI)**| `http://localhost:16686`| - |
| **SQL Server** | `localhost, 1433` | `sa` / `MyStrongPass123!` |

### 🔑 Seeded Test Users
- **Admin**: `admin@bookorbit.com` / `Admin@123456`
- **Student**: `student1@std.mans.edu.eg` / `sa123456`

---

## 📝 Development Notes

### API Testing
- **Postman**: A collection is provided in `Tests/BookOrbit API.postman_collection.json`.

### Infrastructure
- **Observability**: The API exports telemetry via OTLP to the container stack. Use Grafana for dashboards and Jaeger for distributed tracing.
- **Database**: To reset the environment completely, use `docker compose down -v`.
- **Images**: A default student image is bundled at `BookOrbit.Api/uploads/Students/DefaultStudentImage.png`.

---