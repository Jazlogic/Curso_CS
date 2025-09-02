# 🚀 Clase 3: CI/CD con GitHub Actions

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 2: Containerización y Docker](../senior_8/clase_2_containerizacion_docker.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 4: Kubernetes Deployment](../senior_8/clase_4_kubernetes_deployment.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** pipelines de CI/CD con GitHub Actions
2. **Configurar** build y testing automático
3. **Desarrollar** deployment automático
4. **Aplicar** análisis de seguridad y calidad
5. **Optimizar** workflows de CI/CD

---

## 🔄 **Pipeline de Build y Test**

### **Workflow Principal de CI/CD**

```yaml
# .github/workflows/build-and-test.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_FILE: 'MusicalMatchingPlatform.sln'
  TEST_PROJECTS: 'tests/**/*.csproj'

jobs:
  build:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: Your_password123!
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 1433:1433

    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
    
    - name: Install dependencies
      run: |
        dotnet restore ${{ env.SOLUTION_FILE }}
        npm ci
    
    - name: Build solution
      run: dotnet build ${{ env.SOLUTION_FILE }} --configuration Release --no-restore
    
    - name: Run unit tests
      run: |
        dotnet test tests/MusicalMatching.UnitTests \
          --configuration Release \
          --no-build \
          --verbosity normal \
          --collect:"XPlat Code Coverage" \
          --results-directory ./coverage \
          --logger "trx;LogFileName=unit-tests.trx"
    
    - name: Run integration tests
      run: |
        dotnet test tests/MusicalMatching.IntegrationTests \
          --configuration Release \
          --no-build \
          --verbosity normal \
          --collect:"XPlat Code Coverage" \
          --results-directory ./coverage \
          --logger "trx;LogFileName=integration-tests.trx"
    
    - name: Run E2E tests
      run: |
        dotnet test tests/MusicalMatching.E2ETests \
          --configuration Release \
          --no-build \
          --verbosity normal \
          --logger "trx;LogFileName=e2e-tests.trx"
    
    - name: Upload test results
      uses: actions/upload-artifact@v4
      with:
        name: test-results
        path: |
          **/TestResults/*.trx
          **/coverage/
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/coverage.cobertura.xml
        flags: unittests
        name: codecov-umbrella
        fail_ci_if_error: false

  security-scan:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high --fail-on=high
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'MusicalMatching'
        path: '.'
        format: 'HTML'
        out: 'reports'
        args: >
          --failOnCVSS 7
          --enableRetired
          --suppression suppression.xml
    
    - name: Upload security report
      uses: actions/upload-artifact@v4
      with:
        name: security-report
        path: reports/

  quality-check:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Run SonarQube analysis
      uses: sonarqube-quality-gate-action@master
      env:
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
      with:
        scannerHome: ${{ github.workspace }}/.sonar/scanner
        args: >
          -Dsonar.projectKey=MusicalMatching
          -Dsonar.sources=src
          -Dsonar.tests=tests
          -Dsonar.cs.opencover.reportsPaths=coverage/coverage.opencover.xml
          -Dsonar.coverage.exclusions=**/*Test.cs,**/*Tests.cs,**/Program.cs,**/Startup.cs
    
    - name: Run CodeQL analysis
      uses: github/codeql-action/init@v2
      with:
        languages: csharp
    
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v2
```

---

## 🐳 **Pipeline de Docker Build**

### **Build y Push de Imágenes Docker**

```yaml
# .github/workflows/docker-build.yml
name: Docker Build and Push

on:
  workflow_run:
    workflows: ["Build and Test"]
    types:
      - completed
    branches: [main, develop]

jobs:
  docker-build:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ghcr.io
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ghcr.io/${{ github.repository }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        platforms: linux/amd64,linux/arm64
        build-args: |
          BUILDKIT_INLINE_CACHE=1
          DOTNET_VERSION=8.0
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: ghcr.io/${{ github.repository }}:latest
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
    
    - name: Run container security scan
      uses: anchore/scan-action@v3
      with:
        image: ghcr.io/${{ github.repository }}:latest
        fail-build: false
        severity-cutoff: high
```

---

## 🚀 **Pipeline de Deployment**

### **Deployment Automático a Producción**

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  workflow_run:
    workflows: ["Docker Build and Push"]
    types:
      - completed
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    environment: production
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'latest'
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ secrets.AWS_REGION }}
    
    - name: Update kubeconfig
      run: aws eks update-kubeconfig --name ${{ secrets.EKS_CLUSTER_NAME }} --region ${{ secrets.AWS_REGION }}
    
    - name: Deploy to Kubernetes
      run: |
        # Update image tag in deployment
        sed -i 's|ghcr.io/${{ github.repository }}:.*|ghcr.io/${{ github.repository }}:${{ github.sha }}|g' k8s/deployment.yml
        
        # Apply deployment
        kubectl apply -f k8s/namespace.yml
        kubectl apply -f k8s/deployment.yml
        kubectl apply -f k8s/service.yml
        kubectl apply -f k8s/ingress.yml
        
        # Wait for deployment to be ready
        kubectl rollout status deployment/musical-matching-api -n musical-matching --timeout=300s
    
    - name: Run smoke tests
      run: |
        # Wait for application to be ready
        sleep 30
        
        # Get service URL
        SERVICE_URL=$(kubectl get service musical-matching-api-service -n musical-matching -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
        
        # Run health check
        curl -f http://$SERVICE_URL/health || exit 1
        
        # Run basic API test
        curl -f http://$SERVICE_URL/api/health || exit 1
    
    - name: Notify deployment status
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK_URL }}
        text: |
          Deployment to Production ${{ job.status }}
          Commit: ${{ github.sha }}
          Branch: ${{ github.ref_name }}
          Author: ${{ github.actor }}

  rollback:
    runs-on: ubuntu-latest
    if: failure()
    needs: deploy
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'latest'
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ secrets.AWS_REGION }}
    
    - name: Update kubeconfig
      run: aws eks update-kubeconfig --name ${{ secrets.EKS_CLUSTER_NAME }} --region ${{ secrets.AWS_REGION }}
    
    - name: Rollback deployment
      run: |
        kubectl rollout undo deployment/musical-matching-api -n musical-matching
        kubectl rollout status deployment/musical-matching-api -n musical-matching --timeout=300s
    
    - name: Notify rollback
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK_URL }}
        text: |
          🚨 Rollback executed due to deployment failure
          Previous deployment: ${{ github.sha }}
          Status: Rolled back to previous version
```

---

## 🔒 **Pipeline de Seguridad**

### **Análisis de Seguridad Automático**

```yaml
# .github/workflows/security.yml
name: Security Analysis

on:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM
  workflow_dispatch:

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Run Snyk security scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=medium --fail-on=high
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'MusicalMatching'
        path: '.'
        format: 'HTML'
        out: 'reports'
        args: >
          --failOnCVSS 7
          --enableRetired
          --suppression suppression.xml
    
    - name: Run Bandit security linter
      uses: python-security/bandit-action@v1
      with:
        args: -r . -f json -o bandit-report.json
    
    - name: Run Semgrep
      uses: returntocorp/semgrep-action@v1
      with:
        config: >-
          p/security-audit
          p/secrets
          p/owasp-top-ten
        output-format: sarif
        output-file: semgrep-results.sarif
    
    - name: Upload security reports
      uses: actions/upload-artifact@v4
      with:
        name: security-reports
        path: |
          reports/
          bandit-report.json
          semgrep-results.sarif
    
    - name: Create security issue
      if: failure()
      uses: actions/github-script@v7
      with:
        script: |
          github.rest.issues.create({
            owner: context.repo.owner,
            repo: context.repo.repo,
            title: '🚨 Security vulnerabilities detected',
            body: `Security scan detected vulnerabilities in the codebase.
          
          **Scan Results:**
          - Snyk: ${{ steps.snyk.outputs.result }}
          - OWASP: ${{ steps.owasp.outputs.result }}
          - Bandit: ${{ steps.bandit.outputs.result }}
          - Semgrep: ${{ steps.semgrep.outputs.result }}
          
          Please review the security reports and fix the identified issues.`,
            labels: ['security', 'vulnerability', 'high-priority']
          })
```

---

## 📊 **Pipeline de Performance**

### **Testing de Performance Automático**

```yaml
# .github/workflows/performance.yml
name: Performance Testing

on:
  workflow_run:
    workflows: ["Deploy to Production"]
    types:
      - completed
    branches: [main]

jobs:
  performance-test:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
    
    - name: Install Artillery
      run: npm install -g artillery
    
    - name: Run load test
      run: |
        artillery run performance/load-test.yml \
          --output performance-results.json \
          --environment production
    
    - name: Run stress test
      run: |
        artillery run performance/stress-test.yml \
          --output stress-results.json \
          --environment production
    
    - name: Generate performance report
      run: |
        artillery report performance-results.json --output performance-report.html
        artillery report stress-results.json --output stress-report.html
    
    - name: Upload performance reports
      uses: actions/upload-artifact@v4
      with:
        name: performance-reports
        path: |
          performance-results.json
          stress-results.json
          performance-report.html
          stress-report.html
    
    - name: Check performance thresholds
      run: |
        # Parse results and check against thresholds
        NINETY_FIFTH_PERCENTILE=$(jq -r '.aggregate.latency.p95' performance-results.json)
        ERROR_RATE=$(jq -r '.aggregate.errors' performance-results.json)
        
        if (( $(echo "$NINETY_FIFTH_PERCENTILE > 1000" | bc -l) )); then
          echo "Performance threshold exceeded: P95 latency is ${NINETY_FIFTH_PERCENTILE}ms"
          exit 1
        fi
        
        if (( $(echo "$ERROR_RATE > 0.01" | bc -l) )); then
          echo "Error rate threshold exceeded: ${ERROR_RATE}%"
          exit 1
        fi
        
        echo "Performance tests passed"
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Pipeline de Build y Test**
```yaml
# Implementa:
# - Workflow de build automático
# - Testing unitario e integración
# - Análisis de cobertura de código
# - Validación de calidad
```

### **Ejercicio 2: Pipeline de Docker**
```yaml
# Crea:
# - Build automático de imágenes Docker
# - Push a registry
# - Escaneo de vulnerabilidades
# - Cache optimizado
```

### **Ejercicio 3: Pipeline de Deployment**
```yaml
# Implementa:
# - Deployment automático a Kubernetes
# - Smoke tests post-deployment
# - Rollback automático en fallos
# - Notificaciones de estado
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔄 Pipeline de Build y Test**: CI automático con testing completo
2. **🐳 Pipeline de Docker**: Build y push de imágenes con seguridad
3. **🚀 Pipeline de Deployment**: Deployment automático con rollback
4. **🔒 Pipeline de Seguridad**: Análisis automático de vulnerabilidades
5. **📊 Pipeline de Performance**: Testing de rendimiento automático

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Kubernetes Deployment**, implementando orquestación de contenedores y escalabilidad automática.

---

**¡Has completado la tercera clase del Módulo 15! 🚀🔄**

