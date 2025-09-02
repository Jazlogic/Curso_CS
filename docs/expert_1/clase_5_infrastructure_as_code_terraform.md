# 🏗️ Clase 5: Infrastructure as Code con Terraform

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 4: Kubernetes y Orquestación](../expert_1/clase_4_kubernetes_orquestacion.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 6: Monitoring y Observabilidad](../expert_1/clase_6_monitoring_observabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Dominar** Terraform fundamentals
2. **Implementar** Azure/AWS provider configuration
3. **Configurar** state management y remote backends
4. **Crear** modules y reusable infrastructure
5. **Optimizar** infrastructure automation

---

## 🏗️ **Terraform Fundamentals**

### **Terraform Configuration Structure**

```hcl
# terraform/main.tf
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "terraformstateaccount"
    container_name       = "terraform-state"
    key                  = "mussikon.tfstate"
  }
}

# Configure providers
provider "azurerm" {
  features {}
}

provider "aws" {
  region = var.aws_region
}

provider "kubernetes" {
  host                   = azurerm_kubernetes_cluster.mussikon.kube_config.0.host
  client_certificate     = base64decode(azurerm_kubernetes_cluster.mussikon.kube_config.0.client_certificate)
  client_key             = base64decode(azurerm_kubernetes_cluster.mussikon.kube_config.0.client_key)
  cluster_ca_certificate = base64decode(azurerm_kubernetes_cluster.mussikon.kube_config.0.cluster_ca_certificate)
}
```

### **Variables Configuration**

```hcl
# terraform/variables.tf
variable "environment" {
  description = "Environment name"
  type        = string
  default     = "production"
  
  validation {
    condition     = contains(["development", "staging", "production"], var.environment)
    error_message = "Environment must be one of: development, staging, production."
  }
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "cluster_name" {
  description = "Kubernetes cluster name"
  type        = string
  default     = "mussikon-cluster"
}

variable "node_count" {
  description = "Number of nodes in the cluster"
  type        = number
  default     = 3
  
  validation {
    condition     = var.node_count >= 1 && var.node_count <= 10
    error_message = "Node count must be between 1 and 10."
  }
}

variable "vm_size" {
  description = "Size of the virtual machines"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Project     = "MussikOn"
    Environment = "production"
    ManagedBy   = "Terraform"
  }
}
```

---

## ☁️ **Azure Provider Configuration**

### **Resource Group and Networking**

```hcl
# terraform/azure/main.tf
# Resource Group
resource "azurerm_resource_group" "mussikon" {
  name     = "rg-mussikon-${var.environment}"
  location = var.location
  tags     = var.tags
}

# Virtual Network
resource "azurerm_virtual_network" "mussikon" {
  name                = "vnet-mussikon-${var.environment}"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.mussikon.location
  resource_group_name = azurerm_resource_group.mussikon.name
  tags                = var.tags
}

# Subnets
resource "azurerm_subnet" "aks" {
  name                 = "subnet-aks-${var.environment}"
  resource_group_name  = azurerm_resource_group.mussikon.name
  virtual_network_name = azurerm_virtual_network.mussikon.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_subnet" "database" {
  name                 = "subnet-database-${var.environment}"
  resource_group_name  = azurerm_resource_group.mussikon.name
  virtual_network_name = azurerm_virtual_network.mussikon.name
  address_prefixes     = ["10.0.2.0/24"]
  
  delegation {
    name = "database-delegation"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# Network Security Group
resource "azurerm_network_security_group" "mussikon" {
  name                = "nsg-mussikon-${var.environment}"
  location            = azurerm_resource_group.mussikon.location
  resource_group_name = azurerm_resource_group.mussikon.name
  tags                = var.tags

  security_rule {
    name                       = "AllowHTTPS"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "AllowHTTP"
    priority                   = 1002
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "80"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "AllowSSH"
    priority                   = 1003
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "10.0.0.0/16"
    destination_address_prefix = "*"
  }
}
```

### **Azure Kubernetes Service**

```hcl
# terraform/azure/aks.tf
# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "mussikon" {
  name                = "law-mussikon-${var.environment}"
  location            = azurerm_resource_group.mussikon.location
  resource_group_name = azurerm_resource_group.mussikon.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

# AKS Cluster
resource "azurerm_kubernetes_cluster" "mussikon" {
  name                = var.cluster_name
  location            = azurerm_resource_group.mussikon.location
  resource_group_name = azurerm_resource_group.mussikon.name
  dns_prefix          = "mussikon-${var.environment}"
  kubernetes_version  = "1.28"

  default_node_pool {
    name                = "system"
    vm_size             = var.vm_size
    node_count          = var.node_count
    vnet_subnet_id      = azurerm_subnet.aks.id
    enable_auto_scaling = true
    min_count           = 1
    max_count           = 10
    max_pods            = 30
    
    upgrade_settings {
      max_surge = "1"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "standard"
    service_cidr      = "10.1.0.0/16"
    dns_service_ip    = "10.1.0.10"
  }

  oms_agent {
    log_analytics_workspace_id = azurerm_log_analytics_workspace.mussikon.id
  }

  azure_policy_enabled = true

  tags = var.tags
}

# Additional Node Pool for Applications
resource "azurerm_kubernetes_cluster_node_pool" "apps" {
  name                  = "apps"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.mussikon.id
  vm_size               = "Standard_D4s_v3"
  node_count            = 2
  vnet_subnet_id        = azurerm_subnet.aks.id
  enable_auto_scaling   = true
  min_count             = 1
  max_count             = 5
  max_pods              = 30

  node_taints = [
    "node-type=apps:NoSchedule"
  ]

  node_labels = {
    "node-type" = "apps"
  }

  tags = var.tags
}
```

### **Azure Database and Storage**

```hcl
# terraform/azure/database.tf
# PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "mussikon" {
  name                   = "psql-mussikon-${var.environment}"
  resource_group_name    = azurerm_resource_group.mussikon.name
  location               = azurerm_resource_group.mussikon.location
  version                = "13"
  administrator_login    = "mussikonadmin"
  administrator_password = var.database_password
  zone                   = "1"
  
  storage_mb = 32768
  sku_name   = "GP_Standard_D2s_v3"

  backup_retention_days        = 7
  geo_redundant_backup_enabled = false

  high_availability {
    mode = "ZoneRedundant"
  }

  maintenance_window {
    day_of_week  = 0
    start_hour   = 8
    start_minute = 0
  }

  depends_on = [azurerm_private_dns_zone_virtual_network_link.mussikon]
}

# PostgreSQL Database
resource "azurerm_postgresql_flexible_server_database" "mussikon" {
  name      = "mussikon"
  server_id = azurerm_postgresql_flexible_server.mussikon.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Private DNS Zone
resource "azurerm_private_dns_zone" "mussikon" {
  name                = "mussikon.postgres.database.azure.com"
  resource_group_name = azurerm_resource_group.mussikon.name
}

resource "azurerm_private_dns_zone_virtual_network_link" "mussikon" {
  name                  = "mussikon-vnet-link"
  private_dns_zone_name = azurerm_private_dns_zone.mussikon.name
  virtual_network_id    = azurerm_virtual_network.mussikon.id
  resource_group_name   = azurerm_resource_group.mussikon.name
}

# Storage Account
resource "azurerm_storage_account" "mussikon" {
  name                     = "stmussikon${var.environment}"
  resource_group_name      = azurerm_resource_group.mussikon.name
  location                 = azurerm_resource_group.mussikon.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["GET", "POST", "PUT", "DELETE"]
      allowed_origins    = ["*"]
      exposed_headers    = ["*"]
      max_age_in_seconds = 200
    }
  }

  tags = var.tags
}

# Storage Container
resource "azurerm_storage_container" "mussikon" {
  name                  = "mussikon-files"
  storage_account_name  = azurerm_storage_account.mussikon.name
  container_access_type = "private"
}
```

---

## 🌐 **AWS Provider Configuration**

### **VPC and Networking**

```hcl
# terraform/aws/main.tf
# VPC
resource "aws_vpc" "mussikon" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = merge(var.tags, {
    Name = "vpc-mussikon-${var.environment}"
  })
}

# Internet Gateway
resource "aws_internet_gateway" "mussikon" {
  vpc_id = aws_vpc.mussikon.id

  tags = merge(var.tags, {
    Name = "igw-mussikon-${var.environment}"
  })
}

# Public Subnets
resource "aws_subnet" "public" {
  count = 2

  vpc_id                  = aws_vpc.mussikon.id
  cidr_block              = "10.0.${count.index + 1}.0/24"
  availability_zone       = data.aws_availability_zones.available.names[count.index]
  map_public_ip_on_launch = true

  tags = merge(var.tags, {
    Name = "subnet-public-${count.index + 1}-${var.environment}"
    Type = "Public"
  })
}

# Private Subnets
resource "aws_subnet" "private" {
  count = 2

  vpc_id            = aws_vpc.mussikon.id
  cidr_block        = "10.0.${count.index + 10}.0/24"
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = merge(var.tags, {
    Name = "subnet-private-${count.index + 1}-${var.environment}"
    Type = "Private"
  })
}

# NAT Gateways
resource "aws_eip" "nat" {
  count = 2

  domain = "vpc"

  tags = merge(var.tags, {
    Name = "eip-nat-${count.index + 1}-${var.environment}"
  })
}

resource "aws_nat_gateway" "mussikon" {
  count = 2

  allocation_id = aws_eip.nat[count.index].id
  subnet_id     = aws_subnet.public[count.index].id

  tags = merge(var.tags, {
    Name = "nat-gateway-${count.index + 1}-${var.environment}"
  })

  depends_on = [aws_internet_gateway.mussikon]
}

# Route Tables
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.mussikon.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.mussikon.id
  }

  tags = merge(var.tags, {
    Name = "rt-public-${var.environment}"
  })
}

resource "aws_route_table" "private" {
  count = 2

  vpc_id = aws_vpc.mussikon.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.mussikon[count.index].id
  }

  tags = merge(var.tags, {
    Name = "rt-private-${count.index + 1}-${var.environment}"
  })
}

# Route Table Associations
resource "aws_route_table_association" "public" {
  count = 2

  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_route_table_association" "private" {
  count = 2

  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private[count.index].id
}
```

### **EKS Cluster**

```hcl
# terraform/aws/eks.tf
# EKS Cluster
resource "aws_eks_cluster" "mussikon" {
  name     = var.cluster_name
  role_arn = aws_iam_role.eks_cluster.arn
  version  = "1.28"

  vpc_config {
    subnet_ids              = aws_subnet.private[*].id
    endpoint_private_access = true
    endpoint_public_access  = true
    public_access_cidrs     = ["0.0.0.0/0"]
  }

  encryption_config {
    provider {
      key_arn = aws_kms_key.eks.arn
    }
    resources = ["secrets"]
  }

  enabled_cluster_log_types = [
    "api",
    "audit",
    "authenticator",
    "controllerManager",
    "scheduler"
  ]

  depends_on = [
    aws_iam_role_policy_attachment.eks_cluster_policy,
    aws_cloudwatch_log_group.eks_cluster,
  ]

  tags = var.tags
}

# EKS Node Group
resource "aws_eks_node_group" "mussikon" {
  cluster_name    = aws_eks_cluster.mussikon.name
  node_group_name = "mussikon-nodes"
  node_role_arn   = aws_iam_role.eks_node_group.arn
  subnet_ids      = aws_subnet.private[*].id

  capacity_type  = "ON_DEMAND"
  instance_types = ["t3.medium"]

  scaling_config {
    desired_size = var.node_count
    max_size     = 10
    min_size     = 1
  }

  update_config {
    max_unavailable = 1
  }

  depends_on = [
    aws_iam_role_policy_attachment.eks_worker_node_policy,
    aws_iam_role_policy_attachment.eks_cni_policy,
    aws_iam_role_policy_attachment.eks_container_registry_policy,
  ]

  tags = var.tags
}

# IAM Roles
resource "aws_iam_role" "eks_cluster" {
  name = "eks-cluster-role-${var.environment}"

  assume_role_policy = jsonencode({
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "eks.amazonaws.com"
      }
    }]
    Version = "2012-10-17"
  })

  tags = var.tags
}

resource "aws_iam_role" "eks_node_group" {
  name = "eks-node-group-role-${var.environment}"

  assume_role_policy = jsonencode({
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ec2.amazonaws.com"
      }
    }]
    Version = "2012-10-17"
  })

  tags = var.tags
}

# IAM Policy Attachments
resource "aws_iam_role_policy_attachment" "eks_cluster_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSClusterPolicy"
  role       = aws_iam_role.eks_cluster.name
}

resource "aws_iam_role_policy_attachment" "eks_worker_node_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
  role       = aws_iam_role.eks_node_group.name
}

resource "aws_iam_role_policy_attachment" "eks_cni_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
  role       = aws_iam_role.eks_node_group.name
}

resource "aws_iam_role_policy_attachment" "eks_container_registry_policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
  role       = aws_iam_role.eks_node_group.name
}
```

---

## 🔧 **State Management y Remote Backends**

### **Backend Configuration**

```hcl
# terraform/backend.tf
terraform {
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "terraformstateaccount"
    container_name       = "terraform-state"
    key                  = "mussikon.tfstate"
  }
}

# Alternative AWS S3 Backend
# terraform {
#   backend "s3" {
#     bucket         = "terraform-state-bucket"
#     key            = "mussikon/terraform.tfstate"
#     region         = "us-east-1"
#     encrypt        = true
#     dynamodb_table = "terraform-state-lock"
#   }
# }
```

### **State Management Scripts**

```bash
#!/bin/bash
# scripts/terraform-init.sh

set -e

echo "🏗️ Initializing Terraform..."

# Check if backend is configured
if [ -z "$TF_BACKEND_CONFIG" ]; then
    echo "❌ TF_BACKEND_CONFIG environment variable is not set"
    exit 1
fi

# Initialize Terraform
terraform init \
    -backend-config="$TF_BACKEND_CONFIG" \
    -upgrade

# Validate configuration
terraform validate

# Format code
terraform fmt -recursive

echo "✅ Terraform initialized successfully!"
```

```bash
#!/bin/bash
# scripts/terraform-plan.sh

set -e

echo "📋 Planning Terraform changes..."

# Set workspace
terraform workspace select $ENVIRONMENT || terraform workspace new $ENVIRONMENT

# Plan changes
terraform plan \
    -var-file="environments/$ENVIRONMENT.tfvars" \
    -out="terraform-$ENVIRONMENT.tfplan"

echo "✅ Terraform plan completed!"
```

```bash
#!/bin/bash
# scripts/terraform-apply.sh

set -e

echo "🚀 Applying Terraform changes..."

# Apply changes
terraform apply "terraform-$ENVIRONMENT.tfplan"

# Output important values
terraform output -json > "outputs-$ENVIRONMENT.json"

echo "✅ Terraform apply completed!"
```

---

## 📦 **Modules y Reusable Infrastructure**

### **Module Structure**

```hcl
# modules/kubernetes-cluster/main.tf
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

resource "azurerm_kubernetes_cluster" "cluster" {
  name                = var.cluster_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = var.dns_prefix
  kubernetes_version  = var.kubernetes_version

  default_node_pool {
    name                = "system"
    vm_size             = var.vm_size
    node_count          = var.node_count
    vnet_subnet_id      = var.subnet_id
    enable_auto_scaling = var.enable_auto_scaling
    min_count           = var.min_count
    max_count           = var.max_count
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "standard"
  }

  tags = var.tags
}
```

```hcl
# modules/kubernetes-cluster/variables.tf
variable "cluster_name" {
  description = "Name of the Kubernetes cluster"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "dns_prefix" {
  description = "DNS prefix for the cluster"
  type        = string
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "vm_size" {
  description = "Size of the virtual machines"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "node_count" {
  description = "Number of nodes in the cluster"
  type        = number
  default     = 3
}

variable "subnet_id" {
  description = "ID of the subnet for the cluster"
  type        = string
}

variable "enable_auto_scaling" {
  description = "Enable auto scaling for the cluster"
  type        = bool
  default     = true
}

variable "min_count" {
  description = "Minimum number of nodes"
  type        = number
  default     = 1
}

variable "max_count" {
  description = "Maximum number of nodes"
  type        = number
  default     = 10
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
```

```hcl
# modules/kubernetes-cluster/outputs.tf
output "cluster_id" {
  description = "ID of the Kubernetes cluster"
  value       = azurerm_kubernetes_cluster.cluster.id
}

output "cluster_name" {
  description = "Name of the Kubernetes cluster"
  value       = azurerm_kubernetes_cluster.cluster.name
}

output "cluster_fqdn" {
  description = "FQDN of the Kubernetes cluster"
  value       = azurerm_kubernetes_cluster.cluster.fqdn
}

output "kube_config" {
  description = "Kubernetes configuration"
  value       = azurerm_kubernetes_cluster.cluster.kube_config
  sensitive   = true
}

output "cluster_identity" {
  description = "Identity of the Kubernetes cluster"
  value       = azurerm_kubernetes_cluster.cluster.identity
}
```

### **Using Modules**

```hcl
# terraform/main.tf
module "kubernetes_cluster" {
  source = "./modules/kubernetes-cluster"

  cluster_name         = var.cluster_name
  location             = var.location
  resource_group_name  = azurerm_resource_group.mussikon.name
  dns_prefix           = "mussikon-${var.environment}"
  kubernetes_version   = "1.28"
  vm_size              = var.vm_size
  node_count           = var.node_count
  subnet_id            = azurerm_subnet.aks.id
  enable_auto_scaling  = true
  min_count            = 1
  max_count            = 10

  tags = var.tags
}

module "database" {
  source = "./modules/database"

  environment          = var.environment
  location             = var.location
  resource_group_name  = azurerm_resource_group.mussikon.name
  subnet_id            = azurerm_subnet.database.id
  administrator_login  = "mussikonadmin"
  administrator_password = var.database_password

  tags = var.tags
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Terraform Configuration**
```hcl
# Crea una configuración completa de Terraform
# para la infraestructura de MussikOn
```

### **Ejercicio 2: Modules**
```hcl
# Crea módulos reutilizables para
# Kubernetes, Database y Storage
```

### **Ejercicio 3: State Management**
```bash
# Configura backend remoto y
# scripts de gestión de estado
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Terraform Fundamentals**: Conceptos básicos y configuración
2. **☁️ Azure Provider**: Infraestructura en Azure
3. **🌐 AWS Provider**: Infraestructura en AWS
4. **🔧 State Management**: Gestión de estado remoto
5. **📦 Modules**: Infraestructura reutilizable
6. **🚀 Automation**: Scripts y automatización

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Monitoring y Observabilidad**, implementando métricas y alertas.

---

**¡Has completado la quinta clase del Expert Level 1! 🏗️🎯**
