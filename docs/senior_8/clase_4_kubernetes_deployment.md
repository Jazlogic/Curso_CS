# ☸️ Clase 4: Kubernetes Deployment

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 3: CI/CD con GitHub Actions](../senior_8/clase_3_cicd_github_actions.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 5: Monitoreo y Observabilidad](../senior_8/clase_5_monitoreo_observabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** deployment en Kubernetes
2. **Configurar** servicios y ingress
3. **Desarrollar** auto-scaling y health checks
4. **Aplicar** secrets y configmaps
5. **Optimizar** recursos y networking

---

## ☸️ **Deployment Principal**

### **Deployment de la API**

```yaml
# k8s/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: musical-matching-api
  namespace: musical-matching
  labels:
    app: musical-matching-api
    version: v1.0.0
spec:
  replicas: 3
  selector:
    matchLabels:
      app: musical-matching-api
  template:
    metadata:
      labels:
        app: musical-matching-api
        version: v1.0.0
    spec:
      containers:
      - name: api
        image: ghcr.io/your-org/musical-matching:latest
        ports:
        - containerPort: 80
          name: http
        - containerPort: 443
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: jwt-secret
              key: secret-key
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
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
        volumeMounts:
        - name: logs
          mountPath: /app/logs
      volumes:
      - name: logs
        emptyDir: {}
      imagePullSecrets:
      - name: ghcr-secret
```

---

## 🔌 **Service y Ingress**

### **Service para la API**

```yaml
# k8s/service.yml
apiVersion: v1
kind: Service
metadata:
  name: musical-matching-api-service
  namespace: musical-matching
spec:
  selector:
    app: musical-matching-api
  ports:
  - name: http
    port: 80
    targetPort: 80
  - name: https
    port: 443
    targetPort: 443
  type: ClusterIP
```

### **Ingress para Routing**

```yaml
# k8s/ingress.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: musical-matching-ingress
  namespace: musical-matching
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.musicalmatching.com
    secretName: musical-matching-tls
  rules:
  - host: api.musicalmatching.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: musical-matching-api-service
            port:
              number: 80
```

---

## 📊 **Auto-Scaling y HPA**

### **Horizontal Pod Autoscaler**

```yaml
# k8s/hpa.yml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: musical-matching-api-hpa
  namespace: musical-matching
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: musical-matching-api
  minReplicas: 3
  maxReplicas: 10
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
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 100
        periodSeconds: 15
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
```

---

## 🔐 **Secrets y ConfigMaps**

### **Secrets para Datos Sensibles**

```yaml
# k8s/secrets.yml
apiVersion: v1
kind: Secret
metadata:
  name: db-secret
  namespace: musical-matching
type: Opaque
data:
  connection-string: U2VydmVyPWRiO0RhdGFiYXNlPU11c2ljYWxNYXRjaGluZztVc2VyPXNhO1Bhc3N3b3JkPVlvdXJfcGFzc3dvcmQxMjMhO1RydXN0U2VydmVyQ2VydGlmaWNhdGU9dHJ1ZQ==
  password: WW91cl9wYXNzd29yZDEyMyE=
---
apiVersion: v1
kind: Secret
metadata:
  name: jwt-secret
  namespace: musical-matching
type: Opaque
data:
  secret-key: WW91clN1cGVyU2VjcmV0SldUS2V5MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MA==
```

### **ConfigMap para Configuración**

```yaml
# k8s/configmap.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
  namespace: musical-matching
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
        "Redis": "redis:6379"
      },
      "JwtSettings": {
        "Issuer": "MusicalMatching.Production",
        "Audience": "MusicalMatching.Production",
        "ExpiryMinutes": 60
      }
    }
```

---

## 🏥 **Health Checks y Liveness**

### **Health Check Personalizado**

```yaml
# k8s/health-check.yml
apiVersion: v1
kind: Pod
metadata:
  name: health-check-pod
  namespace: musical-matching
spec:
  containers:
  - name: health-check
    image: busybox
    command: ['sh', '-c', 'while true; do sleep 30; done']
    livenessProbe:
      exec:
        command:
        - cat
        - /tmp/healthy
      initialDelaySeconds: 5
      periodSeconds: 5
    readinessProbe:
      exec:
        command:
        - cat
        - /tmp/healthy
      initialDelaySeconds: 5
      periodSeconds: 5
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Deployment Básico**
```yaml
# Implementa:
# - Deployment con health checks
# - Service y Ingress
# - Configuración de recursos
# - Variables de entorno
```

### **Ejercicio 2: Auto-Scaling**
```yaml
# Crea:
# - HPA para escalabilidad automática
# - Métricas de CPU y memoria
# - Políticas de escalado
# - Ventanas de estabilización
```

### **Ejercicio 3: Seguridad y Configuración**
```yaml
# Implementa:
# - Secrets para datos sensibles
# - ConfigMaps para configuración
# - RBAC y políticas de seguridad
# - Network policies
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **☸️ Deployment Principal**: Configuración completa de la aplicación
2. **🔌 Service y Ingress**: Networking y routing
3. **📊 Auto-Scaling**: HPA para escalabilidad automática
4. **🔐 Secrets y ConfigMaps**: Gestión de configuración y secretos
5. **🏥 Health Checks**: Monitoreo de salud de los pods

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Monitoreo y Observabilidad**, implementando métricas, logging y tracing en Kubernetes.

---

**¡Has completado la cuarta clase del Módulo 15! ☸️🔌**


