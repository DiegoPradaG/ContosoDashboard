# Investigación Técnica (Phase 0): Carga y Gestión de Documentos

Este documento consolida las decisiones técnicas tomadas para abordar las restricciones y necesidades del sistema de gestión de documentos.

---

## 1. Servir Archivos Fuera de `wwwroot` de Forma Segura

### Decisión
Servir los archivos cargados mediante un controlador MVC controlador clásico (`DocumentDownloadController`) que escuche en `/api/documents/download/{id}`. Este controlador inyectará `IUserService` e `IDocumentService` para verificar la identidad del usuario y determinar si tiene acceso al documento antes de retornar el archivo como un flujo de bytes (`FileStreamResult`).

### Justificación
Las directivas de seguridad exigen que los archivos no se expongan públicamente en `wwwroot` para prevenir accesos no autorizados. Al ubicarlos en una ruta del sistema de archivos local (`AppData/uploads`) fuera de la raíz web, el servidor web no puede servirlos estáticamente. El controlador actúa como una compuerta de seguridad, validando que el usuario tenga acceso legítimo antes de leer el archivo físico del disco.

### Alternativas Consideradas
* **Exponer la carpeta física usando Middleware de Archivos Estáticos:** Configurar `app.UseStaticFiles()` con una ruta física personalizada. *Rechazado* porque expone los archivos de forma pública a cualquier persona que conozca o adivine la URL del archivo, lo que viola directamente el requisito de protección contra IDOR.

---

## 2. Abstracción y Flexibilidad de Almacenamiento

### Decisión
Definir la interfaz `IFileStorageService` con los métodos abstractos `UploadAsync`, `DownloadAsync` y `DeleteAsync`. Implementar `LocalFileStorageService` utilizando operaciones sincrónicas/asincrónicas de `System.IO` para la fase local y de entrenamiento.

### Justificación
Esto permite cumplir con la arquitectura offline-first del proyecto para desarrollo y talleres. Al mismo tiempo, asegura que el paso a producción (ej. usar Azure Blob Storage) requiera únicamente implementar un nuevo servicio que consuma el SDK de Azure (`AzureBlobStorageService`) e intercambiar el registro de dependencias en `Program.cs`.

### Alternativas Consideradas
* **Acceso directo a System.IO en el servicio de documentos:** *Rechazado* porque acopla de manera permanente el código del negocio al sistema de archivos local de Windows, haciendo costosa una futura migración a la nube.

---

## 3. Prevención de Registros Huérfanos y Colisiones

### Decisión
1. Generar un GUID único para el nombre físico del archivo (ej. `3fa85f64-5717-4562-b3fc-2c963f66afa6.pdf`) antes de escribir a disco o base de datos.
2. Seguir una secuencia estricta en el servicio: **Validar archivo → Generar ruta física basada en GUID → Guardar físicamente en disco → Registrar metadatos en la base de datos**.

### Justificación
Guardar el archivo físicamente primero asegura que no creemos entradas en la base de datos que apunten a archivos inexistentes si ocurre un error de escritura. El uso de nombres basados en GUID evita colisiones si dos usuarios suben un archivo con el mismo nombre y previene ataques de path traversal.

### Alternativas Consideradas
* **Guardar metadatos en BD antes de escribir a disco:** *Rechazado* porque si la escritura a disco falla (por permisos o falta de espacio), el registro de metadatos en la base de datos quedaría guardado y corrompido, apuntando a un archivo que no existe.

---

## 4. Gestión de Estado en Cargas de Blazor Server

### Decisión
En el componente `InputFile` de Blazor, extraer los metadatos básicos (`Name`, `Size`, `ContentType`) a variables locales en cuanto se seleccione el archivo, copiar inmediatamente el stream a un `MemoryStream` local, y limpiar la referencia al objeto `IBrowserFile` asignándole valor `null` antes de forzar la renderización del componente.

### Justificación
Blazor Server maneja la subida a través de conexiones SignalR. Si la conexión fluctúa o la referencia al archivo de entrada se mantiene activa durante la ejecución del guardado lento en disco, pueden ocurrir excepciones de ciclo de vida del componente o fugas de memoria. Copiar los datos de inmediato a un buffer local previene estos problemas.

### Alternativas Consideradas
* **Mantener la referencia a IBrowserFile durante todo el proceso de guardado:** *Rechazado* por ser una fuente recurrente de excepciones de persistencia de flujos en aplicaciones Blazor Server.
