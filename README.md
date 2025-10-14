# ServiceDeskNg

**ServiceDeskNg** es una aplicación de **Mesa de Servicio (Help Desk)** desarrollada con **.NET 9 (ASP.NET Core Web API)** y **Angular 18**, diseñada para gestionar incidentes, solicitudes y flujos de trabajo entre agentes, supervisores y clientes.  
El objetivo es ofrecer una plataforma moderna, modular y escalable para la gestión eficiente de tickets.

---

## 🚀 Tecnologías principales

| Capa | Tecnología |
|------|-------------|
| Backend | ASP.NET Core 9 + Entity Framework Core (MySQL) |
| Frontend | Angular 18 |
| Base de Datos | MySQL 8.x |
| ORM | Entity Framework Core |
| IDEs recomendados | Visual Studio 2022 / VS Code |
| Cliente API | Postman / Insomnia |

---

## ⚙️ Estructura del Proyecto

### 📁 Backend (`ServiceDeskNg.Server`)

ServiceDeskNg.Server/
│

├── Controllers/ # Controladores API (Usuarios, Tickets, etc.)

├── Data/ # DbContext y configuración de la base de datos

├── Services/ #Metodos y funciones que hacen uso de repository

├── Models/ # Entidades de la base de datos

├── Repositories/ # Implementaciones concretas (ej: UsuarioRepository)

├── Repositories/Interfaces/ # Interfaces genéricas (ej: ICrudRepository)

├── Program.cs # Configuración principal del servidor

└── appsettings.json # Configuración de la conexión a la base de datos

### 📁 Frontend (`ServiceDeskNg.Client`)

ServiceDeskNg.Client/

│

├── src/

│ ├── app/

│ │ ├── components/ # Componentes (views)

│ │ ├── services/ # Servicios para consumir APIs

│ │ ├── models/ # Interfaces de datos compartidas

│ │ └── app.module.ts

│ ├── assets/

│ └── environments/

│ ├── environment.ts

│ └── environment.prod.ts


---

## 🧩 Patrón de arquitectura

El backend sigue una arquitectura **Repository Service Pattern**, desacoplando la lógica de acceso a datos del controlador.  
Esto facilita el mantenimiento, las pruebas unitarias y la expansión futura.


