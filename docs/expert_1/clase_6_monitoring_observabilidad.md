# 📊 Clase 6: Monitoring y Observabilidad

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 5: Infrastructure as Code con Terraform](../expert_1/clase_5_infrastructure_as_code_terraform.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 7: Security en CI/CD](../expert_1/clase_7_security_cicd.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** Prometheus y Grafana setup
2. **Configurar** application metrics y custom dashboards
3. **Establecer** alerting rules y notification channels
4. **Implementar** log aggregation con ELK Stack
5. **Optimizar** observabilidad completa

---

## 📈 **Prometheus y Grafana Setup**

### **Prometheus Configuration**

```yaml
# monitoring/prometheus/prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'mussikon-cluster'
    environment: 'production'

rule_files:
  - "rules/*.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  # Kubernetes API Server
  - job_name: 'kubernetes-apiservers'
    kubernetes_sd_configs:
    - role: endpoints
    scheme: https
    tls_config:
      ca_file: /var/run/secrets/kubernetes.io/serviceaccount/ca.crt
    bearer_token_file: /var/run/secrets/kubernetes.io/serviceaccount/token
    relabel_configs:
    - source_labels: [__meta_kubernetes_namespace, __meta_kubernetes_service_name, __meta_kubernetes_endpoint_port_name]
      action: keep
      regex: default;kubernetes;https

  # Kubernetes Nodes
  - job_name: 'kubernetes-nodes'
    kubernetes_sd_configs:
    - role: node
    scheme: https
    tls_config:
      ca_file: /var/run/secrets/kubernetes.io/serviceaccount/ca.crt
    bearer_token_file: /var/run/secrets/kubernetes.io/serviceaccount/token
    relabel_configs:
    - action: labelmap
      regex: __meta_kubernetes_node_label_(.+)
    - target_label: __address__
      replacement: kubernetes.default.svc:443
    - source_labels: [__meta_kubernetes_node_name]
      regex: (.+)
      target_label: __metrics_path__
      replacement: /api/v1/nodes/${1}/proxy/metrics

  # Kubernetes Pods
  - job_name: 'kubernetes-pods'
    kubernetes_sd_configs:
    - role: pod
    relabel_configs:
    - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
      action: keep
      regex: true
    - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
      action: replace
      target_label: __metrics_path__
      regex: (.+)
    - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
      action: replace
      regex: ([^:]+)(?::\d+)?;(\d+)
      replacement: $1:$2
      target_label: __address__
    - action: labelmap
      regex: __meta_kubernetes_pod_label_(.+)
    - source_labels: [__meta_kubernetes_namespace]
      action: replace
      target_label: kubernetes_namespace
    - source_labels: [__meta_kubernetes_pod_name]
      action: replace
      target_label: kubernetes_pod_name

  # MussikOn API
  - job_name: 'mussikon-api'
    static_configs:
    - targets: ['mussikon-api-service:80']
    metrics_path: '/metrics'
    scrape_interval: 10s
    scrape_timeout: 5s

  # MussikOn Web
  - job_name: 'mussikon-web'
    static_configs:
    - targets: ['mussikon-web-service:80']
    metrics_path: '/metrics'
    scrape_interval: 10s
    scrape_timeout: 5s

  # Redis
  - job_name: 'redis'
    static_configs:
    - targets: ['redis-exporter:9121']

  # PostgreSQL
  - job_name: 'postgresql'
    static_configs:
    - targets: ['postgres-exporter:9187']

  # Node Exporter
  - job_name: 'node-exporter'
    kubernetes_sd_configs:
    - role: endpoints
    relabel_configs:
    - source_labels: [__meta_kubernetes_endpoints_name]
      regex: 'node-exporter'
      action: keep
```

### **Prometheus Deployment**

```yaml
# k8s/monitoring/prometheus-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: prometheus
  namespace: monitoring
  labels:
    app: prometheus
spec:
  replicas: 1
  selector:
    matchLabels:
      app: prometheus
  template:
    metadata:
      labels:
        app: prometheus
    spec:
      serviceAccountName: prometheus
      containers:
      - name: prometheus
        image: prom/prometheus:v2.45.0
        args:
          - '--config.file=/etc/prometheus/prometheus.yml'
          - '--storage.tsdb.path=/prometheus/'
          - '--web.console.libraries=/etc/prometheus/console_libraries'
          - '--web.console.templates=/etc/prometheus/consoles'
          - '--storage.tsdb.retention.time=200h'
          - '--web.enable-lifecycle'
          - '--web.enable-admin-api'
        ports:
        - containerPort: 9090
        resources:
          requests:
            cpu: 500m
            memory: 500M
          limits:
            cpu: 1000m
            memory: 1Gi
        volumeMounts:
        - name: prometheus-config-volume
          mountPath: /etc/prometheus/
        - name: prometheus-storage-volume
          mountPath: /prometheus/
      volumes:
      - name: prometheus-config-volume
        configMap:
          defaultMode: 420
          name: prometheus-config
      - name: prometheus-storage-volume
        persistentVolumeClaim:
          claimName: prometheus-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: prometheus
  namespace: monitoring
  labels:
    app: prometheus
spec:
  selector:
    app: prometheus
  ports:
  - name: web
    port: 9090
    targetPort: 9090
  type: ClusterIP
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: prometheus-pvc
  namespace: monitoring
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
```

### **Grafana Configuration**

```yaml
# k8s/monitoring/grafana-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: grafana
  namespace: monitoring
  labels:
    app: grafana
spec:
  replicas: 1
  selector:
    matchLabels:
      app: grafana
  template:
    metadata:
      labels:
        app: grafana
    spec:
      containers:
      - name: grafana
        image: grafana/grafana:10.0.0
        ports:
        - containerPort: 3000
        env:
        - name: GF_SECURITY_ADMIN_PASSWORD
          valueFrom:
            secretKeyRef:
              name: grafana-secrets
              key: admin-password
        - name: GF_INSTALL_PLUGINS
          value: "grafana-piechart-panel,grafana-worldmap-panel"
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        volumeMounts:
        - name: grafana-storage
          mountPath: /var/lib/grafana
        - name: grafana-config
          mountPath: /etc/grafana/provisioning/datasources
        - name: grafana-dashboards
          mountPath: /etc/grafana/provisioning/dashboards
      volumes:
      - name: grafana-storage
        persistentVolumeClaim:
          claimName: grafana-pvc
      - name: grafana-config
        configMap:
          name: grafana-datasources
      - name: grafana-dashboards
        configMap:
          name: grafana-dashboards
---
apiVersion: v1
kind: Service
metadata:
  name: grafana
  namespace: monitoring
  labels:
    app: grafana
spec:
  selector:
    app: grafana
  ports:
  - name: web
    port: 3000
    targetPort: 3000
  type: LoadBalancer
```

---

## 📊 **Application Metrics y Custom Dashboards**

### **.NET Application Metrics**

```csharp
// MusicalMatching.API/Extensions/MetricsExtensions.cs
using Prometheus;

namespace MusicalMatching.API.Extensions;

public static class MetricsExtensions
{
    private static readonly Counter RequestCounter = Metrics
        .CreateCounter("http_requests_total", "Total HTTP requests", new[] { "method", "endpoint", "status_code" });

    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("http_request_duration_seconds", "HTTP request duration", new[] { "method", "endpoint" });

    private static readonly Gauge ActiveConnections = Metrics
        .CreateGauge("active_connections", "Number of active connections");

    private static readonly Counter BusinessEvents = Metrics
        .CreateCounter("business_events_total", "Total business events", new[] { "event_type", "status" });

    private static readonly Gauge DatabaseConnections = Metrics
        .CreateGauge("database_connections_active", "Active database connections");

    private static readonly Histogram DatabaseQueryDuration = Metrics
        .CreateHistogram("database_query_duration_seconds", "Database query duration", new[] { "query_type" });

    public static void ConfigureMetrics(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsService, MetricsService>();
    }

    public static void UseMetrics(this IApplicationBuilder app)
    {
        app.UseHttpMetrics(options =>
        {
            options.AddCustomLabel("endpoint", context => context.Request.Path.Value);
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics();
        });
    }
}

public interface IMetricsService
{
    void IncrementRequest(string method, string endpoint, int statusCode);
    void RecordRequestDuration(string method, string endpoint, double duration);
    void SetActiveConnections(int count);
    void IncrementBusinessEvent(string eventType, string status);
    void SetDatabaseConnections(int count);
    void RecordDatabaseQuery(string queryType, double duration);
}

public class MetricsService : IMetricsService
{
    public void IncrementRequest(string method, string endpoint, int statusCode)
    {
        RequestCounter.WithLabels(method, endpoint, statusCode.ToString()).Inc();
    }

    public void RecordRequestDuration(string method, string endpoint, double duration)
    {
        RequestDuration.WithLabels(method, endpoint).Observe(duration);
    }

    public void SetActiveConnections(int count)
    {
        ActiveConnections.Set(count);
    }

    public void IncrementBusinessEvent(string eventType, string status)
    {
        BusinessEvents.WithLabels(eventType, status).Inc();
    }

    public void SetDatabaseConnections(int count)
    {
        DatabaseConnections.Set(count);
    }

    public void RecordDatabaseQuery(string queryType, double duration)
    {
        DatabaseQueryDuration.WithLabels(queryType).Observe(duration);
    }
}
```

### **Custom Metrics Middleware**

```csharp
// MusicalMatching.API/Middleware/MetricsMiddleware.cs
namespace MusicalMatching.API.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var endpoint = context.Request.Path.Value ?? "";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.Elapsed.TotalSeconds;

            _metricsService.IncrementRequest(method, endpoint, statusCode);
            _metricsService.RecordRequestDuration(method, endpoint, duration);

            _logger.LogInformation("Request {Method} {Endpoint} completed with status {StatusCode} in {Duration}ms",
                method, endpoint, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### **Grafana Dashboard Configuration**

```json
{
  "dashboard": {
    "id": null,
    "title": "MussikOn Application Metrics",
    "tags": ["mussikon", "application"],
    "timezone": "browser",
    "panels": [
      {
        "id": 1,
        "title": "HTTP Requests per Second",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{endpoint}}"
          }
        ],
        "yAxes": [
          {
            "label": "Requests/sec",
            "min": 0
          }
        ],
        "xAxes": [
          {
            "type": "time"
          }
        ]
      },
      {
        "id": 2,
        "title": "HTTP Request Duration",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          },
          {
            "expr": "histogram_quantile(0.50, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "50th percentile"
          }
        ],
        "yAxes": [
          {
            "label": "Duration (seconds)",
            "min": 0
          }
        ]
      },
      {
        "id": 3,
        "title": "Active Connections",
        "type": "singlestat",
        "targets": [
          {
            "expr": "active_connections",
            "legendFormat": "Active Connections"
          }
        ],
        "valueName": "current"
      },
      {
        "id": 4,
        "title": "Business Events",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(business_events_total[5m])",
            "legendFormat": "{{event_type}} - {{status}}"
          }
        ]
      },
      {
        "id": 5,
        "title": "Database Connections",
        "type": "singlestat",
        "targets": [
          {
            "expr": "database_connections_active",
            "legendFormat": "DB Connections"
          }
        ]
      },
      {
        "id": 6,
        "title": "Database Query Duration",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(database_query_duration_seconds_bucket[5m]))",
            "legendFormat": "{{query_type}} - 95th percentile"
          }
        ]
      }
    ],
    "time": {
      "from": "now-1h",
      "to": "now"
    },
    "refresh": "30s"
  }
}
```

---

## 🚨 **Alerting Rules y Notification Channels**

### **Prometheus Alert Rules**

```yaml
# monitoring/prometheus/rules/alerts.yml
groups:
- name: mussikon-alerts
  rules:
  # High Error Rate
  - alert: HighErrorRate
    expr: rate(http_requests_total{status_code=~"5.."}[5m]) / rate(http_requests_total[5m]) > 0.05
    for: 2m
    labels:
      severity: critical
      service: mussikon-api
    annotations:
      summary: "High error rate detected"
      description: "Error rate is {{ $value | humanizePercentage }} for the last 5 minutes"

  # High Response Time
  - alert: HighResponseTime
    expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
    for: 5m
    labels:
      severity: warning
      service: mussikon-api
    annotations:
      summary: "High response time detected"
      description: "95th percentile response time is {{ $value }}s"

  # High Memory Usage
  - alert: HighMemoryUsage
    expr: (container_memory_usage_bytes / container_spec_memory_limit_bytes) > 0.8
    for: 5m
    labels:
      severity: warning
      service: mussikon-api
    annotations:
      summary: "High memory usage detected"
      description: "Memory usage is {{ $value | humanizePercentage }}"

  # High CPU Usage
  - alert: HighCPUUsage
    expr: rate(container_cpu_usage_seconds_total[5m]) > 0.8
    for: 5m
    labels:
      severity: warning
      service: mussikon-api
    annotations:
      summary: "High CPU usage detected"
      description: "CPU usage is {{ $value | humanizePercentage }}"

  # Database Connection Issues
  - alert: DatabaseConnectionIssues
    expr: database_connections_active == 0
    for: 1m
    labels:
      severity: critical
      service: database
    annotations:
      summary: "Database connection issues"
      description: "No active database connections"

  # Redis Connection Issues
  - alert: RedisConnectionIssues
    expr: redis_up == 0
    for: 1m
    labels:
      severity: critical
      service: redis
    annotations:
      summary: "Redis connection issues"
      description: "Redis is not responding"

  # Pod CrashLoopBackOff
  - alert: PodCrashLoopBackOff
    expr: kube_pod_status_phase{phase="Running"} == 0 and kube_pod_status_phase{phase="Pending"} == 0
    for: 5m
    labels:
      severity: critical
      service: kubernetes
    annotations:
      summary: "Pod in CrashLoopBackOff"
      description: "Pod {{ $labels.pod }} is in CrashLoopBackOff state"

  # Node Not Ready
  - alert: NodeNotReady
    expr: kube_node_status_condition{condition="Ready",status="true"} == 0
    for: 5m
    labels:
      severity: critical
      service: kubernetes
    annotations:
      summary: "Node not ready"
      description: "Node {{ $labels.node }} is not ready"
```

### **Alertmanager Configuration**

```yaml
# monitoring/alertmanager/alertmanager.yml
global:
  smtp_smarthost: 'localhost:587'
  smtp_from: 'alerts@mussikon.com'
  smtp_auth_username: 'alerts@mussikon.com'
  smtp_auth_password: 'password'

route:
  group_by: ['alertname', 'cluster', 'service']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'web.hook'
  routes:
  - match:
      severity: critical
    receiver: 'critical-alerts'
  - match:
      severity: warning
    receiver: 'warning-alerts'

receivers:
- name: 'web.hook'
  webhook_configs:
  - url: 'http://webhook:5001/'

- name: 'critical-alerts'
  email_configs:
  - to: 'oncall@mussikon.com'
    subject: '[CRITICAL] {{ .GroupLabels.alertname }}'
    body: |
      {{ range .Alerts }}
      Alert: {{ .Annotations.summary }}
      Description: {{ .Annotations.description }}
      {{ end }}
  slack_configs:
  - api_url: 'https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK'
    channel: '#alerts-critical'
    title: 'Critical Alert'
    text: '{{ range .Alerts }}{{ .Annotations.summary }}{{ end }}'

- name: 'warning-alerts'
  email_configs:
  - to: 'team@mussikon.com'
    subject: '[WARNING] {{ .GroupLabels.alertname }}'
    body: |
      {{ range .Alerts }}
      Alert: {{ .Annotations.summary }}
      Description: {{ .Annotations.description }}
      {{ end }}
  slack_configs:
  - api_url: 'https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK'
    channel: '#alerts-warning'
    title: 'Warning Alert'
    text: '{{ range .Alerts }}{{ .Annotations.summary }}{{ end }}'
```

---

## 📝 **Log Aggregation con ELK Stack**

### **Elasticsearch Configuration**

```yaml
# k8s/monitoring/elasticsearch-deployment.yml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: elasticsearch
  namespace: monitoring
spec:
  serviceName: elasticsearch
  replicas: 3
  selector:
    matchLabels:
      app: elasticsearch
  template:
    metadata:
      labels:
        app: elasticsearch
    spec:
      containers:
      - name: elasticsearch
        image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
        env:
        - name: cluster.name
          value: "mussikon-cluster"
        - name: node.name
          valueFrom:
            fieldRef:
              fieldPath: metadata.name
        - name: discovery.seed_hosts
          value: "elasticsearch-0.elasticsearch,elasticsearch-1.elasticsearch,elasticsearch-2.elasticsearch"
        - name: cluster.initial_master_nodes
          value: "elasticsearch-0,elasticsearch-1,elasticsearch-2"
        - name: ES_JAVA_OPTS
          value: "-Xms512m -Xmx512m"
        - name: xpack.security.enabled
          value: "false"
        ports:
        - containerPort: 9200
        - containerPort: 9300
        resources:
          requests:
            cpu: 500m
            memory: 1Gi
          limits:
            cpu: 1000m
            memory: 2Gi
        volumeMounts:
        - name: data
          mountPath: /usr/share/elasticsearch/data
  volumeClaimTemplates:
  - metadata:
      name: data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
---
apiVersion: v1
kind: Service
metadata:
  name: elasticsearch
  namespace: monitoring
spec:
  selector:
    app: elasticsearch
  ports:
  - port: 9200
    name: http
  - port: 9300
    name: transport
  clusterIP: None
```

### **Logstash Configuration**

```yaml
# k8s/monitoring/logstash-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: logstash
  namespace: monitoring
spec:
  replicas: 2
  selector:
    matchLabels:
      app: logstash
  template:
    metadata:
      labels:
        app: logstash
    spec:
      containers:
      - name: logstash
        image: docker.elastic.co/logstash/logstash:8.11.0
        env:
        - name: LS_JAVA_OPTS
          value: "-Xmx512m -Xms512m"
        ports:
        - containerPort: 5044
        - containerPort: 9600
        resources:
          requests:
            cpu: 500m
            memory: 1Gi
          limits:
            cpu: 1000m
            memory: 2Gi
        volumeMounts:
        - name: config
          mountPath: /usr/share/logstash/pipeline
      volumes:
      - name: config
        configMap:
          name: logstash-config
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: logstash-config
  namespace: monitoring
data:
  logstash.conf: |
    input {
      beats {
        port => 5044
      }
    }
    
    filter {
      if [fields][service] == "mussikon-api" {
        grok {
          match => { "message" => "%{TIMESTAMP_ISO8601:timestamp} \[%{LOGLEVEL:level}\] %{GREEDYDATA:message}" }
        }
        
        date {
          match => [ "timestamp", "ISO8601" ]
        }
        
        if [message] =~ /Request.*completed/ {
          grok {
            match => { "message" => "Request %{WORD:method} %{URIPATH:endpoint} completed with status %{INT:status_code} in %{INT:duration}ms" }
          }
        }
      }
    }
    
    output {
      elasticsearch {
        hosts => ["elasticsearch:9200"]
        index => "mussikon-logs-%{+YYYY.MM.dd}"
      }
    }
```

### **Kibana Configuration**

```yaml
# k8s/monitoring/kibana-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: kibana
  namespace: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: kibana
  template:
    metadata:
      labels:
        app: kibana
    spec:
      containers:
      - name: kibana
        image: docker.elastic.co/kibana/kibana:8.11.0
        env:
        - name: ELASTICSEARCH_HOSTS
          value: "http://elasticsearch:9200"
        ports:
        - containerPort: 5601
        resources:
          requests:
            cpu: 100m
            memory: 256Mi
          limits:
            cpu: 500m
            memory: 512Mi
---
apiVersion: v1
kind: Service
metadata:
  name: kibana
  namespace: monitoring
spec:
  selector:
    app: kibana
  ports:
  - port: 5601
    targetPort: 5601
  type: LoadBalancer
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Prometheus Setup**
```yaml
# Configura Prometheus y Grafana
# para monitorear la aplicación MussikOn
```

### **Ejercicio 2: Custom Metrics**
```csharp
// Implementa métricas personalizadas
// en la aplicación .NET
```

### **Ejercicio 3: Alerting Rules**
```yaml
# Configura reglas de alerta
# para monitoreo proactivo
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📈 Prometheus y Grafana**: Setup y configuración
2. **📊 Application Metrics**: Métricas personalizadas
3. **🚨 Alerting Rules**: Reglas de alerta
4. **📝 ELK Stack**: Agregación de logs
5. **📊 Dashboards**: Visualización de métricas
6. **🔔 Notifications**: Canales de notificación

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Security en CI/CD**, implementando escaneo de seguridad.

---

**¡Has completado la sexta clase del Expert Level 1! 📊🎯**
