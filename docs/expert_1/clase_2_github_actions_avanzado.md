# 🚀 Clase 2: GitHub Actions Avanzado

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 1: Fundamentos de DevOps y Git Workflow](../expert_1/clase_1_fundamentos_devops_git_workflow.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 3: Docker y Containerización Avanzada](../expert_1/clase_3_docker_containerizacion_avanzada.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Dominar** workflows complejos y matrices
2. **Implementar** secrets management y environments
3. **Crear** custom actions y reusable workflows
4. **Configurar** security scanning y dependency management
5. **Optimizar** performance y caching

---

## 🔄 **Workflows Complejos y Matrices**

### **Workflow con Matrices**

```yaml
# .github/workflows/build-matrix.yml
name: Build Matrix

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['6.0', '7.0', '8.0']
        os: [ubuntu-latest, windows-latest, macos-latest]
        include:
          - dotnet-version: '6.0'
            os: ubuntu-latest
            test-group: 'unit'
          - dotnet-version: '7.0'
            os: windows-latest
            test-group: 'integration'
          - dotnet-version: '8.0'
            os: macos-latest
            test-group: 'e2e'
        exclude:
          - dotnet-version: '6.0'
            os: windows-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet-version }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --logger trx --results-directory ./TestResults
        if: matrix.test-group != 'e2e'

      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results-${{ matrix.dotnet-version }}-${{ matrix.os }}
          path: ./TestResults
```

### **Workflow Dependiente**

```yaml
# .github/workflows/deployment-pipeline.yml
name: Deployment Pipeline

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    outputs:
      test-result: ${{ steps.test.outcome }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Run tests
        id: test
        run: |
          dotnet test
          echo "outcome=success" >> $GITHUB_OUTPUT

  security-scan:
    runs-on: ubuntu-latest
    needs: test
    if: needs.test.outputs.test-result == 'success'
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Run security scan
        run: |
          dotnet list package --vulnerable
          # OWASP ZAP scan
          docker run -t owasp/zap2docker-stable zap-baseline.py -t https://localhost

  build:
    runs-on: ubuntu-latest
    needs: [test, security-scan]
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Build Docker image
        run: |
          docker build -t mussikon:${{ github.sha }} .
          docker tag mussikon:${{ github.sha }} mussikon:latest

  deploy-staging:
    runs-on: ubuntu-latest
    needs: build
    environment: staging
    steps:
      - name: Deploy to staging
        run: |
          echo "Deploying to staging environment"
          # Deploy logic here

  deploy-production:
    runs-on: ubuntu-latest
    needs: deploy-staging
    environment: production
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to production
        run: |
          echo "Deploying to production environment"
          # Deploy logic here
```

---

## 🔐 **Secrets Management y Environments**

### **Configuración de Secrets**

```yaml
# .github/workflows/secrets-management.yml
name: Secrets Management

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: production
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy to Azure
        run: |
          az webapp deployment source config-zip \
            --resource-group ${{ secrets.AZURE_RESOURCE_GROUP }} \
            --name ${{ secrets.AZURE_WEBAPP_NAME }} \
            --src ./deployment.zip

      - name: Notify deployment
        uses: 8398a7/action-slack@v3
        with:
          status: ${{ job.status }}
          channel: '#deployments'
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### **Environment Protection Rules**

```yaml
# .github/workflows/environment-protection.yml
name: Environment Protection

on:
  push:
    branches: [main]

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment: 
      name: staging
      url: https://staging.mussikon.com
    steps:
      - name: Deploy to staging
        run: echo "Deploying to staging"

  deploy-production:
    runs-on: ubuntu-latest
    needs: deploy-staging
    environment: 
      name: production
      url: https://mussikon.com
    steps:
      - name: Deploy to production
        run: echo "Deploying to production"
```

---

## 🛠️ **Custom Actions y Reusable Workflows**

### **Custom Action**

```yaml
# .github/actions/setup-dotnet/action.yml
name: 'Setup .NET with caching'
description: 'Setup .NET with intelligent caching'
inputs:
  dotnet-version:
    description: 'The .NET version to use'
    required: true
    default: '8.0'
  cache-key:
    description: 'Custom cache key'
    required: false
    default: 'dotnet'

outputs:
  cache-hit:
    description: 'Whether the cache was hit'
    value: ${{ steps.cache.outputs.cache-hit }}

runs:
  using: 'composite'
  steps:
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ inputs.dotnet-version }}

    - name: Cache dependencies
      id: cache
      uses: actions/cache@v3
      with:
        path: ~/.nuget/packages
        key: ${{ runner.os }}-${{ inputs.cache-key }}-${{ hashFiles('**/*.csproj') }}
        restore-keys: |
          ${{ runner.os }}-${{ inputs.cache-key }}-

    - name: Restore dependencies
      shell: bash
      run: |
        if [ "${{ steps.cache.outputs.cache-hit }}" != "true" ]; then
          dotnet restore
        else
          echo "Cache hit, skipping restore"
        fi
```

### **Reusable Workflow**

```yaml
# .github/workflows/build-and-test.yml
name: Build and Test

on:
  workflow_call:
    inputs:
      dotnet-version:
        required: false
        type: string
        default: '8.0'
      test-group:
        required: false
        type: string
        default: 'all'
      publish-coverage:
        required: false
        type: boolean
        default: true
    outputs:
      test-results:
        description: "Test results"
        value: ${{ jobs.test.outputs.results }}
      coverage-report:
        description: "Coverage report"
        value: ${{ jobs.test.outputs.coverage }}

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ inputs.dotnet-version }}

      - name: Build
        run: dotnet build --configuration Release

  test:
    runs-on: ubuntu-latest
    needs: build
    outputs:
      results: ${{ steps.test-results.outputs.results }}
      coverage: ${{ steps.coverage.outputs.coverage }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ inputs.dotnet-version }}

      - name: Run tests
        id: test-results
        run: |
          dotnet test --configuration Release --collect:"XPlat Code Coverage" --logger trx
          echo "results=success" >> $GITHUB_OUTPUT

      - name: Generate coverage report
        id: coverage
        if: inputs.publish-coverage == true
        run: |
          # Generate coverage report
          echo "coverage=generated" >> $GITHUB_OUTPUT
```

### **Usar Reusable Workflow**

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    uses: ./.github/workflows/build-and-test.yml
    with:
      dotnet-version: '8.0'
      test-group: 'unit'
      publish-coverage: true

  integration-tests:
    uses: ./.github/workflows/build-and-test.yml
    with:
      dotnet-version: '8.0'
      test-group: 'integration'
      publish-coverage: false
```

---

## 🔒 **Security Scanning y Dependency Management**

### **Security Scanning Workflow**

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 2 * * 1' # Weekly on Monday at 2 AM

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

### **Dependency Management**

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
          # Add more packages as needed

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
            
            ## Testing
            - [ ] All tests pass
            - [ ] No breaking changes
          branch: automated-dependency-update
          delete-branch: true
```

---

## ⚡ **Performance y Caching**

### **Optimización con Caching**

```yaml
# .github/workflows/optimized-build.yml
name: Optimized Build

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Cache NuGet packages
        uses: actions/cache@v3
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Cache build outputs
        uses: actions/cache@v3
        with:
          path: |
            **/bin
            **/obj
          key: ${{ runner.os }}-build-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-build-

      - name: Restore dependencies
        run: dotnet restore --verbosity minimal

      - name: Build
        run: dotnet build --no-restore --configuration Release --verbosity minimal

      - name: Test
        run: dotnet test --no-build --configuration Release --verbosity minimal --collect:"XPlat Code Coverage"

      - name: Upload coverage reports
        uses: codecov/codecov-action@v3
        with:
          file: ./**/coverage.cobertura.xml
          flags: unittests
          name: codecov-umbrella
```

### **Parallel Jobs**

```yaml
# .github/workflows/parallel-jobs.yml
name: Parallel Jobs

on:
  push:
    branches: [main]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Run unit tests
        run: dotnet test --filter Category=Unit

  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Run integration tests
        run: dotnet test --filter Category=Integration

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Run E2E tests
        run: dotnet test --filter Category=E2E

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Security scan
        run: dotnet list package --vulnerable

  build-artifacts:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: build-artifacts
          path: ./bin/Release

  deploy:
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests, e2e-tests, security-scan, build-artifacts]
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy
        run: echo "Deploying application"
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Workflow con Matrices**
```yaml
# Crea un workflow que ejecute tests en múltiples versiones de .NET
# y sistemas operativos usando matrices
```

### **Ejercicio 2: Custom Action**
```yaml
# Crea una custom action que configure .NET con caching inteligente
```

### **Ejercicio 3: Security Scanning**
```yaml
# Implementa un workflow de security scanning completo
# con dependency scanning, code scanning y container scanning
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔄 Workflows Complejos**: Matrices y jobs dependientes
2. **🔐 Secrets Management**: Gestión segura de credenciales
3. **🛠️ Custom Actions**: Creación de acciones reutilizables
4. **🔒 Security Scanning**: Escaneo de vulnerabilidades
5. **⚡ Performance**: Caching y optimización
6. **📦 Dependency Management**: Actualización automática

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Docker y Containerización Avanzada**, implementando multi-stage builds y optimización.

---

**¡Has completado la segunda clase del Expert Level 1! 🚀🎯**
