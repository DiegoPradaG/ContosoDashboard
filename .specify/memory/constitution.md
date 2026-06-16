<!--
REPORTE DE IMPACTO DE SINCRONIZACIÓN
- Cambio de versión: Plantilla -> 1.0.0 (Ratificación Inicial)
- Lista de principios modificados:
  - PRINCIPIO 1: [PRINCIPLE_1_NAME] -> I. Arquitectura Offline-First (Primero sin conexión)
  - PRINCIPIO 2: [PRINCIPLE_2_NAME] -> II. Abstracción de Infraestructura
  - PRINCIPIO 3: [PRINCIPLE_3_NAME] -> III. Seguridad y Control de Acceso Basado en Roles (RBAC)
  - PRINCIPIO 4: [PRINCIPLE_4_NAME] -> IV. Integración de Infraestructura CI/CD (Estándares CI/CD)
  - PRINCIPIO 5: [PRINCIPLE_5_NAME] -> V. Simplicidad Arquitectónica y Separación de Responsabilidades
- Secciones añadidas:
  - Stack Tecnológico y Gestión de Datos
  - Flujo de Trabajo de Desarrollo y Revisión
- Secciones eliminadas:
  - Ninguna
- Plantillas que requieren actualizaciones (✅ actualizada / ⚠ pendiente) con rutas de archivo:
  - .specify/templates/plan-template.md: ✅ No requiere actualizaciones, compuertas alineadas.
  - .specify/templates/spec-template.md: ✅ No requiere actualizaciones, secciones alindadas.
  - .specify/templates/tasks-template.md: ✅ No requiere actualizaciones, tipos de tareas alineados.
- Tareas pendientes de seguimiento:
  - Ninguna (todos los marcadores de posición han sido completados)
-->

# Constitución de ContosoDashboard

## Principios Fundamentales

### I. Arquitectura Offline-First (Primero sin conexión)
La aplicación DEBE soportar el desarrollo "offline-first" utilizando alternativas locales (base de datos SQLite, autenticación simulada basada en cookies, almacenamiento de archivos local) con abstracciones de interfaz limpias. Las actividades de desarrollo y prueba NO DEBEN depender de servicios en la nube activos. El cambio a servicios en la nube (como Azure SQL Database, Azure Blob Storage o Microsoft Entra ID) DEBE poder realizarse únicamente mediante cambios de configuración, sin alterar la lógica de negocio.

### II. Abstracción de Infraestructura
Todos los servicios externos y dependencias de infraestructura (acceso a bases de datos, autenticación, almacenamiento, notificaciones) DEBEN estar ocultos detrás de interfaces (por ejemplo, `IUserService`, `IFileStorageService`). La lógica de negocio y la capa de interfaz de usuario DEBEN depender únicamente de estas abstracciones, no de implementaciones concretas. Las implementaciones DEBEN registrarse y resolverse utilizando el contenedor de Inyección de Dependencias integrado.

### III. Seguridad y Control de Acceso Basado en Roles (RBAC)
Cada página, endpoint y método de servicio de negocio DEBE aplicar comprobaciones de autorización. La aplicación DEBE restringir el acceso basándose en roles de usuario jerárquicos (Employee, TeamLead, ProjectManager, Administrator). Se DEBEN implementar comprobaciones de seguridad a nivel de servicio para prevenir vulnerabilidades de Referencia Directa Insegura a Objetos (IDOR), como verificar que un usuario pertenezca a un proyecto antes de mostrar los detalles del mismo.

### IV. Integración de Infraestructura CI/CD
El proyecto DEBE mantener los pipelines de Dockerfile y Cloud Build (`/backend/cloudbuild.yaml` y `/frontend/cloudbuild.yaml` si existe separación frontend/backend, o descriptores de despliegue equivalentes). Al añadir nuevas variables de entorno o configuraciones de la aplicación, el desarrollador DEBE sincronizarlas con las sustituciones (`substitutions`) y variables de entorno en los archivos de despliegue correspondientes.

### V. Simplicidad Arquitectónica y Separación de Responsabilidades
El código de la aplicación DEBE seguir una arquitectura de capas estricta: Modelos (entidades y esquemas de datos), Servicios (lógica y reglas de negocio), Datos (contexto y semillas de EF Core) y Páginas (lógica de presentación de Razor/Blazor). La lógica de negocio y las consultas a la base de datos NO DEBEN estar incrustadas directamente dentro de los componentes Razor/Blazor o archivos PageModel.

## Stack Tecnológico y Gestión de Datos

- **Framework**: ASP.NET Core 8.0 con Blazor Server.
- **Base de Datos**: SQLite (`ContosoDashboard.db`) es el proveedor de base de datos predeterminado para desarrollo.
- **Inicialización de Datos**: El DbContext de la aplicación DEBE inicializar el esquema de la base de datos y sembrar los datos iniciales en la primera ejecución utilizando `context.Database.EnsureCreated()`.
- **Estilo**: Bootstrap 5.3 con Bootstrap Icons para mantener la consistencia del diseño.

## Flujo de Trabajo de Desarrollo y Revisión

- Todas las implementaciones de características DEBEN seguir el proceso de Desarrollo Guiado por Especificaciones (SDD): Especificación -> Plan -> Tareas -> Implementación.
- La modificación de entidades de base de datos requiere añadir o actualizar los datos semilla correspondientes en `ApplicationDbContext` y asegurar la regeneración de la base de datos durante las pruebas locales.
- Justificación de complejidad: Cualquier desviación de estas reglas arquitectónicas DEBE ser documentada en el plan de implementación de características correspondiente bajo la sección "Seguimiento de Complejidad" (Complexity Tracking).

## Gobernanza

- La Constitución de ContosoDashboard prevalece sobre cualquier otra documentación y práctica de desarrollo en este repositorio.
- Las enmiendas a esta constitución DEBEN ser documentadas, incrementar el número de versión e incluir un plan de migración para el código existente.
- Todas las revisiones de código (Pull Requests) DEBEN verificar el cumplimiento de estos principios fundamentales.

**Versión**: 1.0.0 | **Ratified**: 2026-06-16 | **Última Modificación**: 2026-06-16
