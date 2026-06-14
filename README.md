# 🚀 API RESTful de Productos (Enterprise)

API robusta y escalable para la gestión de un catálogo de productos, construida con **ASP.NET Core 10** siguiendo las mejores prácticas de la industria y patrones de diseño modernos.

## 🛠️ Tecnologías Utilizadas
- **Framework:** .NET 10 (C#)
- **Base de Datos:** PostgreSQL (vía Entity Framework Core)
- **Caché:** Redis (Patrón Cache-Aside)
- **Seguridad:** Autenticación JWT, Autorización por Roles y Rate Limiting
- **Infraestructura:** Docker y Docker Compose
- **Logging:** Serilog (Logs estructurados)
- **Arquitectura:** Clean Architecture / Separación de responsabilidades (DTOs, AutoMapper, Servicios)

## 📋 Características Principales
- ✅ Operaciones CRUD completas con validación de datos.
- ✅ Protección contra ataques de fuerza bruta (Rate Limiting).
- ✅ Respuestas de error globales y consistentes (Middleware personalizado).
- ✅ Totalmente containerizada para despliegue consistente.

## 🚀 Cómo ejecutar el proyecto localmente

### Requisito previo
Tener instalado [Docker Desktop](https://www.docker.com/products/docker-desktop).

### Pasos
1. Clona este repositorio:
   ```bash
   git clone https://github.com/TU_USUARIO/api-restful.git
   cd api-restful


# 🚀 API RESTful de Productos (Enterprise)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ioseph06_api-restful&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ioseph06_api-restful)
[![.NET CI & SonarCloud Analysis](https://github.com/ioseph06/api-restful/actions/workflows/ci.yml/badge.svg)](https://github.com/ioseph06_api-restful/actions/workflows/ci.yml)

API robusta y escalable para la gestión de un catálogo de productos...