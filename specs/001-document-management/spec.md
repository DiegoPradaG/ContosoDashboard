# Feature Specification: Carga y Gestión de Documentos (Document Upload & Management)

**Feature Branch**: `001-document-management`  
**Created**: 2026-06-16  
**Status**: Draft  
**Input**: Requisitos de negocio proporcionados en la carpeta `StakeholderDocs`

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Carga y Almacenamiento Seguro de Documentos (Priority: P1) 🎯 MVP

Como empleado o administrador, quiero poder cargar archivos locales y definir sus datos de clasificación (título, categoría, tags y proyecto asociado) para mantenerlos almacenados de manera organizada y segura.

**Why this priority**: Es la funcionalidad básica habilitadora para todo el sistema de gestión de documentos. Sin la carga de archivos no hay datos para listar, buscar o compartir.

**Independent Test**: Cargar un archivo PDF de 5MB con título "Plan de Marketing" y categoría "Presentaciones". Verificar que se almacena físicamente en el disco local (`AppData/uploads/{userId}/personal/{guid}.pdf`) y que la metadata se guarda de forma consistente en la base de datos.

**Acceptance Scenarios**:

1. **Given** que un usuario está autenticado en la aplicación, **When** accede a la sección de carga de documentos, selecciona un archivo compatible (ej. PDF) menor de 25MB, completa los campos requeridos (Título y Categoría) y hace clic en "Cargar", **Then** el sistema guarda el archivo en el almacenamiento local usando un nombre basado en GUID, registra la metadata en la base de datos vinculada a su usuario, y muestra un mensaje de éxito.
2. **Given** que un usuario intenta cargar un archivo, **When** el tamaño del archivo excede los 25 MB o tiene una extensión no permitida (ej. `.exe`), **Then** el sistema cancela la operación de forma inmediata antes del guardado físico y muestra un mensaje de error claro al usuario indicando el motivo de rechazo.

---

### User Story 2 - Visualización, Búsqueda y Descarga de Documentos (Priority: P1)

Como empleado, quiero ver mis documentos cargados, buscar archivos mediante diferentes criterios y descargarlos o previsualizarlos para utilizarlos en mis tareas cotidianas.

**Why this priority**: Permite recuperar los archivos cargados, entregando valor directo al resolver el problema de la pérdida de visibilidad de los archivos del proyecto.

**Independent Test**: Buscar por el tag "Marketing" o el título "Plan" en la barra de búsqueda y verificar que los resultados coincidan, que se devuelvan en menos de 2 segundos y que solo se visualicen archivos a los que el usuario tiene acceso legítimo.

**Acceptance Scenarios**:

1. **Given** que un usuario ha cargado múltiples documentos, **When** abre su vista "Mis Documentos", **Then** puede ordenar la lista por título, fecha de carga y tamaño, y filtrar por categoría o proyecto asociado.
2. **Given** que un usuario tiene acceso a ciertos documentos compartidos o de sus proyectos, **When** realiza una búsqueda global por título o tags, **Then** el sistema muestra solo los resultados autorizados, ocultando los archivos privados de otros usuarios.

---

### User Story 3 - Compartir y Gestionar Archivos Compartidos (Priority: P2)

Como propietario de un documento, quiero poder compartirlo con otros compañeros o departamentos completos de la empresa para colaborar en equipo, recibiendo notificaciones en tiempo real cuando se comparte algo conmigo.

**Why this priority**: Aumenta la colaboración entre equipos, reduciendo el envío descontrolado de adjuntos por correo electrónico.

**Independent Test**: Compartir un documento personal con el usuario "Camille Nicole" y comprobar que Camille recibe una notificación dentro de la aplicación y puede ver el archivo en su sección de "Compartidos conmigo".

**Acceptance Scenarios**:

1. **Given** que un usuario es dueño de un documento, **When** selecciona la opción "Compartir", elige a un usuario específico o a un departamento (equipo) y confirma, **Then** el sistema registra la relación de acceso y genera una notificación en tiempo real para los destinatarios.
2. **Given** que un usuario accede a su sección de "Compartidos conmigo", **When** descarga uno de los archivos listados, **Then** el endpoint de descarga valida que exista la relación de compartición activa antes de despachar el flujo de bytes.

---

### User Story 4 - Integración con Tareas y Dashboard (Priority: P2)

Como miembro de un proyecto, quiero adjuntar documentos directamente a tareas específicas y ver un widget de documentos recientes en mi panel de inicio para acceder rápidamente a la información relevante.

**Why this priority**: Vincula los recursos documentales directamente con el flujo operativo del negocio (las tareas diarias y la pantalla principal de monitoreo).

**Independent Test**: Adjuntar un archivo en el detalle de la Tarea 1. Comprobar que al consultar la vista de detalles del Proyecto 1, el archivo aparece automáticamente asociado a la lista de documentos del proyecto.

**Acceptance Scenarios**:

1. **Given** que un usuario está viendo el detalle de una tarea asignada, **When** carga un documento directamente desde esa sección, **Then** el archivo se asocia tanto a la tarea como al proyecto al que pertenece dicha tarea de forma automática.
2. **Given** que un usuario ingresa al panel principal (Dashboard), **When** carga la página de inicio, **Then** se renderiza un widget con los últimos 5 documentos subidos por él y el contador total de documentos se actualiza en las tarjetas del dashboard.

---

### User Story 5 - Reportes e Informes de Auditoría para Administradores (Priority: P3)

Como administrador del sistema, quiero ver el historial de actividades de los documentos y reportes estadísticos para asegurar el cumplimiento de las políticas de información y auditoría de la empresa.

**Why this priority**: Garantiza el cumplimiento normativo interno y ayuda a identificar cuellos de botella u patrones de uso de datos.

**Independent Test**: Ejecutar actividades de carga y descarga de un archivo y verificar que la tabla de logs de auditoría contenga los registros correspondientes del usuario, fecha, tipo de acción y DocumentId asociado.

**Acceptance Scenarios**:

1. **Given** que se ejecutan acciones sobre los documentos (carga, descarga, eliminación, compartir), **When** se procesa la acción, **Then** el sistema registra de forma automática una entrada de auditoría inmutable con la fecha, id del usuario y tipo de operación.

---

### Edge Cases

- **Colisiones de Nombres**: Si dos usuarios suben un archivo llamado exactamente `reporte.pdf`, el sistema debe renombrar internamente el archivo usando GUID para evitar que se sobrescriban o que ocurran errores de clave duplicada en la base de datos.
- **Acceso Directo Malintencionado (IDOR)**: Si un usuario intenta descargar un archivo adivinando su ruta URL física o su identificador en la base de datos sin tener permisos de acceso directos (no es dueño, no es miembro del proyecto asociado ni se le ha compartido el archivo), el sistema debe bloquear la descarga y retornar un código HTTP 403 (Prohibido).
- **Fallas en el Almacenamiento**: Si el guardado del archivo físico en el disco de red o local falla a mitad del proceso, la transacción de la base de datos no debe completarse, evitando registros huérfanos que apunten a archivos inexistentes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE permitir la carga de archivos locales con extensiones válidas: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, TXT, JPG, JPEG, PNG.
- **FR-002**: El tamaño máximo de archivo aceptado por carga DEBE ser de 25 megabytes (25 MB).
- **FR-003**: Al cargar un documento, el título y la categoría del mismo DEBEN ser obligatorios. Las categorías permitidas son: `Project Documents`, `Team Resources`, `Personal Files`, `Reports`, `Presentations`, `Other`.
- **FR-004**: Los archivos almacenados físicamente DEBEN guardarse fuera de la carpeta web pública `wwwroot` (`AppData/uploads/{userId}/{projectId o "personal"}/{guid}.{extension}`) para evitar accesos no autorizados directos.
- **FR-005**: El sistema DEBE exponer un servicio abstracto (`IFileStorageService`) con métodos para guardar (`UploadAsync`), descargar (`DownloadAsync`), borrar (`DeleteAsync`) y obtener URL (`GetUrlAsync`), permitiendo migración futura a Azure Blob Storage sin modificar el código de negocio.
- **FR-006**: La descarga de cualquier documento DEBE requerir una validación previa de autorización en el endpoint que sirve el archivo, comparando los roles y permisos del usuario autenticado contra la pertenencia al proyecto o la propiedad del archivo.
- **FR-007**: El sistema DEBE permitir la compartición de un documento con usuarios individuales o a nivel de departamento (equipo), enviando notificaciones internas en tiempo real al receptor.
- **FR-008**: Se DEBE habilitar una barra de búsqueda que filtre por título, descripción, tags, nombre del cargador o proyecto asociado, retornando resultados únicamente para documentos que el usuario tiene permitido leer.
- **FR-009**: Las acciones de carga, descarga, eliminación y compartición DEBEN registrar logs de auditoría en la base de datos.

### Key Entities

- **Document**:
  - `DocumentId` (int, clave primaria)
  - `Title` (string, máx 255, requerido)
  - `Description` (string, opcional)
  - `Category` (string, requerido, valores de texto predefinidos)
  - `FileName` (string, máx 255, requerido)
  - `FilePath` (string, requerido, nombre físico GUID)
  - `FileType` (string, máx 255, tipo MIME requerido)
  - `FileSize` (long, requerido, bytes)
  - `UploadDate` (DateTime, requerido)
  - `UploadedByUserId` (int, FK a User, requerido)
  - `ProjectId` (int, FK a Project, opcional)
  - `Tags` (string, opcional, delimitado por comas)
- **DocumentShare**:
  - `DocumentShareId` (int, clave primaria)
  - `DocumentId` (int, FK a Document, requerido)
  - `SharedWithUserId` (int, FK a User, opcional)
  - `SharedWithDepartment` (string, opcional)
  - `SharedDate` (DateTime, requerido)
- **AuditLog**:
  - `AuditLogId` (int, clave primaria)
  - `UserId` (int, FK a User, requerido)
  - `Action` (string, requerido, ej: "Upload", "Download")
  - `Details` (string, opcional)
  - `Timestamp` (DateTime, requerido)

### Límites del Sistema y Fuera de Alcance

Los siguientes aspectos NO forman parte de esta especificación inicial:
- Edición colaborativa en tiempo real de los documentos.
- Historial de versiones del archivo y reversión de cambios (rollback).
- Flujos de trabajo de aprobación o enrutamiento avanzado de documentos.
- Integración con repositorios externos (como SharePoint, OneDrive o Google Drive).
- Soporte para aplicación móvil nativa (el alcance inicial es exclusivamente web).
- Plantillas de generación automática de documentos.
- Gestión de cuotas de almacenamiento por usuario o por proyecto.
- Funcionalidad de eliminación suave (papelera de reciclaje) con recuperación.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 70% de los usuarios activos de ContosoDashboard habrán subido al menos un documento dentro de los 3 meses posteriores al lanzamiento.
- **SC-002**: Las búsquedas de documentos DEBEN retornar resultados en un tiempo promedio inferior a 2 segundos en colecciones de hasta 500 registros.
- **SC-003**: El 90% de los documentos en el sistema DEBEN estar correctamente categorizados mediante el selector obligatorio del formulario de carga.
- **SC-004**: Se deben registrar cero (0) incidentes de seguridad relacionados con el acceso o descarga de archivos por parte de usuarios no autorizados (IDOR).
- **SC-005**: El proceso de subida física y persistencia en disco de archivos de hasta 25MB DEBE completarse en menos de 30 segundos bajo condiciones normales de red local.
