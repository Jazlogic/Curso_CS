# 🚀 Clase 6: Orquestación con Kubernetes

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 5: Containerización con Docker](clase_5_containerizacion_docker.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 7: CI/CD y Pipelines](clase_7_cicd_pipelines.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Crear y configurar manifests de Kubernetes
- Implementar deployments y servicios
- Gestionar ConfigMaps y Secrets
- Configurar Horizontal Pod Autoscaler

---

## 📚 Contenido Teórico

### 6.1 Manifests de Kubernetes

#### Deployment Básico para .NET

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-api
  namespace: production
  labels:
    app: myapp-api
    version: v1.0.0
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp-api
  template:
    metadata:
      labels:
        app: myapp-api
        version: v1.0.0
    spec:
      containers:
      - name: myapp-api
        image: myapp/api:latest
        ports:
        - containerPort: 80
          protocol: TCP
        - containerPort: 443
          protocol: TCP
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:80;https://+:443"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        securityContext:
          runAsNonRoot: true
          runAsUser: 1001
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
      imagePullSecrets:
      - name: myapp-registry-secret
      restartPolicy: Always
      terminationGracePeriodSeconds: 30
```

#### Service para Exposición

```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: myapp-api-service
  namespace: production
  labels:
    app: myapp-api
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 80
    protocol: TCP
    name: http
  - port: 443
    targetPort: 443
    protocol: TCP
    name: https
  selector:
    app: myapp-api
  sessionAffinity: ClientIP
  sessionAffinityConfig:
    clientIP:
      timeoutSeconds: 10800
```

#### Ingress para Routing Externo

```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: myapp-ingress
  namespace: production
  annotations:
    kubernetes.io/ingress.class: "nginx"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    nginx.ingress.kubernetes.io/ssl-passthrough: "true"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - api.myapp.com
    secretName: myapp-tls-secret
  rules:
  - host: api.myapp.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: myapp-api-service
            port:
              number: 80
```

### 6.2 Deployments y Servicios

#### Deployment con Rolling Update

```yaml
# k8s/deployment-rolling.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-api-rolling
  namespace: production
spec:
  replicas: 5
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 2
      maxUnavailable: 1
  selector:
    matchLabels:
      app: myapp-api
  template:
    metadata:
      labels:
        app: myapp-api
    spec:
      containers:
      - name: myapp-api
        image: myapp/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

#### Service LoadBalancer

```yaml
# k8s/service-loadbalancer.yaml
apiVersion: v1
kind: Service
metadata:
  name: myapp-api-lb
  namespace: production
  annotations:
    service.beta.kubernetes.io/aws-load-balancer-type: "nlb"
    service.beta.kubernetes.io/aws-load-balancer-cross-zone-load-balancing-enabled: "true"
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 80
    protocol: TCP
    name: http
  - port: 443
    targetPort: 443
    protocol: TCP
    name: https
  selector:
    app: myapp-api
  externalTrafficPolicy: Local
  healthCheckNodePort: 30000
```

### 6.3 ConfigMaps y Secrets

#### ConfigMap para Configuración

```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: myapp-config
  namespace: production
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Warning",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "DefaultConnection": "Server=myapp-db-service;Database=MyAppDb;User Id=sa;Password=placeholder;"
      },
      "Redis": {
        "ConnectionString": "myapp-redis-service:6379"
      },
      "RabbitMQ": {
        "Host": "myapp-rabbitmq-service",
        "Port": 5672,
        "Username": "admin",
        "Password": "placeholder"
      },
      "Jwt": {
        "Issuer": "myapp.com",
        "Audience": "myapp.com",
        "ExpiryMinutes": 60
      }
    }
  nlog.config: |
    <?xml version="1.0" encoding="utf-8" ?>
    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
           xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <targets>
        <target name="console" xsi:type="Console" />
        <target name="file" xsi:type="File" fileName="/var/log/myapp/app.log" />
      </targets>
      <rules>
        <logger name="*" minlevel="Info" writeTo="console,file" />
      </rules>
    </nlog>
```

#### Secret para Datos Sensibles

```yaml
# k8s/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: myapp-secrets
  namespace: production
type: Opaque
data:
  # Base64 encoded values
  connection-string: U2VydmVyPW15YXBwLWRiLXNlcnZpY2U7RGF0YWJhc2U9TXlBcHBEYjtVc2VyIElkPXNhO1Bhc3N3b3JkPVN1cGVyU2VjcmV0MTIzIQ==
  redis-password: UmVkaXNQYXNzd29yZDEyMw==
  rabbitmq-password: UmFiYml0TVFQYXNzd29yZDEyMw==
  jwt-secret-key: SnNvbldlYlRva2VuU2VjcmV0S2V5Rm9yTXlBcHAyMDI0IQ==
  api-key: QVBJS2V5Rm9yTXlBcHAyMDI0IQ==
```

#### Deployment usando ConfigMap y Secret

```yaml
# k8s/deployment-with-config.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-api-config
  namespace: production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp-api
  template:
    metadata:
      labels:
        app: myapp-api
    spec:
      containers:
      - name: myapp-api
        image: myapp/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: connection-string
        - name: Redis__Password
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: redis-password
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: rabbitmq-password
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: jwt-secret-key
        volumeMounts:
        - name: config-volume
          mountPath: /app/config
        - name: logs-volume
          mountPath: /var/log/myapp
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
      volumes:
      - name: config-volume
        configMap:
          name: myapp-config
          items:
          - key: appsettings.json
            path: appsettings.json
          - key: nlog.config
            path: nlog.config
      - name: logs-volume
        emptyDir: {}
```

### 6.4 Horizontal Pod Autoscaler

#### HPA para Escalado Automático

```yaml
# k8s/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: myapp-api-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: myapp-api
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  - type: Object
    object:
      metric:
        name: requests-per-second
      describedObject:
        apiVersion: networking.k8s.io/v1
        kind: Ingress
        name: myapp-ingress
      target:
        type: Value
        value: 1000
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 100
        periodSeconds: 15
      - type: Pods
        value: 4
        periodSeconds: 15
      selectPolicy: Max
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
      selectPolicy: Min
```

#### HPA con Métricas Personalizadas

```yaml
# k8s/hpa-custom-metrics.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: myapp-api-hpa-custom
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: myapp-api
  minReplicas: 2
  maxReplicas: 15
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 65
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 75
  - type: Object
    object:
      metric:
        name: http_requests_total
      describedObject:
        apiVersion: v1
        kind: Service
        name: myapp-api-service
      target:
        type: AverageValue
        averageValue: 500
  - type: Pods
    pods:
      metric:
        name: packets-per-second
      target:
        type: AverageValue
        averageValue: 1000
```

### 6.5 Namespace y RBAC

#### Namespace para Organización

```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: production
  labels:
    name: production
    environment: production
    team: backend
  annotations:
    description: "Production environment for MyApp"
    owner: "backend-team@myapp.com"
```

#### ServiceAccount y RBAC

```yaml
# k8s/rbac.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: myapp-service-account
  namespace: production
  labels:
    app: myapp-api
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: myapp-role
  namespace: production
rules:
- apiGroups: [""]
  resources: ["pods", "services", "endpoints"]
  verbs: ["get", "list", "watch"]
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list", "watch"]
- apiGroups: ["apps"]
  resources: ["deployments", "replicasets"]
  verbs: ["get", "list", "watch"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: myapp-role-binding
  namespace: production
subjects:
- kind: ServiceAccount
  name: myapp-service-account
  namespace: production
roleRef:
  kind: Role
  name: myapp-role
  apiGroup: rbac.authorization.k8s.io
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Crear Deployment para Microservicio

Crea un deployment completo para un microservicio de usuarios:

```yaml
# Implementa:
# - Deployment con health checks
# - Service ClusterIP
# - ConfigMap para configuración
# - Secret para credenciales
# - HPA para escalado automático
```

### Ejercicio 2: Configurar Ingress con TLS

Implementa un ingress con certificados TLS:

```yaml
# Incluye:
# - Ingress con múltiples hosts
# - Certificados TLS automáticos
# - Rate limiting
# - CORS configuration
```

---

## 🔍 Casos de Uso Reales

### 1. Deployment de Microservicios

```yaml
# k8s/microservices-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: users-service
  namespace: production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: users-service
  template:
    metadata:
      labels:
        app: users-service
    spec:
      serviceAccountName: myapp-service-account
      containers:
      - name: users-service
        image: myapp/users-service:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__UsersDb
          valueFrom:
            secretKeyRef:
              name: users-secrets
              key: connection-string
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
```

### 2. Service Mesh con Istio

```yaml
# k8s/istio-virtual-service.yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: myapp-vs
  namespace: production
spec:
  hosts:
  - api.myapp.com
  gateways:
  - myapp-gateway
  http:
  - match:
    - uri:
        prefix: /api/users
    route:
    - destination:
        host: users-service
        port:
          number: 8080
  - match:
    - uri:
        prefix: /api/orders
    route:
    - destination:
        host: orders-service
        port:
          number: 8080
  - route:
    - destination:
        host: api-gateway
        port:
          number: 80
```

---

## 📊 Métricas de Kubernetes

### KPIs de Orquestación

1. **Pod Availability**: Disponibilidad de pods
2. **Resource Utilization**: Utilización de CPU y memoria
3. **Scaling Events**: Eventos de escalado automático
4. **Deployment Success Rate**: Tasa de éxito de deployments
5. **Service Response Time**: Tiempo de respuesta de servicios

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Manifests de Kubernetes**: Creación de recursos de Kubernetes
✅ **Deployments y Servicios**: Configuración de aplicaciones y exposición
✅ **ConfigMaps y Secrets**: Gestión de configuración y datos sensibles
✅ **Horizontal Pod Autoscaler**: Escalado automático de aplicaciones
✅ **RBAC y Seguridad**: Control de acceso y permisos

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **CI/CD y Pipelines**
- GitHub Actions para .NET
- Azure DevOps pipelines
- Testing automatizado en CI/CD

---

## 🔗 Enlaces de Referencia

- [Kubernetes for .NET](https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
- [Istio Service Mesh](https://istio.io/docs/)


