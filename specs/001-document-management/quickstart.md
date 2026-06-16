# Guía Rápida (Quickstart): Carga y Gestión de Documentos

Esta guía detalla los pasos necesarios para configurar, compilar y probar la funcionalidad de documentos en tu entorno de desarrollo.

---

## 1. Configuración Inicial

### 1.1 Configuración de Rutas de Almacenamiento
Añade la ruta del almacenamiento físico al archivo [appsettings.json](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/appsettings.json):
```json
"DocumentSettings": {
  "UploadDirectory": "AppData/uploads"
}
```

### 1.2 Limpieza del Estado de Base de Datos
Dado que modificaremos el DbContext agregando entidades e inicializando con datos semilla, se recomienda restablecer la base de datos local SQLite para evitar conflictos de claves primarias:
1. Detén cualquier instancia activa de la aplicación.
2. Elimina el archivo `ContosoDashboard.db` si ya existe en la raíz de [ContosoDashboard](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard).
3. La base de datos y sus tablas se crearán automáticamente con el nuevo esquema en el siguiente inicio gracias a `context.Database.EnsureCreated()`.

---

## 2. Inyección de Dependencias

Registra los nuevos servicios en [Program.cs](file:///c:/Users/dprada/Documents/GLocation/ContosoDashboard/ContosoDashboard/Program.cs):

```csharp
// Registrar servicios de documentos
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
```

---

## 3. Compilación y Ejecución

Ejecuta los siguientes comandos en la consola dentro del directorio de la aplicación:

1. **Compilar el proyecto:**
   ```powershell
   dotnet build
   ```
2. **Iniciar la aplicación:**
   ```powershell
   dotnet run
   ```
3. **Acceder a la aplicación:**
   Abre tu navegador en `http://localhost:5000` o `https://localhost:7111`.

---

## 4. Pruebas de Flujo Funcional (Paso a Paso)

### 4.1 Carga Inicial y Validación
1. Inicia sesión con el usuario semilla **Camille Nicole** (Project Manager).
2. Ve a la nueva sección **Documentos** en la barra lateral.
3. Haz clic en "Subir Documento".
4. Selecciona un archivo (ej. un PDF de pruebas de 2MB) y completa:
   * **Título**: "Presupuesto de Desarrollo"
   * **Categoría**: "Project Documents"
   * **Proyecto Asociado**: "ContosoDashboard Development"
5. Confirma la subida. Verifica que el archivo aparezca en la lista "Mis Documentos".
6. Navega a la carpeta física `ContosoDashboard/AppData/uploads` y comprueba que se haya creado la estructura de carpetas: `{userId}/{projectId}/{guid}.pdf`.

### 4.2 Validación de Seguridad (Prevención de IDOR)
1. Copia el enlace de descarga de la lista de documentos de Camille Nicole.
2. Cierra la sesión e inicia sesión como **Ni Kang** (Employee).
3. Pega el enlace en la barra de direcciones del navegador.
4. **Resultado Esperado**: Si Ni Kang no es miembro del proyecto "ContosoDashboard Development", el sistema debe denegar el acceso mostrando un error HTTP 403 (Prohibido). Si es miembro del proyecto, la descarga debe ser exitosa.

### 4.3 Integración con Tareas
1. Ve a la sección **Tareas**.
2. Selecciona cualquier tarea asignada y abre sus detalles.
3. Utiliza la sección "Documentos Adjuntos" para subir un archivo.
4. Verifica que el archivo cargado se asocie automáticamente a la vista de documentos globales de ese Proyecto.
