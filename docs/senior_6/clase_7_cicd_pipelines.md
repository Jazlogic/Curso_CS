# 🚀 Clase 7: CI/CD y Pipelines

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 6: Orquestación con Kubernetes](clase_6_orquestacion_kubernetes.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 8: Monitoreo y Observabilidad](clase_8_monitoreo_observabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar GitHub Actions para .NET
- Configurar Azure DevOps pipelines
- Implementar testing automatizado en CI/CD
- Configurar deployment automatizado

---

## 📚 Contenido Teórico

### 7.1 GitHub Actions para .NET

#### Pipeline Básico de CI/CD

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '8.0.x'
  DOCKER_REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # Build y Test
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --configuration Release --collect:"XPlat Code Coverage"
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./**/coverage.cobertura.xml
        flags: unittests
        name: codecov-umbrella
        
    - name: Run security scan
      run: dotnet list package --vulnerable
      
    - name: Run code analysis
      run: |
        dotnet tool install --global dotnet-format
        dotnet format --verify-no-changes
        
  # Build Docker Image
  build-docker:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.DOCKER_REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
        
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}
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
        
  # Deploy to Staging
  deploy-staging:
    needs: build-docker
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - name: Deploy to staging
      run: |
        echo "Deploying to staging environment..."
        # Aquí irían los comandos para desplegar en staging
        
  # Deploy to Production
  deploy-production:
    needs: build-docker
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to production
      run: |
        echo "Deploying to production environment..."
        # Aquí irían los comandos para desplegar en producción
```

#### Pipeline Avanzado con Testing

```yaml
# .github/workflows/advanced-ci.yml
name: Advanced CI Pipeline

on:
  push:
    branches: [ main, develop, feature/* ]
  pull_request:
    branches: [ main, develop ]

jobs:
  # Code Quality
  code-quality:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install SonarCloud
      uses: sonarqube-quality-gate-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        
    - name: Run SonarCloud analysis
      run: |
        dotnet tool install --global dotnet-sonarscanner
        dotnet sonarscanner begin /k:"myapp" /d:sonar.organization="myorg" /d:sonar.host.url="https://sonarcloud.io"
        dotnet build --no-restore
        dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
        
  # Security Scanning
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'MyApp'
        path: '.'
        format: 'HTML'
        out: 'reports'
        
    - name: Upload security report
      uses: actions/upload-artifact@v4
      with:
        name: security-report
        path: reports/
        
  # Performance Testing
  performance-test:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Run performance tests
      run: |
        dotnet test --filter Category=Performance --logger trx --results-directory ./TestResults
        
    - name: Upload performance results
      uses: actions/upload-artifact@v4
      with:
        name: performance-results
        path: TestResults/
```

### 7.2 Azure DevOps Pipelines

#### Pipeline Básico de Azure DevOps

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main
    - develop
    - feature/*

pr:
  branches:
    include:
    - main
    - develop

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'
  dotNetVersion: '8.0.x'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - job: Build
    pool:
      vmImage: 'ubuntu-latest'
      
    steps:
    - task: UseDotNet@2
      displayName: 'Use .NET $(dotNetVersion) SDK'
      inputs:
        version: '$(dotNetVersion)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Restore NuGet packages'
      inputs:
        command: 'restore'
        projects: '$(solution)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration) --no-restore'
        
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build --collect:"XPlat Code Coverage"'
        
    - task: PublishTestResults@2
      displayName: 'Publish test results'
      inputs:
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        
    - task: PublishCodeCoverageResults@1
      displayName: 'Publish code coverage'
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '**/coverage.cobertura.xml'
        
    - task: PublishBuildArtifacts@1
      displayName: 'Publish build artifacts'
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: Deploy
  displayName: 'Deploy to Staging'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/develop'))
  jobs:
  - deployment: Deploy
    pool:
      vmImage: 'ubuntu-latest'
    environment: 'staging'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: DownloadBuildArtifacts@1
            inputs:
              buildType: 'current'
              artifactName: 'drop'
              downloadPath: '$(System.ArtifactsDirectory)'
              
          - script: |
              echo "Deploying to staging..."
              # Comandos de deployment
```

#### Pipeline con Variables y Templates

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main
    - develop

variables:
  - group: production-secrets
  - name: solution
    value: '**/*.sln'
  - name: buildPlatform
    value: 'Any CPU'
  - name: buildConfiguration
    value: 'Release'
  - name: dotNetVersion
    value: '8.0.x'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - template: templates/build-job.yml
    parameters:
      jobName: 'Build'
      vmImage: 'ubuntu-latest'
      
- stage: Security
  displayName: 'Security Scan'
  dependsOn: Build
  jobs:
  - template: templates/security-job.yml
    parameters:
      jobName: 'Security'
      vmImage: 'ubuntu-latest'
      
- stage: DeployStaging
  displayName: 'Deploy to Staging'
  dependsOn: Security
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/develop'))
  jobs:
  - template: templates/deploy-job.yml
    parameters:
      jobName: 'DeployStaging'
      environment: 'staging'
      vmImage: 'ubuntu-latest'
      
- stage: DeployProduction
  displayName: 'Deploy to Production'
  dependsOn: DeployStaging
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - template: templates/deploy-job.yml
    parameters:
      jobName: 'DeployProduction'
      environment: 'production'
      vmImage: 'ubuntu-latest'
```

#### Template de Build Job

```yaml
# templates/build-job.yml
parameters:
  - name: jobName
    type: string
  - name: vmImage
    type: string

jobs:
- job: ${{ parameters.jobName }}
  pool:
    vmImage: ${{ parameters.vmImage }}
    
  steps:
  - task: UseDotNet@2
    displayName: 'Use .NET $(dotNetVersion) SDK'
    inputs:
      version: '$(dotNetVersion)'
      
  - task: DotNetCoreCLI@2
    displayName: 'Restore NuGet packages'
    inputs:
      command: 'restore'
      projects: '$(solution)'
      
  - task: DotNetCoreCLI@2
    displayName: 'Build solution'
    inputs:
      command: 'build'
      projects: '$(solution)'
      arguments: '--configuration $(buildConfiguration) --no-restore'
      
  - task: DotNetCoreCLI@2
    displayName: 'Run tests'
    inputs:
      command: 'test'
      projects: '**/*Tests/*.csproj'
      arguments: '--configuration $(buildConfiguration) --no-build --collect:"XPlat Code Coverage"'
      
  - task: PublishTestResults@2
    displayName: 'Publish test results'
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
      
  - task: PublishCodeCoverageResults@1
    displayName: 'Publish code coverage'
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '**/coverage.cobertura.xml'
      
  - task: PublishBuildArtifacts@1
    displayName: 'Publish build artifacts'
    inputs:
      pathToPublish: '$(Build.ArtifactStagingDirectory)'
      artifactName: 'drop'
```

### 7.3 Testing Automatizado en CI/CD

#### Pipeline con Testing Completo

```yaml
# .github/workflows/testing-pipeline.yml
name: Testing Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  # Unit Tests
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Run unit tests
      run: dotnet test --filter Category=Unit --logger trx --results-directory ./TestResults/Unit
      
    - name: Upload unit test results
      uses: actions/upload-artifact@v4
      with:
        name: unit-test-results
        path: TestResults/Unit/
        
  # Integration Tests
  integration-tests:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: Test_password123
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Test_password123 -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
          
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Wait for SQL Server
      run: |
        while ! /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Test_password123 -Q "SELECT 1" &> /dev/null; do
          echo "Waiting for SQL Server..."
          sleep 2
        done
        
    - name: Run integration tests
      run: |
        dotnet test --filter Category=Integration --logger trx --results-directory ./TestResults/Integration
        env:
          ConnectionStrings__DefaultConnection: "Server=localhost;Database=TestDb;User Id=sa;Password=Test_password123;"
          
    - name: Upload integration test results
      uses: actions/upload-artifact@v4
      with:
        name: integration-test-results
        path: TestResults/Integration/
        
  # Performance Tests
  performance-tests:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install NBomber
      run: dotnet tool install --global NBomber.CLI
        
    - name: Run performance tests
      run: |
        dotnet test --filter Category=Performance --logger trx --results-directory ./TestResults/Performance
        
    - name: Upload performance test results
      uses: actions/upload-artifact@v4
      with:
        name: performance-test-results
        path: TestResults/Performance/
        
  # Security Tests
  security-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run OWASP ZAP scan
      uses: zaproxy/action-full-scan@v0.8.0
      with:
        target: 'http://localhost:5000'
        
    - name: Upload security scan results
      uses: actions/upload-artifact@v4
      with:
        name: security-scan-results
        path: zap-report.html
```

### 7.4 Deployment Automatizado

#### Pipeline de Deployment

```yaml
# .github/workflows/deployment.yml
name: Deployment Pipeline

on:
  workflow_run:
    workflows: ["Testing Pipeline"]
    types: [completed]
    branches: [main, develop]

jobs:
  # Deploy to Staging
  deploy-staging:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' && github.ref == 'refs/heads/develop' }}
    environment: staging
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to staging
      run: |
        echo "Deploying to staging environment..."
        # Comandos para desplegar en staging
        
    - name: Run smoke tests
      run: |
        echo "Running smoke tests..."
        # Tests básicos de funcionamiento
        
    - name: Notify team
      run: |
        echo "Staging deployment completed successfully"
        
  # Deploy to Production
  deploy-production:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' && github.ref == 'refs/heads/main' }}
    environment: production
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to production
      run: |
        echo "Deploying to production environment..."
        # Comandos para desplegar en producción
        
    - name: Run health checks
      run: |
        echo "Running health checks..."
        # Verificaciones de salud del sistema
        
    - name: Notify stakeholders
      run: |
        echo "Production deployment completed successfully"
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Crear Pipeline de CI/CD Completo

Implementa un pipeline completo con:

```yaml
# Incluye:
# - Build y testing automatizado
# - Análisis de código
# - Escaneo de seguridad
# - Deployment a múltiples entornos
# - Notificaciones
```

### Ejercicio 2: Pipeline con Testing de Microservicios

Crea un pipeline para arquitectura de microservicios:

```yaml
# Implementa:
# - Testing de cada microservicio
# - Testing de integración
# - Testing de contrato
# - Deployment coordinado
```

---

## 🔍 Casos de Uso Reales

### 1. Pipeline para Microservicios

```yaml
# .github/workflows/microservices-pipeline.yml
name: Microservices Pipeline

on:
  push:
    paths:
    - 'src/UsersService/**'
    - 'src/OrdersService/**'
    - 'src/ProductsService/**'

jobs:
  # Build all services
  build-services:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [UsersService, OrdersService, ProductsService]
        
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Build ${{ matrix.service }}
      run: |
        dotnet build src/${{ matrix.service }}/${{ matrix.service }}.csproj --configuration Release
        
    - name: Test ${{ matrix.service }}
      run: |
        dotnet test src/${{ matrix.service }}.Tests/${{ matrix.service }}.Tests.csproj --configuration Release
        
    - name: Build Docker image
      run: |
        docker build -t myapp/${{ matrix.service }}:latest src/${{ matrix.service }}/
```

### 2. Pipeline con Approval Gates

```yaml
# azure-pipelines.yml
stages:
- stage: DeployProduction
  displayName: 'Deploy to Production'
  dependsOn: DeployStaging
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: Deploy
    pool:
      vmImage: 'ubuntu-latest'
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: ManualValidation@0
            inputs:
              notifyUsers: 'stakeholders@myapp.com'
              instructions: 'Please approve the production deployment'
              onTimeout: 'reject'
              
          - script: |
              echo "Deploying to production..."
              # Comandos de deployment
```

---

## 📊 Métricas de CI/CD

### KPIs de Pipeline

1. **Build Success Rate**: Tasa de éxito de builds
2. **Test Coverage**: Cobertura de tests
3. **Deployment Frequency**: Frecuencia de deployments
4. **Lead Time**: Tiempo desde commit hasta producción
5. **Mean Time to Recovery**: Tiempo medio de recuperación

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **GitHub Actions**: Implementación de pipelines para .NET
✅ **Azure DevOps**: Configuración de pipelines empresariales
✅ **Testing Automatizado**: Integración de tests en CI/CD
✅ **Deployment Automatizado**: Despliegue continuo a múltiples entornos
✅ **Casos de Uso Reales**: Implementación en proyectos reales

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Monitoreo y Observabilidad**
- Logging estructurado con Serilog
- Métricas con Prometheus
- Health checks y readiness probes

---

## 🔗 Enlaces de Referencia

- [GitHub Actions for .NET](https://docs.github.com/en/actions/guides/building-and-testing-net)
- [Azure DevOps for .NET](https://docs.microsoft.com/en-us/azure/devops/pipelines/languages/dotnet-core)
- [CI/CD Best Practices](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/define-multistage-release-process)
- [Testing in CI/CD](https://docs.microsoft.com/en-us/azure/devops/pipelines/test/continuous-test-selenium)

