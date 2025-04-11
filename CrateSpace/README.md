# cratespace Monolithic Application - Azure Deployment Guide

This guide explains how to deploy the cratespace monolithic application to Azure as part of Phase 1 (Lift & Shift) of our cloud migration strategy.

## Prerequisites

- Azure subscription
- Azure CLI installed (for script-based deployment)
- PostgreSQL database credentials
- Basic knowledge of Azure VM configuration

## Deployment Options

### Option 1: Manual Deployment via Azure Portal

1. **Create Resource Group**:
   - Navigate to Resource Groups in the Azure Portal
   - Click "Create" and provide a name (e.g., "cratespace-rg")
   - Select your region and click "Review + create"

2. **Create Virtual Network**:
   - Navigate to Virtual Networks
   - Click "Create" and provide a name (e.g., "cratespace-vnet")
   - Configure IP address space (e.g., 10.0.0.0/16)
   - Create a subnet (e.g., "default", 10.0.0.0/24)
   - Click "Review + create"

3. **Create Network Security Group**:
   - Navigate to Network Security Groups
   - Click "Create" and provide a name (e.g., "cratespace-nsg")
   - Add inbound rules for:
     - SSH (port 22) for management
     - HTTP (port 80) for web traffic
     - HTTPS (port 443) for secure web traffic
   - Click "Review + create"

4. **Create a Virtual Machine**:
   - Navigate to Virtual Machines
   - Click "Create" and select "Azure virtual machine"
   - Select your resource group and provide a name (e.g., "cratespace-vm")
   - Choose Ubuntu Server 20.04 LTS or similar
   - Select VM size (Standard_B2s recommended for testing)
   - Configure Authentication (SSH key or password)
   - Open ports 22, 80, and 443
   - Click "Review + create"

5. **Set Up PostgreSQL**:
   - Option A: Install PostgreSQL on the VM
     - SSH into the VM: `ssh username@vm-ip-address`
     - Install PostgreSQL: 
       ```bash
       sudo apt update
       sudo apt install postgresql postgresql-contrib
       ```
     - Configure PostgreSQL for remote access (if needed)
   
   - Option B: Create an Azure Database for PostgreSQL
     - Navigate to Azure Database for PostgreSQL
     - Click "Create" and select "Single server"
     - Configure the server details
     - Set up firewall rules to allow access from your VM

6. **Deploy the Application**:
   - Install .NET SDK on the VM:
     ```bash
     wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
     sudo dpkg -i packages-microsoft-prod.deb
     sudo apt-get update
     sudo apt-get install -y dotnet-sdk-8.0
     ```
   - Copy the application files to the VM (using SCP or Git)
   - Update the connection string in appsettings.json
   - Run the application:
     ```bash
     cd cratespace.Monolith
     dotnet run --urls=http://0.0.0.0:80
     ```
   - For production, set up a service to keep the application running

### Option 2: Automated Deployment with Azure CLI

1. **Create a deployment script (deploy.sh)**:

```bash
#!/bin/bash

# Variables
RESOURCE_GROUP="cratespace-rg"
LOCATION="eastus"
VNET_NAME="cratespace-vnet"
NSG_NAME="cratespace-nsg"
VM_NAME="cratespace-vm"
VM_SIZE="Standard_B2s"
ADMIN_USERNAME="cratespaceadmin"
SSH_KEY_PATH="~/.ssh/id_rsa.pub"
POSTGRES_SERVER="cratespace-postgres"
POSTGRES_ADMIN="postgresadmin"
POSTGRES_PASSWORD="YourStrongPasswordHere123!"  # Change this
DB_NAME="cratespace_db"

# Login to Azure
echo "Logging in to Azure..."
az login

# Create Resource Group
echo "Creating Resource Group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Virtual Network
echo "Creating Virtual Network..."
az network vnet create \
  --resource-group $RESOURCE_GROUP \
  --name $VNET_NAME \
  --address-prefix 10.0.0.0/16 \
  --subnet-name default \
  --subnet-prefix 10.0.0.0/24

# Create Network Security Group
echo "Creating Network Security Group..."
az network nsg create \
  --resource-group $RESOURCE_GROUP \
  --name $NSG_NAME

# Add NSG rules
echo "Adding NSG rules..."
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name $NSG_NAME \
  --name AllowSSH \
  --priority 1000 \
  --destination-port-ranges 22 \
  --protocol Tcp \
  --access Allow

az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name $NSG_NAME \
  --name AllowHTTP \
  --priority 1001 \
  --destination-port-ranges 80 \
  --protocol Tcp \
  --access Allow

az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name $NSG_NAME \
  --name AllowHTTPS \
  --priority 1002 \
  --destination-port-ranges 443 \
  --protocol Tcp \
  --access Allow

# Create VM
echo "Creating Virtual Machine..."
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --image UbuntuLTS \
  --admin-username $ADMIN_USERNAME \
  --ssh-key-value @$SSH_KEY_PATH \
  --size $VM_SIZE \
  --vnet-name $VNET_NAME \
  --subnet default \
  --nsg $NSG_NAME \
  --public-ip-sku Standard

# Get the VM IP
VM_IP=$(az vm show -d -g $RESOURCE_GROUP -n $VM_NAME --query publicIps -o tsv)
echo "VM created with IP: $VM_IP"

# Create PostgreSQL server (Option B - Azure Database for PostgreSQL)
echo "Creating Azure Database for PostgreSQL..."
az postgres server create \
  --resource-group $RESOURCE_GROUP \
  --name $POSTGRES_SERVER \
  --location $LOCATION \
  --admin-user $POSTGRES_ADMIN \
  --admin-password $POSTGRES_PASSWORD \
  --sku-name GP_Gen5_2 \
  --version 12

# Allow Azure services to access postgres
echo "Configuring PostgreSQL firewall..."
az postgres server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $POSTGRES_SERVER \
  --name AllowAll \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 255.255.255.255

# Create database
echo "Creating database..."
az postgres db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $POSTGRES_SERVER \
  --name $DB_NAME

echo "Setting up VM environment..."
ssh $ADMIN_USERNAME@$VM_IP << 'EOF'
  # Update packages
  sudo apt update
  sudo apt upgrade -y

  # Install .NET SDK
  wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  sudo apt-get update
  sudo apt-get install -y dotnet-sdk-8.0

  # Install Git
  sudo apt install -y git

  # Create app directory
  mkdir -p ~/cratespace
EOF

echo "Deployment completed successfully!"
echo "Next steps:"
echo "1. Clone your repo to the VM: git clone your-repo-url ~/cratespace"
echo "2. Update connection string in appsettings.json"
echo "3. Run the application or set up a service"
echo "4. Access your application at http://$VM_IP"
```

2. **Make the script executable and run it**:

```bash
chmod +x deploy.sh
./deploy.sh
```

3. **Deploy the Application to the VM**:

After the infrastructure is set up, you'll need to deploy your application to the VM:

```bash
# Clone your repository (replace with your actual repository URL)
git clone https://github.com/your-username/cratespace.git ~/cratespace

# Navigate to the application directory
cd ~/cratespace

# Update the connection string in appsettings.json
# Replace [SERVER] with your PostgreSQL server name or IP
# Replace [USERNAME] and [PASSWORD] with your credentials
sed -i 's/"DefaultConnection": ".*"/"DefaultConnection": "Host=[SERVER];Database=cratespace_db;Username=[USERNAME];Password=[PASSWORD]"/' appsettings.json

# Build and run the application
dotnet build
dotnet run --urls=http://0.0.0.0:80
```

### Option 3: Using Terraform (Recommended for Week 1, Day 4)

1. **Create a main.tf file**:

```terraform
provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "cratespace-rg"
  location = "eastus"
}

resource "azurerm_virtual_network" "vnet" {
  name                = "cratespace-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_subnet" "subnet" {
  name                 = "default"
  resource_group_name  = azurerm_resource_group.rg.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.0.0/24"]
}

resource "azurerm_network_security_group" "nsg" {
  name                = "cratespace-nsg"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  security_rule {
    name                       = "SSH"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "HTTP"
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
    name                       = "HTTPS"
    priority                   = 1003
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

resource "azurerm_subnet_network_security_group_association" "nsg_association" {
  subnet_id                 = azurerm_subnet.subnet.id
  network_security_group_id = azurerm_network_security_group.nsg.id
}

resource "azurerm_public_ip" "publicip" {
  name                = "cratespace-publicip"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  allocation_method   = "Static"
  sku                 = "Standard"
}

resource "azurerm_network_interface" "nic" {
  name                = "cratespace-nic"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.subnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.publicip.id
  }
}

resource "azurerm_linux_virtual_machine" "vm" {
  name                = "cratespace-vm"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  size                = "Standard_B2s"
  admin_username      = "cratespaceadmin"
  network_interface_ids = [
    azurerm_network_interface.nic.id,
  ]

  admin_ssh_key {
    username   = "cratespaceadmin"
    public_key = file("~/.ssh/id_rsa.pub")
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-focal"
    sku       = "20_04-lts"
    version   = "latest"
  }
}

resource "azurerm_postgresql_server" "postgres" {
  name                = "cratespace-postgres"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  sku_name = "GP_Gen5_2"

  storage_mb                   = 5120
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false
  auto_grow_enabled            = true

  administrator_login          = "postgresadmin"
  administrator_login_password = "YourStrongPasswordHere123!" # Change this
  version                      = "11"
  ssl_enforcement_enabled      = true
}

resource "azurerm_postgresql_database" "database" {
  name                = "cratespace_db"
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_postgresql_server.postgres.name
  charset             = "UTF8"
  collation           = "English_United States.1252"
}

resource "azurerm_postgresql_firewall_rule" "firewall" {
  name                = "AllowAll"
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_postgresql_server.postgres.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "255.255.255.255"
}

output "public_ip_address" {
  value = azurerm_public_ip.publicip.ip_address
}

output "postgres_server_name" {
  value = azurerm_postgresql_server.postgres.name
}
```

2. **Initialize and apply Terraform configuration**:

```bash
terraform init
terraform plan
terraform apply
```

3. **Connect to the VM and deploy the application**:

After Terraform successfully creates the infrastructure, you can SSH into the VM and deploy your application following the same steps as in Option 2.

## Setting Up a Service

To ensure your application runs continuously and starts automatically when the VM reboots, you should set up a systemd service:

1. **Create a service file**:

```bash
sudo nano /etc/systemd/system/cratespace.service
```

2. **Add the following configuration**:

```
[Unit]
Description=cratespace Monolithic Application
After=network.target

[Service]
WorkingDirectory=/home/cratespaceadmin/cratespace
ExecStart=/usr/bin/dotnet run --urls=http://0.0.0.0:80
Restart=always
RestartSec=10
SyslogIdentifier=cratespace
User=cratespaceadmin
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

3. **Enable and start the service**:

```bash
sudo systemctl enable cratespace.service
sudo systemctl start cratespace.service
sudo systemctl status cratespace.service
```

## Post-Deployment Verification

After deploying the application, verify that everything is working correctly:

1. **Check the service status**:
   ```bash
   sudo systemctl status cratespace.service
   ```

2. **View application logs**:
   ```bash
   sudo journalctl -u cratespace.service
   ```

3. **Access the application in a web browser**:
   - Navigate to `http://<VM-IP-ADDRESS>`
   - Verify that the dashboard loads
   - Test inventory and order management functionality

## Documentation for Submission

To complete Phase 1 (Week 1, Friday), create the following documentation:

1. **Architecture Diagram**: 
   - Create a diagram showing the VM, NSG, VNet, and PostgreSQL components
   - Include the application and data flow

2. **Deployment Screenshots**:
   - Azure Portal resource group view
   - Running VM details
   - PostgreSQL server details
   - Application running in browser

3. **Setup Instructions**:
   - Steps to recreate the deployment
   - Configuration details
   - Connection string format

4. **Troubleshooting Guide**:
   - Common issues and solutions
   - Log locations
   - How to restart services

This completes the deployment guide for Phase 1 of our Azure migration strategy.