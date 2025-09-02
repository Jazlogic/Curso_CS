# 🔒 Clase 7: Security en CI/CD

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 6: Monitoring y Observabilidad](../expert_1/clase_6_monitoring_observabilidad.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 8: Deployment Strategies](../expert_1/clase_8_deployment_strategies.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** security scanning en pipelines
2. **Configurar** dependency vulnerability management
3. **Establecer** container security scanning
4. **Gestionar** secrets management y rotation
5. **Optimizar** security automation

---

## 🔍 **Security Scanning en Pipelines**

### **GitHub Actions Security Workflow**

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 2 * * 1' # Weekly on Monday

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Check for vulnerable packages
        run: |
          dotnet list package --vulnerable --include-transitive
          if [ $? -ne 0 ]; then
            echo "Vulnerable packages found!"
            exit 1
          fi

      - name: Run Snyk to check for vulnerabilities
        uses: snyk/actions/dotnet@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --severity-threshold=high

  code-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Run CodeQL Analysis
        uses: github/codeql-action/init@v2
        with:
          languages: csharp

      - name: Autobuild
        uses: github/codeql-action/autobuild@v2

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v2

  container-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Build Docker image
        run: docker build -t mussikon:test .

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'mussikon:test'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'
```

### **SonarQube Integration**

```yaml
# .github/workflows/sonarqube.yml
name: SonarQube Analysis

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Cache SonarQube packages
        uses: actions/cache@v3
        with:
          path: ~/.sonar/cache
          key: ${{ runner.os }}-sonar
          restore-keys: ${{ runner.os }}-sonar

      - name: Cache SonarQube scanner
        id: cache-sonar-scanner
        uses: actions/cache@v3
        with:
          path: ./.sonar/scanner
          key: ${{ runner.os }}-sonar-scanner
          restore-keys: ${{ runner.os }}-sonar-scanner

      - name: Install SonarQube Scanner
        if: steps.cache-sonar-scanner.outputs.cache-hit != 'true'
        run: |
          dotnet tool install dotnet-sonarscanner --tool-path ./.sonar/scanner

      - name: Build and analyze
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: |
          ./.sonar/scanner/dotnet-sonarscanner begin \
            /k:"mussikon" \
            /o:"mussikon-org" \
            /d:sonar.token="${SONAR_TOKEN}" \
            /d:sonar.host.url="https://sonarcloud.io" \
            /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
            /d:sonar.exclusions="**/bin/**,**/obj/**,**/Migrations/**"
          
          dotnet build
          dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
          
          ./.sonar/scanner/dotnet-sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
```

---

## 🔐 **Dependency Vulnerability Management**

### **Dependency Scanning Script**

```bash
#!/bin/bash
# scripts/security-scan.sh

set -e

echo "🔒 Starting security scan..."

# Check for vulnerable packages
echo "🔍 Checking for vulnerable packages..."
dotnet list package --vulnerable --include-transitive
if [ $? -ne 0 ]; then
    echo "❌ Vulnerable packages found!"
    exit 1
fi

# Run Snyk scan
echo "🔍 Running Snyk scan..."
if [ -n "$SNYK_TOKEN" ]; then
    dotnet tool install --global snyk
    snyk test --severity-threshold=high
else
    echo "⚠️ SNYK_TOKEN not set, skipping Snyk scan"
fi

# Run OWASP Dependency Check
echo "🔍 Running OWASP Dependency Check..."
if command -v dependency-check &> /dev/null; then
    dependency-check --project "MussikOn" --scan . --format JSON --out ./security-reports/
else
    echo "⚠️ OWASP Dependency Check not installed"
fi

echo "✅ Security scan completed!"
```

### **Automated Dependency Updates**

```yaml
# .github/workflows/dependency-update.yml
name: Dependency Update

on:
  schedule:
    - cron: '0 0 * * 1' # Weekly on Monday
  workflow_dispatch:

jobs:
  update-dependencies:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Update dependencies
        run: |
          dotnet add package Microsoft.EntityFrameworkCore --version latest
          dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version latest
          dotnet add package Serilog.AspNetCore --version latest

      - name: Run security scan
        run: |
          dotnet list package --vulnerable --include-transitive
          if [ $? -ne 0 ]; then
            echo "Vulnerable packages found after update!"
            exit 1
          fi

      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: 'chore: update dependencies'
          title: 'Automated dependency update'
          body: |
            This PR contains automated dependency updates.
            
            ## Changes
            - Updated Microsoft.EntityFrameworkCore to latest version
            - Updated Microsoft.AspNetCore.Authentication.JwtBearer to latest version
            - Updated Serilog.AspNetCore to latest version
            
            ## Security
            - [ ] All security scans pass
            - [ ] No vulnerable packages detected
          branch: automated-dependency-update
          delete-branch: true
```

---

## 🐳 **Container Security Scanning**

### **Multi-stage Security Scanning**

```dockerfile
# Dockerfile.security
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files
COPY ["src/MusicalMatching.API/MusicalMatching.API.csproj", "src/MusicalMatching.API/"]
COPY ["src/MusicalMatching.Application/MusicalMatching.Application.csproj", "src/MusicalMatching.Application/"]
COPY ["src/MusicalMatching.Domain/MusicalMatching.Domain.csproj", "src/MusicalMatching.Domain/"]
COPY ["src/MusicalMatching.Infrastructure/MusicalMatching.Infrastructure.csproj", "src/MusicalMatching.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/MusicalMatching.API/MusicalMatching.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/MusicalMatching.API/MusicalMatching.API.csproj" -c Release -o /app/build

# Security scan stage
FROM build AS security-scan
RUN apk add --no-cache curl
RUN curl -sSfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh -s -- -b /usr/local/bin
RUN trivy fs --exit-code 1 --severity HIGH,CRITICAL /src

# Test stage
FROM build AS test
RUN dotnet test --no-build -c Release --logger trx --results-directory /testresults

# Publish stage
FROM build AS publish
RUN dotnet publish "src/MusicalMatching.API/MusicalMatching.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app
USER appuser

# Expose port
EXPOSE 8080

# Start application
ENTRYPOINT ["dotnet", "MusicalMatching.API.dll"]
```

### **Container Security Scanning Workflow**

```yaml
# .github/workflows/container-security.yml
name: Container Security

on:
  push:
    branches: [main]
    tags: ['v*']
  pull_request:
    branches: [main]

jobs:
  container-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Build Docker image
        uses: docker/build-push-action@v4
        with:
          context: .
          file: ./src/MusicalMatching.API/Dockerfile
          push: false
          tags: mussikon:test
          cache-from: type=gha
          cache-to: type=gha,mode=max

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'mussikon:test'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'

      - name: Run Docker Bench Security
        uses: docker/docker-bench-security-action@master
        with:
          image: mussikon:test

      - name: Run Hadolint Dockerfile analysis
        uses: hadolint/hadolint-action@v3.1.0
        with:
          dockerfile: ./src/MusicalMatching.API/Dockerfile
          format: sarif
          output-file: hadolint-results.sarif

      - name: Upload Hadolint scan results
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'hadolint-results.sarif'
```

---

## 🔑 **Secrets Management y Rotation**

### **Azure Key Vault Integration**

```yaml
# .github/workflows/secrets-rotation.yml
name: Secrets Rotation

on:
  schedule:
    - cron: '0 0 1 * *' # Monthly on the 1st
  workflow_dispatch:

jobs:
  rotate-secrets:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Rotate JWT Secret
        run: |
          # Generate new JWT secret
          NEW_JWT_SECRET=$(openssl rand -base64 32)
          
          # Update in Key Vault
          az keyvault secret set \
            --vault-name "mussikon-keyvault" \
            --name "jwt-secret" \
            --value "$NEW_JWT_SECRET"
          
          # Update in GitHub Secrets
          gh secret set JWT_SECRET --body "$NEW_JWT_SECRET"

      - name: Rotate Database Password
        run: |
          # Generate new database password
          NEW_DB_PASSWORD=$(openssl rand -base64 32)
          
          # Update in Key Vault
          az keyvault secret set \
            --vault-name "mussikon-keyvault" \
            --name "database-password" \
            --value "$NEW_DB_PASSWORD"
          
          # Update in GitHub Secrets
          gh secret set DATABASE_PASSWORD --body "$NEW_DB_PASSWORD"

      - name: Notify Team
        uses: 8398a7/action-slack@v3
        with:
          status: success
          channel: '#security'
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}
          fields: repo,message,commit,author,action,eventName,ref,workflow
```

### **Secrets Management Script**

```bash
#!/bin/bash
# scripts/secrets-management.sh

set -e

echo "🔑 Managing secrets..."

# Function to generate secure password
generate_password() {
    openssl rand -base64 32
}

# Function to update secret in Azure Key Vault
update_keyvault_secret() {
    local secret_name=$1
    local secret_value=$2
    
    az keyvault secret set \
        --vault-name "mussikon-keyvault" \
        --name "$secret_name" \
        --value "$secret_value"
}

# Function to update secret in GitHub
update_github_secret() {
    local secret_name=$1
    local secret_value=$2
    
    gh secret set "$secret_name" --body "$secret_value"
}

# Rotate JWT secret
echo "🔄 Rotating JWT secret..."
NEW_JWT_SECRET=$(generate_password)
update_keyvault_secret "jwt-secret" "$NEW_JWT_SECRET"
update_github_secret "JWT_SECRET" "$NEW_JWT_SECRET"

# Rotate database password
echo "🔄 Rotating database password..."
NEW_DB_PASSWORD=$(generate_password)
update_keyvault_secret "database-password" "$NEW_DB_PASSWORD"
update_github_secret "DATABASE_PASSWORD" "$NEW_DB_PASSWORD"

# Rotate Redis password
echo "🔄 Rotating Redis password..."
NEW_REDIS_PASSWORD=$(generate_password)
update_keyvault_secret "redis-password" "$NEW_REDIS_PASSWORD"
update_github_secret "REDIS_PASSWORD" "$NEW_REDIS_PASSWORD"

echo "✅ Secrets rotation completed!"
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Security Scanning**
```yaml
# Configura un pipeline completo de security scanning
# con dependency scanning, code scanning y container scanning
```

### **Ejercicio 2: Secrets Management**
```bash
# Implementa un sistema de rotación automática
# de secrets y credenciales
```

### **Ejercicio 3: Container Security**
```dockerfile
# Crea un Dockerfile seguro con
# security scanning integrado
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔍 Security Scanning**: Escaneo de seguridad en pipelines
2. **🔐 Dependency Management**: Gestión de vulnerabilidades
3. **🐳 Container Security**: Seguridad de contenedores
4. **🔑 Secrets Management**: Gestión de secretos
5. **🔄 Secrets Rotation**: Rotación automática
6. **🚨 Security Automation**: Automatización de seguridad

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Deployment Strategies**, implementando estrategias de despliegue avanzadas.

---

**¡Has completado la séptima clase del Expert Level 1! 🔒🎯**
