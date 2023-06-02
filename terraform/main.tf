terraform {
    required_providers {
      azurerm = {
        source = "hashicorp/azurerm"
        version = "~> 3.0.2"
      }
    }
    required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name = "rg-cosmos-partition-tests"
  location = "australiaeast"
}


resource "azurerm_cosmosdb_account" "cosmos-account" {
  name                = "cosmos-partition-tests"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB" 
  enable_automatic_failover = false
  
  consistency_policy {
    consistency_level       = "Eventual"
  }

  geo_location {
    location          = "australiaeast"
    failover_priority = 0
  }

  tags = {
    source = "terraform"
  }
}


resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "cosmos-partition-tests-db"
  resource_group_name = azurerm_cosmosdb_account.cosmos-account.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos-account.name
  throughput          = 400
}


