# ☸️ Clase 4: Kubernetes y Orquestación

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 3: Docker y Containerización Avanzada](../expert_1/clase_3_docker_containerizacion_avanzada.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 5: Infrastructure as Code con Terraform](../expert_1/clase_5_infrastructure_as_code_terraform.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Dominar** Kubernetes fundamentals
2. **Implementar** Deployments, Services y Ingress
3. **Configurar** ConfigMaps y Secrets
4. **Gestionar** Helm charts y package management
5. **Optimizar** auto-scaling y resource management

---

## ☸️ **Kubernetes Fundamentals**

### **Cluster Architecture**

```yaml
# k8s/cluster-setup.yml
apiVersion: v1
kind: Namespace
metadata:
  name: mussikon
  labels:
    name: mussikon
    environment: production
---
apiVersion: v1
kind: ResourceQuota
metadata:
  name: mussikon-quota
  namespace: mussikon
spec:
  hard:
    requests.cpu: "4"
    requests.memory: 8Gi
    limits.cpu: "8"
    limits.memory: 16Gi
    persistentvolumeclaims: "10"
    pods: "20"
    services: "10"
```

### **Node Configuration**

```yaml
# k8s/node-config.yml
apiVersion: v1
kind: Node
metadata:
  name: worker-node-1
  labels:
    node-type: worker
    zone: us-east-1a
    instance-type: t3.medium
spec:
  taints:
    - key: "node-type"
      value: "worker"
      effect: "NoSchedule"
```

---

## 🚀 **Deployments y Services**

### **API Deployment**

```yaml
# k8s/api-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mussikon-api
  namespace: mussikon
  labels:
    app: mussikon-api
    version: v1.0.0
spec:
  replicas: 3
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
        version: v1.0.0
    spec:
      containers:
      - name: api
        image: mussikon/api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: mussikon-secrets
              key: database-connection
        - name: Redis__ConnectionString
          valueFrom:
            configMapKeyRef:
              name: mussikon-config
              key: redis-connection
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
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
      nodeSelector:
        node-type: worker
      tolerations:
      - key: "node-type"
        operator: "Equal"
        value: "worker"
        effect: "NoSchedule"
```

### **API Service**

```yaml
# k8s/api-service.yml
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-service
  namespace: mussikon
  labels:
    app: mussikon-api
spec:
  selector:
    app: mussikon-api
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
  type: ClusterIP
---
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-loadbalancer
  namespace: mussikon
  labels:
    app: mussikon-api
spec:
  selector:
    app: mussikon-api
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
  type: LoadBalancer
  loadBalancerSourceRanges:
  - 10.0.0.0/8
  - 172.16.0.0/12
  - 192.168.0.0/16
```

### **Web Frontend Deployment**

```yaml
# k8s/web-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mussikon-web
  namespace: mussikon
  labels:
    app: mussikon-web
    version: v1.0.0
spec:
  replicas: 2
  selector:
    matchLabels:
      app: mussikon-web
  template:
    metadata:
      labels:
        app: mussikon-web
        version: v1.0.0
    spec:
      containers:
      - name: web
        image: mussikon/web:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: API__BaseUrl
          value: "http://mussikon-api-service:80"
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## 🌐 **Ingress y Load Balancing**

### **Ingress Configuration**

```yaml
# k8s/ingress.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: mussikon-ingress
  namespace: mussikon
  annotations:
    kubernetes.io/ingress.class: "nginx"
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/rate-limit-window: "1m"
    nginx.ingress.kubernetes.io/proxy-body-size: "10m"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "300"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "300"
spec:
  tls:
  - hosts:
    - api.mussikon.com
    - www.mussikon.com
    secretName: mussikon-tls
  rules:
  - host: api.mussikon.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: mussikon-api-service
            port:
              number: 80
  - host: www.mussikon.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: mussikon-web-service
            port:
              number: 80
```

### **Ingress Controller**

```yaml
# k8s/nginx-ingress-controller.yml
apiVersion: v1
kind: Namespace
metadata:
  name: ingress-nginx
---
apiVersion: helm.cattle.io/v1
kind: HelmChart
metadata:
  name: nginx-ingress
  namespace: ingress-nginx
spec:
  chart: nginx-ingress
  repo: https://kubernetes.github.io/ingress-nginx
  targetNamespace: ingress-nginx
  valuesContent: |-
    controller:
      service:
        type: LoadBalancer
        annotations:
          service.beta.kubernetes.io/aws-load-balancer-type: nlb
      config:
        use-forwarded-headers: "true"
        compute-full-forwarded-for: "true"
        use-proxy-protocol: "false"
      resources:
        requests:
          cpu: 100m
          memory: 90Mi
        limits:
          cpu: 500m
          memory: 512Mi
      nodeSelector:
        node-type: worker
      tolerations:
      - key: "node-type"
        operator: "Equal"
        value: "worker"
        effect: "NoSchedule"
```

---

## 🔧 **ConfigMaps y Secrets**

### **ConfigMap**

```yaml
# k8s/configmap.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mussikon-config
  namespace: mussikon
data:
  redis-connection: "redis-service:6379"
  rabbitmq-connection: "rabbitmq-service:5672"
  elasticsearch-connection: "elasticsearch-service:9200"
  app-settings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Warning",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      },
      "AllowedHosts": "*",
      "Cors": {
        "AllowedOrigins": [
          "https://www.mussikon.com",
          "https://api.mussikon.com"
        ]
      },
      "RateLimiting": {
        "RequestsPerMinute": 100,
        "BurstLimit": 200
      }
    }
  nginx.conf: |
    upstream api_backend {
        server mussikon-api-service:80;
    }
    
    server {
        listen 80;
        server_name api.mussikon.com;
        
        location / {
            proxy_pass http://api_backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
```

### **Secrets**

```yaml
# k8s/secrets.yml
apiVersion: v1
kind: Secret
metadata:
  name: mussikon-secrets
  namespace: mussikon
type: Opaque
data:
  database-connection: U2VydmVyPWRiLXNlcnZpY2U7RGF0YWJhc2U9TXVzc2lrb247VXNlciBJZD1zYTtQYXNzd29yZD1Zb3VyU3Ryb25nQFBhc3N3MHJkO1RydXN0U2VydmVyQ2VydGlmaWNhdGU9dHJ1ZTs=
  jwt-secret: eW91ci1qd3Qtc2VjcmV0LWtleS1oZXJl
  redis-password: cmVkaXMtcGFzc3dvcmQ=
  api-key: eW91ci1hcGkta2V5LWhlcmU=
---
apiVersion: v1
kind: Secret
metadata:
  name: mussikon-tls
  namespace: mussikon
type: kubernetes.io/tls
data:
  tls.crt: LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0t...
  tls.key: LS0tLS1CRUdJTiBQUklWQVRFIEtFWS0tLS0t...
```

---

## 📦 **Helm Charts y Package Management**

### **Helm Chart Structure**

```yaml
# helm/mussikon/Chart.yaml
apiVersion: v2
name: mussikon
description: A Helm chart for MussikOn application
type: application
version: 1.0.0
appVersion: "1.0.0"
keywords:
  - music
  - matching
  - platform
home: https://mussikon.com
sources:
  - https://github.com/mussikon/mussikon
maintainers:
  - name: MussikOn Team
    email: team@mussikon.com
dependencies:
  - name: postgresql
    version: 12.1.2
    repository: https://charts.bitnami.com/bitnami
  - name: redis
    version: 17.3.7
    repository: https://charts.bitnami.com/bitnami
  - name: rabbitmq
    version: 11.1.3
    repository: https://charts.bitnami.com/bitnami
```

### **Values.yaml**

```yaml
# helm/mussikon/values.yaml
replicaCount: 3

image:
  repository: mussikon/api
  pullPolicy: IfNotPresent
  tag: "latest"

imagePullSecrets: []
nameOverride: ""
fullnameOverride: ""

service:
  type: ClusterIP
  port: 80
  targetPort: 8080

ingress:
  enabled: true
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
  hosts:
    - host: api.mussikon.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: mussikon-tls
      hosts:
        - api.mussikon.com

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 250m
    memory: 256Mi

autoscaling:
  enabled: true
  minReplicas: 3
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

nodeSelector: {}

tolerations: []

affinity: {}

# Database configuration
postgresql:
  enabled: true
  auth:
    postgresPassword: "postgres"
    username: "mussikon"
    password: "mussikon"
    database: "mussikon"
  primary:
    persistence:
      enabled: true
      size: 20Gi

# Redis configuration
redis:
  enabled: true
  auth:
    enabled: true
    password: "redis"
  master:
    persistence:
      enabled: true
      size: 8Gi

# RabbitMQ configuration
rabbitmq:
  enabled: true
  auth:
    username: "admin"
    password: "admin"
  persistence:
    enabled: true
    size: 8Gi
```

### **Deployment Template**

```yaml
# helm/mussikon/templates/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "mussikon.fullname" . }}
  labels:
    {{- include "mussikon.labels" . | nindent 4 }}
spec:
  {{- if not .Values.autoscaling.enabled }}
  replicas: {{ .Values.replicaCount }}
  {{- end }}
  selector:
    matchLabels:
      {{- include "mussikon.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      {{- with .Values.podAnnotations }}
      annotations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      labels:
        {{- include "mussikon.selectorLabels" . | nindent 8 }}
    spec:
      {{- with .Values.imagePullSecrets }}
      imagePullSecrets:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      serviceAccountName: {{ include "mussikon.serviceAccountName" . }}
      securityContext:
        {{- toYaml .Values.podSecurityContext | nindent 8 }}
      containers:
        - name: {{ .Chart.Name }}
          securityContext:
            {{- toYaml .Values.securityContext | nindent 12 }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - name: http
              containerPort: {{ .Values.service.targetPort }}
              protocol: TCP
          livenessProbe:
            httpGet:
              path: /health
              port: http
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /health/ready
              port: http
            initialDelaySeconds: 5
            periodSeconds: 5
          resources:
            {{- toYaml .Values.resources | nindent 12 }}
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: {{ include "mussikon.fullname" . }}-secrets
                  key: database-connection
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.affinity }}
      affinity:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.tolerations }}
      tolerations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
```

---

## 📈 **Auto-scaling y Resource Management**

### **Horizontal Pod Autoscaler**

```yaml
# k8s/hpa.yml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: mussikon-api-hpa
  namespace: mussikon
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: mussikon-api
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
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: "100"
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
      - type: Pods
        value: 2
        periodSeconds: 60
      selectPolicy: Max
```

### **Vertical Pod Autoscaler**

```yaml
# k8s/vpa.yml
apiVersion: autoscaling.k8s.io/v1
kind: VerticalPodAutoscaler
metadata:
  name: mussikon-api-vpa
  namespace: mussikon
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: mussikon-api
  updatePolicy:
    updateMode: "Auto"
  resourcePolicy:
    containerPolicies:
    - containerName: api
      minAllowed:
        cpu: 100m
        memory: 128Mi
      maxAllowed:
        cpu: 1000m
        memory: 1Gi
      controlledResources: ["cpu", "memory"]
```

### **Cluster Autoscaler**

```yaml
# k8s/cluster-autoscaler.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cluster-autoscaler
  namespace: kube-system
  labels:
    app: cluster-autoscaler
spec:
  replicas: 1
  selector:
    matchLabels:
      app: cluster-autoscaler
  template:
    metadata:
      labels:
        app: cluster-autoscaler
    spec:
      serviceAccountName: cluster-autoscaler
      containers:
      - image: k8s.gcr.io/autoscaling/cluster-autoscaler:v1.21.0
        name: cluster-autoscaler
        resources:
          limits:
            cpu: 100m
            memory: 300Mi
          requests:
            cpu: 100m
            memory: 300Mi
        command:
        - ./cluster-autoscaler
        - --v=4
        - --stderrthreshold=info
        - --cloud-provider=aws
        - --skip-nodes-with-local-storage=false
        - --expander=least-waste
        - --node-group-auto-discovery=asg:tag=k8s.io/cluster-autoscaler/enabled,k8s.io/cluster-autoscaler/mussikon-cluster
        - --balance-similar-node-groups
        - --scale-down-enabled=true
        - --scale-down-delay-after-add=10m
        - --scale-down-unneeded-time=10m
        env:
        - name: AWS_REGION
          value: us-east-1
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Kubernetes Deployment**
```yaml
# Crea un deployment completo para MussikOn
# con health checks y resource limits
```

### **Ejercicio 2: Helm Chart**
```yaml
# Crea un Helm chart completo
# para la aplicación MussikOn
```

### **Ejercicio 3: Auto-scaling**
```yaml
# Configura HPA y VPA
# para optimizar recursos
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **☸️ Kubernetes Fundamentals**: Arquitectura y conceptos básicos
2. **🚀 Deployments y Services**: Orquestación de aplicaciones
3. **🌐 Ingress**: Load balancing y routing
4. **🔧 ConfigMaps y Secrets**: Gestión de configuración
5. **📦 Helm Charts**: Package management
6. **📈 Auto-scaling**: Optimización de recursos

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Infrastructure as Code con Terraform**, implementando infraestructura automatizada.

---

**¡Has completado la cuarta clase del Expert Level 1! ☸️🎯**
