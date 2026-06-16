using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContosoDashboard.Services
{
    /// <summary>
    /// Servicio abstracto para la gestión física de archivos.
    /// Permite intercambiar el motor de almacenamiento (Local vs Cloud).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo físico en el almacenamiento.
        /// </summary>
        /// <param name="fileStream">Flujo de datos del archivo.</param>
        /// <param name="fileName">Nombre físico único (generalmente un GUID con extensión).</param>
        /// <param name="contentType">Tipo MIME del archivo.</param>
        /// <returns>La ruta relativa o URL pública de acceso al archivo.</returns>
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Descarga el flujo de bytes de un archivo físico.
        /// </summary>
        /// <param name="filePath">Ruta de almacenamiento del archivo.</param>
        /// <returns>Flujo de lectura del archivo.</returns>
        Task<Stream> DownloadAsync(string filePath);

        /// <summary>
        /// Elimina físicamente un archivo del almacenamiento.
        /// </summary>
        /// <param name="filePath">Ruta de almacenamiento del archivo.</param>
        Task DeleteAsync(string filePath);
    }

    /// <summary>
    /// Servicio de lógica de negocio para la gestión de documentos y su metadatos.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Registra y guarda un nuevo documento en el sistema.
        /// </summary>
        Task<bool> UploadDocumentAsync(string title, string description, string category, 
            string originalFileName, string contentType, Stream fileStream, 
            int uploadedByUserId, int? projectId = null, string tags = null);

        /// <summary>
        /// Obtiene un documento por su ID si el usuario está autorizado.
        /// </summary>
        Task<object> GetDocumentByIdAsync(int documentId, int requestingUserId);

        /// <summary>
        /// Obtiene la lista de documentos subidos por un usuario específico.
        /// </summary>
        Task<List<object>> GetUserDocumentsAsync(int userId);

        /// <summary>
        /// Obtiene todos los documentos asociados a un proyecto.
        /// </summary>
        Task<List<object>> GetProjectDocumentsAsync(int projectId, int requestingUserId);

        /// <summary>
        /// Modifica los metadatos de un archivo (Título, Descripción, Categoría, Tags).
        /// </summary>
        Task<bool> UpdateDocumentMetadataAsync(int documentId, string title, string description, 
            string category, string tags, int requestingUserId);

        /// <summary>
        /// Comparte un documento con un usuario o con un departamento.
        /// </summary>
        Task<bool> ShareDocumentAsync(int documentId, int? targetUserId, string targetDepartment, 
            int requestingUserId);

        /// <summary>
        /// Elimina un documento (física y lógicamente en BD).
        /// </summary>
        Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);

        /// <summary>
        /// Realiza una búsqueda segura de documentos filtrando por criterios de permisos.
        /// </summary>
        Task<List<object>> SearchDocumentsAsync(string query, int requestingUserId);
    }
}
