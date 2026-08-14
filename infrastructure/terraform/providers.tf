terraform {
  required_version = ">= 1.7"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  # Remote state — configure via `terraform init -backend-config=...` per environment
  # rather than hardcoding a storage account here.
  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}
