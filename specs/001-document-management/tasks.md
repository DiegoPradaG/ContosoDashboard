# Tareas: Carga y Gestión de Documentos

**Input**: Documentos de diseño de `/specs/001-document-management/`
**Prerrequisitos**: [plan.md](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/plan.md), [spec.md](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/spec.md), [research.md](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/research.md), [data-model.md](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/data-model.md) y [ServiceContracts.cs](file:///C:/Users/dprada/Documents/GLocation/ContosoDashboard/specs/001-document-management/contracts/ServiceContracts.cs).

---

## Estructura de Fases de Tareas

## Phase 1: Setup (Infraestructura Compartida)

**Propósito**: Configuración inicial y registro del módulo de documentos en el proyecto.

- [x] T001 Registrar la configuración de inyección de dependencias para los servicios de almacenamiento en [ContosoDashboard/Program.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Program.cs)
- [x] T002 [P] Agregar configuraciones del directorio de subida de archivos en [ContosoDashboard/appsettings.json](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/appsettings.json)

---

## Phase 2: Foundational (Prerrequisitos Bloqueantes)

**Propósito**: Establecer las bases de datos y la capa de servicios de almacenamiento físico y metadatos antes de implementar cualquier UI.

- [x] T003 [P] Crear la interfaz de almacenamiento físico en [ContosoDashboard/Services/IFileStorageService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/IFileStorageService.cs)
- [x] T004 Crear la implementación local de guardado físico en [ContosoDashboard/Services/LocalFileStorageService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/LocalFileStorageService.cs) (depende de T003)
- [x] T005 [P] Crear las clases de modelo `Document.cs`, `DocumentShare.cs` y `AuditLog.cs` en [ContosoDashboard/Models/](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Models/)
- [x] T006 Registrar las nuevas entidades DbSets y configurar relaciones en [ContosoDashboard/Data/ApplicationDbContext.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Data/ApplicationDbContext.cs) (depende de T005)
- [x] T007 [P] Crear la interfaz de lógica de negocio en [ContosoDashboard/Services/IDocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/IDocumentService.cs)
- [x] T008 Crear la clase controladora de descargas seguras para prevención de IDOR en [ContosoDashboard/Controllers/DocumentDownloadController.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Controllers/DocumentDownloadController.cs)
- [x] T009 Configurar el ruteo de controladores en [ContosoDashboard/Program.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Program.cs) para dar soporte al controlador de descargas.

---

## Phase 3: User Story 1 - Carga y Almacenamiento Seguro (Priority: P1) 🎯 MVP

**Meta**: Permitir subir archivos locales y registrar metadatos en la base de datos de manera segura y controlada.

**Prueba Independiente**: Ejecutar la subida de un documento de prueba y comprobar que se almacena físicamente en la carpeta `AppData/uploads/{userId}/personal/{guid}.ext` y que su registro aparece en la base de datos con los datos de tamaño y tipo correctos.

- [x] T010 [US1] Implementar el método de subida `UploadDocumentAsync` en [ContosoDashboard/Services/DocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs) (con validaciones de tipo, tamaño y transacciones)
- [x] T011 [US1] Diseñar y crear el modal y el formulario de subida de archivos en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)
- [x] T012 [US1] Registrar registros de auditoría obligatorios para subida de archivos en [ContosoDashboard/Services/DocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs)

**Checkpoint**: Al completar esta fase, los archivos pueden ser cargados de forma segura mediante código de backend y frontend.

---

## Phase 4: User Story 2 - Visualización, Búsqueda y Descarga (Priority: P1)

**Meta**: Visualizar los archivos subidos, buscar utilizando filtros avanzados y descargarlos de forma segura con verificación de permisos.

**Prueba Independiente**: Buscar un documento por tags, presionar el botón de descarga y verificar que la descarga ocurra satisfactoriamente. Intentar descargar un archivo ajeno mediante ID directo en la URL del controlador y verificar que retorne un HTTP 403 (Acceso denegado).

- [x] T013 [US2] Implementar los métodos de consulta `GetUserDocumentsAsync` y `SearchDocumentsAsync` en [ContosoDashboard/Services/DocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs)
- [x] T014 [US2] Diseñar la vista principal de la lista de documentos cargados en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)
- [x] T015 [US2] Integrar filtros de búsqueda por título, tags y proyectos en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)
- [x] T016 [US2] Conectar los enlaces de descarga de los documentos al controlador seguro [ContosoDashboard/Controllers/DocumentDownloadController.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Controllers/DocumentDownloadController.cs)

**Checkpoint**: En este punto, los usuarios pueden cargar, buscar y descargar sus propios archivos de forma segura.

---

## Phase 5: User Story 3 - Compartir y Gestionar Archivos Compartidos (Priority: P2)

**Meta**: Permitir compartir documentos con usuarios o departamentos, visualizarlos en una pestaña dedicada de compartidos y borrar archivos.

**Prueba Independiente**: Compartir un documento con otro usuario de pruebas, iniciar sesión con el destinatario y comprobar que aparece en su lista de compartidos y que recibe una notificación en el sistema.

- [x] T017 [US3] Implementar los métodos de compartición y eliminación física/lógica `ShareDocumentAsync` y `DeleteDocumentAsync` en [ContosoDashboard/Services/DocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs)
- [x] T018 [US3] Crear el cuadro de diálogo para compartir con usuarios/departamentos en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)
- [x] T019 [US3] Agregar la pestaña de "Compartidos conmigo" en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)
- [x] T020 [US3] Integrar la creación automática de notificaciones internas en el sistema tras compartir en [ContosoDashboard/Services/DocumentService.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs)

**Checkpoint**: La funcionalidad de colaboración (compartir y notificar) está 100% activa.

---

## Phase 6: User Story 4 - Integración con Tareas y Dashboard (Priority: P2)

**Meta**: Habilitar la asociación de documentos directamente en el detalle de las tareas, los proyectos y en los widgets principales de inicio.

**Prueba Independiente**: Adjuntar un archivo en una tarea específica y confirmar que se puede descargar desde los detalles del proyecto y que figura en el widget de "Documentos Recientes" del inicio del usuario.

- [x] T021 [US4] Modificar el componente de detalles de tareas en [ContosoDashboard/Pages/TaskDetails.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/TaskDetails.razor) para poder listar y adjuntar documentos
- [x] T022 [US4] Integrar la sección de documentos dentro del visor de proyectos en [ContosoDashboard/Pages/ProjectDetails.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/ProjectDetails.razor)
- [x] T023 [US4] Diseñar e implementar el widget de "Documentos Recientes" en el dashboard principal [ContosoDashboard/Pages/Index.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Index.razor)
- [x] T024 [US4] Actualizar las tarjetas del dashboard en [ContosoDashboard/Pages/Index.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Index.razor) para reflejar las métricas de documentos cargados

---

## Phase 7: User Story 5 - Reportes e Informes de Auditoría (Priority: P3)

**Meta**: Facilitar reportes detallados del uso de documentos para administradores.

**Prueba Independiente**: Iniciar sesión como administrador y constatar la visualización de la lista de logs de auditoría de documentos.

- [x] T025 [US5] Crear la pestaña de auditoría visible solo para administradores en [ContosoDashboard/Pages/Documents.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)

---

## Phase 8: Polish & Cross-Cutting Concerns

**Propósito**: Ajustes visuales, enlaces del menú lateral y pruebas finales de integración.

- [x] T026 Agregar el enlace del menú para la sección de Documentos en la barra lateral [ContosoDashboard/Shared/NavMenu.razor](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Shared/NavMenu.razor)
- [x] T027 Realizar una revisión de diseño, asegurando el cumplimiento de la Constitución del proyecto y limpiando archivos temporales.

---

## Dependencias y Orden de Ejecución

### Dependencias de Fases
* **Setup (Phase 1)**: Sin dependencias, puede iniciar de inmediato.
* **Foundational (Phase 2)**: Depende de completar Setup. **BLOQUEA** toda la lógica posterior.
* **User Stories (Phase 3 a 7)**: Dependen de la finalización de Foundational (Phase 2). Se ejecutan preferentemente de forma secuencial por prioridad (US1 -> US2 -> US3 -> US4 -> US5).
* **Polish (Phase 8)**: Depende de que todas las historias de usuario deseadas estén completas.

### Oportunidades de Paralelismo
* Las tareas marcadas con `[P]` se pueden implementar simultáneamente porque modifican archivos distintos sin dependencias entre sí (ej. T001 y T002 en la fase 1, o T003 y T005 en la fase 2).

---

## Estrategia de Implementación

### 1. MVP Primero (Historias de Usuario 1 y 2 únicamente)
1. Completar Phase 1 y Phase 2.
2. Completar Phase 3 (US1: Carga).
3. Completar Phase 4 (US2: Búsqueda y Descarga).
4. **Detener y Validar**: Comprobar el correcto almacenamiento físico y la seguridad contra IDOR.

### 2. Entrega Incremental
1. Integrar compartición de archivos (Phase 5).
2. Integrar vistas de tareas y widgets en Dashboard (Phase 6).
3. Integrar reportes de administración (Phase 7).
4. Pulido de UI y navegación (Phase 8).
