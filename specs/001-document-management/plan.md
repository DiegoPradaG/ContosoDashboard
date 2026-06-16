# Plan de Implementación: Carga y Gestión de Documentos

**Rama**: `001-document-management` | **Fecha**: 2026-06-16 | **Especificación**: [spec.md](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/spec.md)
**Entrada**: Especificación de características de `/specs/001-document-management/spec.md`

## Resumen

El objetivo de esta funcionalidad es incorporar capacidades de carga, almacenamiento, organización y compartición segura de documentos en ContosoDashboard. Para ello, se implementará un servicio de almacenamiento abstracto (`IFileStorageService`) con una implementación local (`LocalFileStorageService`) que guarda los archivos en un directorio externo (`AppData/uploads`) de forma segura, previniendo accesos directos malintencionados (IDOR) a través de un endpoint controlado de descarga. La metadata se registrará en la base de datos SQLite y la interfaz de usuario se construirá con Blazor Server.

## Contexto Técnico

**Lenguaje/Versión**: C# / .NET 8  
**Dependencias Principales**: `Microsoft.EntityFrameworkCore.Sqlite` (8.0.0), `Microsoft.AspNetCore.Components.Authorization`  
**Almacenamiento**: Base de datos SQLite local (`ContosoDashboard.db`) y almacenamiento en sistema de archivos local (`AppData/uploads`)  
**Pruebas**: xUnit (`dotnet test`)  
**Plataforma Objetivo**: Windows / Linux / macOS (Agnóstico mediante .NET 8)  
**Tipo de Proyecto**: Aplicación Web Single Project (ASP.NET Core con Blazor Server)  
**Objetivos de Rendimiento**: Carga de archivos de hasta 25MB < 30s, listado y búsquedas en la UI en < 2 segundos  
**Restricciones**: Completamente ejecutable fuera de línea, límite de tamaño de archivo de 25MB, almacenamiento seguro fuera de `wwwroot`  
**Escala/Alcance**: ~100 usuarios, almacenamiento local inicial para capacitación  

## Verificación de la Constitución (Constitution Check)

*GATE: Debe pasar antes de la fase de investigación 0 y re-evaluarse después del diseño de la fase 1.*

- **I. Arquitectura Offline-First**: ✅ **PASADO**. El almacenamiento físico y la base de datos son 100% locales, eliminando dependencias de red en la nube para el desarrollo.
- **II. Abstracción de Infraestructura**: ✅ **PASADO**. Se expone `IFileStorageService` para el almacenamiento, permitiendo cambiar a Azure Blob Storage en el futuro sin modificar la lógica de negocio.
- **III. Seguridad y RBAC**: ✅ **PASADO**. Las descargas se realizan a través de un endpoint controlado con validación previa de claims (propiedad, compartición o pertenencia al proyecto) para evitar IDOR.
- **IV. Integración de Infraestructura CI/CD**: ✅ **PASADO**. No se introducen variables de entorno obligatorias en producción sin documentarlas. Si se despliega, se configurará la carpeta correspondiente.
- **V. Simplicidad Arquitectónica**: ✅ **PASADO**. Los archivos de UI no contienen lógica de base de datos ni acceso directo al disco, todo se delega a `DocumentService` y `LocalFileStorageService`.

## Estructura del Proyecto

### Documentación (esta característica)

```text
specs/001-document-management/
├── plan.md              # Este archivo (Plan de Implementación)
├── research.md          # Resultados de la Investigación de Fase 0
├── data-model.md        # Diseño de Entidades de Fase 1
├── quickstart.md        # Guía Rápida de Integración y Pruebas
├── contracts/           # Interfaces y contratos del servicio
└── tasks.md             # Lista de tareas y subtareas (Fase 2)
```

### Código Fuente (Estructura de Archivos)

```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs      # Modificar: Agregar DbSets para Document, DocumentShare, AuditLog
├── Models/
│   ├── Document.cs                  # Nuevo: Entidad de metadatos de documentos
│   ├── DocumentShare.cs             # Nuevo: Entidad para el control de archivos compartidos
│   └── AuditLog.cs                  # Nuevo: Entidad para el registro de auditoría de documentos
├── Services/
│   ├── IFileStorageService.cs       # Nuevo: Interfaz abstracta de almacenamiento
│   ├── LocalFileStorageService.cs   # Nuevo: Guardado físico en AppData/uploads
│   ├── IDocumentService.cs          # Nuevo: Interfaz de lógica de negocio de documentos
│   └── DocumentService.cs           # Nuevo: Lógica de negocio (upload workflow, validaciones y share)
├── Controllers/
│   └── DocumentDownloadController.cs # Nuevo: Endpoint seguro de descargas y vistas previas
├── Pages/
│   ├── Documents.razor              # Nuevo: Página de visualización y carga de documentos
│   ├── TaskDetails.razor            # Modificar: Permitir ver y adjuntar archivos de tareas
│   └── ProjectDetails.razor         # Modificar: Agregar pestaña de documentos del proyecto
├── Shared/
│   └── NavMenu.razor                # Modificar: Agregar enlace a la sección de Documentos
└── appsettings.json                 # Modificar: Agregar configuración para la ruta de subidas
```

**Decisión de Estructura**: Seguimos la arquitectura modular del proyecto existente, agregando las nuevas entidades en `Models/`, servicios en `Services/` y páginas UI en `Pages/`. Creamos además un `Controller` estándar de MVC para poder servir los archivos de forma segura fuera de la carpeta estática `wwwroot`.

## Seguimiento de Complejidad

> *No se registran violaciones a la Constitución en este plan técnico.*
