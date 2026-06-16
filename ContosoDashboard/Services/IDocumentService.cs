using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services
{
    public interface IDocumentService
    {
        Task<bool> UploadDocumentAsync(string title, string description, string category, 
            string originalFileName, string contentType, Stream fileStream, 
            int uploadedByUserId, int? projectId = null, string tags = null, int? taskId = null);

        Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);

        Task<List<Document>> GetUserDocumentsAsync(int userId);

        Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId);

        Task<List<Document>> GetSharedDocumentsAsync(int userId, string department);

        Task<bool> UpdateDocumentMetadataAsync(int documentId, string title, string description, 
            string category, string tags, int requestingUserId);

        Task<bool> ShareDocumentAsync(int documentId, int? targetUserId, string targetDepartment, 
            int requestingUserId);

        Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);

        Task<List<Document>> SearchDocumentsAsync(string query, int requestingUserId);

        Task<List<AuditLog>> GetAuditLogsAsync(int requestingUserId);
    }
}
