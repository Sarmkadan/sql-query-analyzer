// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Production deployment scenarios and best practices for SQL Query Analyzer.

## Prerequisites

- Docker and Docker Compose (recommended)
- Kubernetes cluster (for K8s deployment)
- SQL Server/PostgreSQL instance
- Minimum 1 CPU, 512 MB RAM

## Docker Deployment (Recommended)

### Basic Docker Compose

```bash
docker-compose up -d
```

This starts:
- SQL Server 2022
- Analyzer application on port 5000

### Production Configuration

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "${DB_PASSWORD}"
    restart: always
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - analyzer-net
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${DB_PASSWORD}", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 3s
      retries: 10

  analyzer:
    build: .
    restart: always
    depends_on:
      sqlserver:
        condition: service_healthy
    environment:
      DB_SERVER: sqlserver
      DB_USER: sa
      DB_PASSWORD: "${DB_PASSWORD}"
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "5000:5000"
    volumes:
      - ./logs:/app/logs
      - ./reports:/app/reports
    networks:
      - analyzer-net
```

### Environment File (.env)

```bash
DB_PASSWORD=YourSecurePassword123!
ANALYZER_LOG_LEVEL=Information
ANALYZER_ENABLE_CACHE=true
```

## Kubernetes Deployment

### Namespace Creation

```bash
kubectl create namespace sql-analyzer
```

### Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sql-analyzer
  namespace: sql-analyzer
spec:
  replicas: 3
  selector:
    matchLabels:
      app: sql-analyzer
  template:
    metadata:
      labels:
        app: sql-analyzer
    spec:
      containers:
      - name: analyzer
        image: sql-query-analyzer:1.0.0
        ports:
        - containerPort: 5000
        env:
        - name: DB_SERVER
          valueFrom:
            configMapKeyRef:
              name: analyzer-config
              key: db-server
        - name: DB_USER
          valueFrom:
            secretKeyRef:
              name: analyzer-secrets
              key: db-user
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: analyzer-secrets
              key: db-password
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
```

### Service Configuration

```yaml
apiVersion: v1
kind: Service
metadata:
  name: sql-analyzer-service
  namespace: sql-analyzer
spec:
  type: LoadBalancer
  selector:
    app: sql-analyzer
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
```

### ConfigMap and Secrets

```bash
kubectl create configmap analyzer-config \
  --from-literal=db-server=sql-server.default.svc.cluster.local \
  -n sql-analyzer

kubectl create secret generic analyzer-secrets \
  --from-literal=db-user=sa \
  --from-literal=db-password=YourPassword123! \
  -n sql-analyzer
```

## Azure App Service Deployment

### Using Azure CLI

```bash
# Create resource group
az group create --name analyzer-rg --location eastus

# Create App Service Plan
az appservice plan create \
  --name analyzer-plan \
  --resource-group analyzer-rg \
  --sku B2 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group analyzer-rg \
  --plan analyzer-plan \
  --name sql-query-analyzer \
  --runtime "DOTNET|10.0"

# Configure deployment
az webapp deployment source config-zip \
  --resource-group analyzer-rg \
  --name sql-query-analyzer \
  --src publish.zip
```

### Environment Variables

Set in Azure Portal > Configuration:

```
DB_SERVER=your-sql-server.database.windows.net
DB_USER=admin@server
DB_PASSWORD=YourSecurePassword123!
ASPNETCORE_ENVIRONMENT=Production
```

## AWS Elastic Container Service (ECS)

### Task Definition

```json
{
  "family": "sql-query-analyzer",
  "containerDefinitions": [
    {
      "name": "analyzer",
      "image": "YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/sql-query-analyzer:latest",
      "memory": 512,
      "cpu": 256,
      "portMappings": [
        {
          "containerPort": 5000,
          "hostPort": 5000,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "DB_SERVER",
          "value": "your-rds-endpoint.rds.amazonaws.com"
        },
        {
          "name": "DB_USER",
          "value": "admin"
        }
      ],
      "secrets": [
        {
          "name": "DB_PASSWORD",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT:secret:db-password"
        }
      ]
    }
  ]
}
```

### ECS Service Deployment

```bash
aws ecs create-service \
  --cluster analyzer-cluster \
  --service-name analyzer-service \
  --task-definition sql-query-analyzer \
  --desired-count 3 \
  --launch-type FARGATE
```

## GCP Cloud Run Deployment

### Using Cloud Build

```bash
# Build and push to Container Registry
gcloud builds submit --tag gcr.io/PROJECT_ID/sql-query-analyzer

# Deploy to Cloud Run
gcloud run deploy sql-query-analyzer \
  --image gcr.io/PROJECT_ID/sql-query-analyzer \
  --memory 512Mi \
  --cpu 1 \
  --max-instances 10 \
  --set-env-vars DB_SERVER=sql.instance.connection,DB_USER=admin \
  --set-cloudsql-instances PROJECT_ID:us-central1:sql-instance
```

## SSL/TLS Configuration

### With Nginx Reverse Proxy

```nginx
upstream analyzer {
    server analyzer:5000;
}

server {
    listen 443 ssl http2;
    server_name analyzer.example.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location / {
        proxy_pass http://analyzer;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Docker Compose with Nginx

```yaml
services:
  nginx:
    image: nginx:latest
    ports:
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - analyzer
```

## Monitoring & Logging

### Prometheus Metrics

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'sql-analyzer'
    static_configs:
      - targets: ['localhost:5000']
```

### Log Aggregation (ELK Stack)

```yaml
filebeat:
  inputs:
    - type: log
      enabled: true
      paths:
        - /app/logs/*.log
      
  output.elasticsearch:
    hosts: ["elasticsearch:9200"]
```

## Database Backup Strategy

### Automated Backups

SQL Server in Docker:

```bash
# Weekly backup
docker exec sql-query-analyzer-sqlserver \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
  -P 'YourPassword123!' \
  -Q "BACKUP DATABASE [QueryAnalyzer] TO DISK = '/var/opt/mssql/backup/weekly.bak'"
```

### Backup Script

```bash
#!/bin/bash
BACKUP_DIR="/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

docker exec sql-query-analyzer-sqlserver \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$DB_PASSWORD" \
  -Q "BACKUP DATABASE [QueryAnalyzer] TO DISK = '/var/opt/mssql/backup/db_$TIMESTAMP.bak'"

# Keep backups for 30 days
find $BACKUP_DIR -name "db_*.bak" -mtime +30 -delete
```

## Performance Optimization

### Caching Configuration

```bash
ANALYZER_ENABLE_CACHE=true
ANALYZER_CACHE_TTL=3600
ANALYZER_MAX_CACHE_SIZE=10000
```

### Database Connection Pooling

```
Max Pool Size=100
Min Pool Size=10
Connection Lifetime=300
```

### Resource Limits

```yaml
resources:
  requests:
    memory: "512Mi"
    cpu: "500m"
  limits:
    memory: "2Gi"
    cpu: "2000m"
```

## Health Checks & Readiness

### Liveness Probe

```bash
curl http://localhost:5000/health
# Returns: 200 OK if healthy
```

### Readiness Probe

```bash
curl http://localhost:5000/ready
# Returns: 200 OK if database connected
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker logs sql-query-analyzer-app

# Check environment variables
docker inspect sql-query-analyzer-app | grep -A 20 Env
```

### Database Connection Issues

```bash
# Test connection from container
docker exec sql-query-analyzer-app \
  dotnet tool install -g dotnet-sql-cli

# Check connectivity
docker exec sqlserver \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'password' -Q "SELECT 1"
```

### Memory Issues

```bash
# Increase Docker memory
docker update --memory 2g sql-query-analyzer-app

# Monitor usage
docker stats sql-query-analyzer-app
```

---

**Deployment Version**: 1.0  
**Last Updated**: 2026-05-04
