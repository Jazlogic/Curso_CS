# 🚀 Clase 1: Fundamentos de DevOps y Git Workflow

## 🧭 Navegación del Módulo

- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 2: GitHub Actions Avanzado](../expert_1/clase_2_github_actions_avanzado.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Dominar** estrategias de branching avanzadas
2. **Implementar** conventional commits y semantic versioning
3. **Configurar** code review workflows
4. **Automatizar** pull request templates
5. **Establecer** Git hooks y pre-commit checks

---

## 🌿 **Git Branching Strategies**

### **GitFlow Strategy**

```bash
# Estructura de ramas GitFlow
main                    # Producción
├── develop            # Desarrollo principal
├── feature/user-auth  # Nuevas funcionalidades
├── release/v1.2.0    # Preparación de releases
└── hotfix/critical-bug # Fixes críticos
```

#### **Configuración GitFlow**

```bash
# Instalar GitFlow
git flow init

# Configurar ramas principales
git config gitflow.branch.main main
git config gitflow.branch.develop develop
git config gitflow.prefix.feature feature/
git config gitflow.prefix.release release/
git config gitflow.prefix.hotfix hotfix/
```

#### **Workflow GitFlow**

```bash
# Iniciar nueva feature
git flow feature start user-authentication
git flow feature finish user-authentication

# Iniciar release
git flow release start 1.2.0
git flow release finish 1.2.0

# Iniciar hotfix
git flow hotfix start critical-security-fix
git flow hotfix finish critical-security-fix
```

### **GitHub Flow Strategy**

```bash
# Estructura simplificada
main                    # Producción
└── feature/new-feature # Features directas
```

#### **Configuración GitHub Flow**

```bash
# Configurar rama principal
git config init.defaultBranch main

# Configurar push upstream
git config push.default current
git config push.autoSetupRemote true
```

---

## 📝 **Conventional Commits**

### **Especificación Conventional Commits**

```bash
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### **Tipos de Commits**

```bash
# Tipos principales
feat:     # Nueva funcionalidad
fix:      # Corrección de bug
docs:     # Documentación
style:    # Formato, espacios, etc.
refactor: # Refactoring de código
test:     # Agregar o modificar tests
chore:    # Tareas de mantenimiento

# Tipos adicionales
perf:     # Mejora de performance
ci:       # Cambios en CI/CD
build:    # Cambios en build system
revert:   # Revertir commit anterior
```

### **Ejemplos de Commits**

```bash
# Feature
feat(auth): add JWT token validation
feat(api): implement user registration endpoint

# Fix
fix(payment): resolve credit card validation issue
fix(chat): handle connection timeout gracefully

# Documentation
docs(readme): update installation instructions
docs(api): add authentication examples

# Refactoring
refactor(domain): extract user validation logic
refactor(service): simplify payment processing

# Performance
perf(database): optimize user query performance
perf(cache): implement Redis caching strategy
```

### **Configuración de Conventional Commits**

```bash
# Instalar commitizen
npm install -g commitizen cz-conventional-changelog

# Configurar commitizen
echo '{ "path": "cz-conventional-changelog" }' > ~/.czrc

# Usar commitizen
git cz
```

---

## 🏷️ **Semantic Versioning**

### **Especificación SemVer**

```
MAJOR.MINOR.PATCH
```

- **MAJOR**: Cambios incompatibles en API
- **MINOR**: Nueva funcionalidad compatible
- **PATCH**: Correcciones de bugs compatibles

### **Ejemplos de Versionado**

```bash
# Versiones de ejemplo
1.0.0    # Primera release estable
1.0.1    # Fix de bug
1.1.0    # Nueva funcionalidad
1.1.1    # Fix de bug
2.0.0    # Breaking change
2.0.1    # Fix de bug
```

### **Configuración de Semantic Versioning**

```bash
# Instalar semantic-release
npm install -g semantic-release

# Configurar .releaserc
{
  "branches": ["main"],
  "plugins": [
    "@semantic-release/commit-analyzer",
    "@semantic-release/release-notes-generator",
    "@semantic-release/changelog",
    "@semantic-release/git",
    "@semantic-release/github"
  ]
}
```

---

## 🔍 **Code Review Workflows**

### **Configuración de Branch Protection**

```yaml
# .github/branch-protection.yml
name: Branch Protection
on:
  push:
    branches: [main, develop]

jobs:
  branch-protection:
    runs-on: ubuntu-latest
    steps:
      - name: Check branch protection
        uses: actions/github-script@v6
        with:
          script: |
            const { data: branch } = await github.rest.repos.getBranch({
              owner: context.repo.owner,
              repo: context.repo.repo,
              branch: context.ref.replace('refs/heads/', '')
            });
            
            if (!branch.protected) {
              throw new Error('Branch is not protected');
            }
```

### **Pull Request Template**

```markdown
<!-- .github/pull_request_template.md -->
## 📋 Descripción
Breve descripción de los cambios realizados.

## 🎯 Tipo de Cambio
- [ ] Bug fix (cambio que corrige un problema)
- [ ] Nueva funcionalidad (cambio que agrega funcionalidad)
- [ ] Breaking change (fix o funcionalidad que causaría que la funcionalidad existente no funcione como se espera)
- [ ] Documentación (cambios solo en documentación)

## ✅ Checklist
- [ ] Mi código sigue las convenciones de estilo del proyecto
- [ ] He realizado una auto-revisión de mi código
- [ ] He comentado mi código, especialmente en áreas difíciles de entender
- [ ] He hecho los cambios correspondientes en la documentación
- [ ] Mis cambios no generan nuevas advertencias
- [ ] He agregado tests que prueban que mi fix es efectivo o que mi funcionalidad funciona
- [ ] Los tests nuevos y existentes pasan localmente con mis cambios
- [ ] Cualquier cambio dependiente ha sido mergeado y publicado

## 🧪 Testing
- [ ] Tests unitarios
- [ ] Tests de integración
- [ ] Tests de performance (si aplica)

## 📸 Screenshots (si aplica)
Agregar screenshots para cambios en UI.

## 🔗 Issues Relacionados
Closes #123
```

### **Code Review Guidelines**

```markdown
# Code Review Guidelines

## 🎯 Objetivos del Code Review
- Mejorar la calidad del código
- Compartir conocimiento
- Detectar bugs temprano
- Mantener consistencia

## ✅ Checklist del Reviewer
- [ ] El código es legible y mantenible
- [ ] Sigue las convenciones del proyecto
- [ ] Tiene tests apropiados
- [ ] No introduce vulnerabilidades de seguridad
- [ ] Performance es aceptable
- [ ] Documentación está actualizada

## 💬 Comentarios Constructivos
- Ser específico y claro
- Explicar el "por qué" no solo el "qué"
- Sugerir alternativas cuando sea apropiado
- Reconocer buenas prácticas
- Ser respetuoso y profesional
```

---

## 🔧 **Git Hooks y Pre-commit Checks**

### **Pre-commit Hook**

```bash
#!/bin/sh
# .git/hooks/pre-commit

echo "🔍 Ejecutando pre-commit checks..."

# Verificar conventional commits
commit_msg_file=$1
commit_msg=$(cat $commit_msg_file)

if ! echo "$commit_msg" | grep -qE "^(feat|fix|docs|style|refactor|test|chore)(\(.+\))?: .+"; then
    echo "❌ Error: Commit message no sigue conventional commits"
    echo "Formato esperado: type(scope): description"
    echo "Tipos válidos: feat, fix, docs, style, refactor, test, chore"
    exit 1
fi

# Ejecutar tests
echo "🧪 Ejecutando tests..."
dotnet test --no-build --verbosity quiet
if [ $? -ne 0 ]; then
    echo "❌ Error: Tests fallaron"
    exit 1
fi

# Ejecutar linting
echo "🔍 Ejecutando linting..."
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
    echo "❌ Error: Código no está formateado correctamente"
    exit 1
fi

echo "✅ Pre-commit checks pasaron exitosamente"
exit 0
```

### **Commit-msg Hook**

```bash
#!/bin/sh
# .git/hooks/commit-msg

commit_msg_file=$1
commit_msg=$(cat $commit_msg_file)

# Verificar conventional commits
if ! echo "$commit_msg" | grep -qE "^(feat|fix|docs|style|refactor|test|chore)(\(.+\))?: .+"; then
    echo "❌ Error: Commit message no sigue conventional commits"
    echo "Formato esperado: type(scope): description"
    echo "Tipos válidos: feat, fix, docs, style, refactor, test, chore"
    exit 1
fi

# Verificar longitud del mensaje
if [ ${#commit_msg} -gt 100 ]; then
    echo "❌ Error: Commit message es muy largo (máximo 100 caracteres)"
    exit 1
fi

echo "✅ Commit message es válido"
exit 0
```

### **Pre-push Hook**

```bash
#!/bin/sh
# .git/hooks/pre-push

echo "🚀 Ejecutando pre-push checks..."

# Ejecutar tests completos
echo "🧪 Ejecutando test suite completo..."
dotnet test --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Error: Test suite falló"
    exit 1
fi

# Ejecutar análisis de código
echo "🔍 Ejecutando análisis de código..."
dotnet build --configuration Release --verbosity quiet
if [ $? -ne 0 ]; then
    echo "❌ Error: Build falló"
    exit 1
fi

echo "✅ Pre-push checks pasaron exitosamente"
exit 0
```

---

## 🤖 **Automatización con GitHub**

### **Issue Templates**

```markdown
<!-- .github/ISSUE_TEMPLATE/bug_report.md -->
---
name: Bug report
about: Crear un reporte para ayudarnos a mejorar
title: '[BUG] '
labels: bug
assignees: ''
---

**Describe el bug**
Una descripción clara y concisa de qué es el bug.

**Para reproducir**
Pasos para reproducir el comportamiento:
1. Ir a '...'
2. Hacer clic en '....'
3. Scroll hacia abajo hasta '....'
4. Ver error

**Comportamiento esperado**
Una descripción clara y concisa de qué esperabas que pasara.

**Screenshots**
Si aplica, agrega screenshots para ayudar a explicar tu problema.

**Información adicional:**
 - OS: [e.g. Windows 10]
 - Browser: [e.g. chrome, safari]
 - Versión: [e.g. 22]

**Contexto adicional**
Agrega cualquier otro contexto sobre el problema aquí.
```

```markdown
<!-- .github/ISSUE_TEMPLATE/feature_request.md -->
---
name: Feature request
about: Sugerir una idea para este proyecto
title: '[FEATURE] '
labels: enhancement
assignees: ''
---

**¿Tu feature request está relacionada con un problema? Por favor describe.**
Una descripción clara y concisa de cuál es el problema. Ex. Siempre me frustra cuando [...]

**Describe la solución que te gustaría**
Una descripción clara y concisa de qué quieres que pase.

**Describe alternativas que has considerado**
Una descripción clara y concisa de cualquier solución o feature alternativa que hayas considerado.

**Contexto adicional**
Agrega cualquier otro contexto o screenshots sobre la feature request aquí.
```

### **GitHub Actions para Automatización**

```yaml
# .github/workflows/auto-assign.yml
name: Auto Assign
on:
  issues:
    types: [opened]
  pull_request:
    types: [opened]

jobs:
  auto-assign:
    runs-on: ubuntu-latest
    steps:
      - name: Auto assign issue
        if: github.event_name == 'issues'
        uses: actions/github-script@v6
        with:
          script: |
            const { owner, repo, number } = context.issue;
            const assignees = ['developer1', 'developer2'];
            
            await github.rest.issues.addAssignees({
              owner,
              repo,
              issue_number: number,
              assignees
            });

      - name: Auto assign PR
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const { owner, repo, number } = context.pull_request;
            const reviewers = ['reviewer1', 'reviewer2'];
            
            await github.rest.pulls.requestReviewers({
              owner,
              repo,
              pull_number: number,
              reviewers
            });
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Configurar GitFlow**
```bash
# Configura GitFlow en tu proyecto MussikOn
git flow init
git flow feature start user-dashboard
# Hacer cambios
git flow feature finish user-dashboard
```

### **Ejercicio 2: Conventional Commits**
```bash
# Instala commitizen y haz commits siguiendo conventional commits
npm install -g commitizen cz-conventional-changelog
git cz
```

### **Ejercicio 3: Git Hooks**
```bash
# Configura pre-commit hooks en tu proyecto
chmod +x .git/hooks/pre-commit
chmod +x .git/hooks/commit-msg
chmod +x .git/hooks/pre-push
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🌿 Git Branching**: GitFlow y GitHub Flow strategies
2. **📝 Conventional Commits**: Estándares de mensajes de commit
3. **🏷️ Semantic Versioning**: Versionado semántico
4. **🔍 Code Review**: Workflows y templates
5. **🔧 Git Hooks**: Automatización de checks
6. **🤖 GitHub Automation**: Templates y automatización

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **GitHub Actions Avanzado**, implementando workflows complejos y automatización.

---

**¡Has completado la primera clase del Expert Level 1! 🚀🎯**
