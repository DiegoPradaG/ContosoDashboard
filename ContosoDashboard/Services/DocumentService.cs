using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;

        public DocumentService(
            ApplicationDbContext context, 
            IFileStorageService fileStorageService,
            INotificationService notificationService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
        }

        public async Task<bool> UploadDocumentAsync(string title, string description, string category, 
            string originalFileName, string contentType, Stream fileStream, 
            int uploadedByUserId, int? projectId = null, string tags = null, int? taskId = null)
        {
            // Validations
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
            {
                return false;
            }

            var allowedCategories = new[] { "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other" };
            if (!allowedCategories.Contains(category))
            {
                return false;
            }

            // Task resolution and project auto-association
            if (taskId.HasValue)
            {
                var task = await _context.Tasks.FindAsync(taskId.Value);
                if (task == null)
                {
                    return false;
                }
                projectId = task.ProjectId;
            }

            // Project authorization check
            if (projectId.HasValue)
            {
                var isAuthorized = await IsUserProjectMemberOrPmAsync(uploadedByUserId, projectId.Value);
                if (!isAuthorized)
                {
                    return false;
                }
            }

            // File upload sequence (Constitución: Guardar físico primero para evitar huérfanos)
            var extension = Path.GetExtension(originalFileName);
            var guidFilename = $"{Guid.NewGuid()}{extension}";
            var folderPrefix = projectId.HasValue ? projectId.Value.ToString() : "personal";
            var relativePath = $"{uploadedByUserId}/{folderPrefix}/{guidFilename}";

            try
            {
                // Save to physical storage
                await _fileStorageService.UploadAsync(fileStream, relativePath, contentType);

                // Create database record
                var document = new Document
                {
                    Title = title,
                    Description = description,
                    Category = category,
                    FileName = originalFileName,
                    FilePath = relativePath,
                    FileType = contentType,
                    FileSize = fileStream.Length,
                    UploadDate = DateTime.UtcNow,
                    UploadedByUserId = uploadedByUserId,
                    ProjectId = projectId,
                    TaskId = taskId,
                    Tags = tags
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Audit Log
                await LogAuditAsync(uploadedByUserId, "Upload", $"Uploaded document '{title}' (ID: {document.DocumentId})");

                // Notifications
                if (projectId.HasValue)
                {
                    await NotifyProjectMembersAsync(projectId.Value, uploadedByUserId, document);
                }

                return true;
            }
            catch (Exception)
            {
                // Clean up physical file if database save fails
                try
                {
                    await _fileStorageService.DeleteAsync(relativePath);
                }
                catch { }
                
                throw;
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
        {
            var document = await _context.Documents
                .Include(d => d.UploadedByUser)
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null) return null;

            var canAccess = await CanUserAccessDocumentAsync(document, requestingUserId);
            if (!canAccess)
            {
                return null; // IDOR Protection
            }

            return document;
        }

        public async Task<List<Document>> GetUserDocumentsAsync(int userId)
        {
            return await _context.Documents
                .Include(d => d.Project)
                .Where(d => d.UploadedByUserId == userId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId)
        {
            var isAuthorized = await IsUserProjectMemberOrPmAsync(requestingUserId, projectId);
            if (!isAuthorized)
            {
                // Check if requesting user is Administrator
                var user = await _context.Users.FindAsync(requestingUserId);
                if (user == null || user.Role != UserRole.Administrator)
                {
                    return new List<Document>(); // Unauthorized
                }
            }

            return await _context.Documents
                .Include(d => d.UploadedByUser)
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<List<Document>> GetSharedDocumentsAsync(int userId, string department)
        {
            // Fetch documents shared individually or by department
            var sharedDocIds = await _context.DocumentShares
                .Where(ds => ds.SharedWithUserId == userId || 
                             (!string.IsNullOrEmpty(ds.SharedWithDepartment) && ds.SharedWithDepartment == department))
                .Select(ds => ds.DocumentId)
                .Distinct()
                .ToListAsync();

            return await _context.Documents
                .Include(d => d.UploadedByUser)
                .Include(d => d.Project)
                .Where(d => sharedDocIds.Contains(d.DocumentId))
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateDocumentMetadataAsync(int documentId, string title, string description, 
            string category, string tags, int requestingUserId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            // Only owner or Admin can update metadata
            var user = await _context.Users.FindAsync(requestingUserId);
            if (document.UploadedByUserId != requestingUserId && (user == null || user.Role != UserRole.Administrator))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
            {
                return false;
            }

            var allowedCategories = new[] { "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other" };
            if (!allowedCategories.Contains(category))
            {
                return false;
            }

            document.Title = title;
            document.Description = description;
            document.Category = category;
            document.Tags = tags;

            await _context.SaveChangesAsync();
            await LogAuditAsync(requestingUserId, "Update", $"Updated metadata of document ID {documentId}");

            return true;
        }

        public async Task<bool> ShareDocumentAsync(int documentId, int? targetUserId, string targetDepartment, 
            int requestingUserId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            // Only owner or Admin can share
            var user = await _context.Users.FindAsync(requestingUserId);
            if (document.UploadedByUserId != requestingUserId && (user == null || user.Role != UserRole.Administrator))
            {
                return false;
            }

            // Create share record
            var share = new DocumentShare
            {
                DocumentId = documentId,
                SharedWithUserId = targetUserId,
                SharedWithDepartment = targetDepartment,
                SharedDate = DateTime.UtcNow
            };

            _context.DocumentShares.Add(share);
            await _context.SaveChangesAsync();

            await LogAuditAsync(requestingUserId, "Share", $"Shared document ID {documentId} with User: {targetUserId}, Department: {targetDepartment}");

            // Notify Target User
            if (targetUserId.HasValue)
            {
                await _notificationService.CreateNotificationAsync(new Notification
                {
                    UserId = targetUserId.Value,
                    Title = "Documento Compartido",
                    Message = $"{user?.DisplayName ?? "Un usuario"} ha compartido el documento '{document.Title}' contigo.",
                    Type = NotificationType.SystemAnnouncement,
                    Priority = NotificationPriority.Important
                });
            }

            // Notify Department Members
            if (!string.IsNullOrEmpty(targetDepartment))
            {
                var deptMembers = await _context.Users
                    .Where(u => u.Department == targetDepartment && u.UserId != requestingUserId)
                    .ToListAsync();

                foreach (var member in deptMembers)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = member.UserId,
                        Title = "Recurso de Equipo Compartido",
                        Message = $"Se ha compartido el documento '{document.Title}' con tu departamento ({targetDepartment}).",
                        Type = NotificationType.SystemAnnouncement,
                        Priority = NotificationPriority.Informational
                    });
                }
            }

            return true;
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            // Authorization: Owner, Admin, or Project Manager of associated project can delete
            var isOwner = document.UploadedByUserId == requestingUserId;
            var user = await _context.Users.FindAsync(requestingUserId);
            var isAdmin = user != null && user.Role == UserRole.Administrator;
            
            var isPm = false;
            if (document.ProjectId.HasValue)
            {
                var project = await _context.Projects.FindAsync(document.ProjectId.Value);
                isPm = project != null && project.ProjectManagerId == requestingUserId;
            }

            if (!isOwner && !isAdmin && !isPm)
            {
                return false;
            }

            // Delete physical file first
            try
            {
                await _fileStorageService.DeleteAsync(document.FilePath);
            }
            catch (Exception)
            {
                // Log and continue, do not block DB deletion
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            await LogAuditAsync(requestingUserId, "Delete", $"Deleted document '{document.Title}' (ID: {documentId})");

            return true;
        }

        public async Task<List<Document>> SearchDocumentsAsync(string query, int requestingUserId)
        {
            var user = await _context.Users.FindAsync(requestingUserId);
            if (user == null) return new List<Document>();

            var isAdmin = user.Role == UserRole.Administrator;

            // Get projects user belongs to
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == requestingUserId)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            var managedProjectIds = await _context.Projects
                .Where(p => p.ProjectManagerId == requestingUserId)
                .Select(p => p.ProjectId)
                .ToListAsync();

            var allowedProjectIds = userProjectIds.Union(managedProjectIds).Distinct().ToList();

            // Get documents shared with user or their department
            var sharedDocIds = await _context.DocumentShares
                .Where(ds => ds.SharedWithUserId == requestingUserId || 
                             (!string.IsNullOrEmpty(ds.SharedWithDepartment) && ds.SharedWithDepartment == user.Department))
                .Select(ds => ds.DocumentId)
                .Distinct()
                .ToListAsync();

            // Form query
            var docQuery = _context.Documents
                .Include(d => d.UploadedByUser)
                .Include(d => d.Project)
                .AsQueryable();

            // Enforce IDOR / Permission Filter
            if (!isAdmin)
            {
                docQuery = docQuery.Where(d => 
                    d.UploadedByUserId == requestingUserId || 
                    (d.ProjectId.HasValue && allowedProjectIds.Contains(d.ProjectId.Value)) ||
                    sharedDocIds.Contains(d.DocumentId)
                );
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                docQuery = docQuery.Where(d => 
                    d.Title.ToLower().Contains(lowerQuery) || 
                    (d.Description != null && d.Description.ToLower().Contains(lowerQuery)) ||
                    (d.Tags != null && d.Tags.ToLower().Contains(lowerQuery)) ||
                    d.UploadedByUser.DisplayName.ToLower().Contains(lowerQuery) ||
                    (d.Project != null && d.Project.Name.ToLower().Contains(lowerQuery))
                );
            }

            return await docQuery
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(int requestingUserId)
        {
            var user = await _context.Users.FindAsync(requestingUserId);
            if (user == null || user.Role != UserRole.Administrator)
            {
                return new List<AuditLog>(); // Administrators only
            }

            return await _context.AuditLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Take(100)
                .ToListAsync();
        }

        // --- Helper Methods ---

        private async Task<bool> IsUserProjectMemberOrPmAsync(int userId, int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;
            if (project.ProjectManagerId == userId) return true;
            return await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        private async Task<bool> CanUserAccessDocumentAsync(Document document, int requestingUserId)
        {
            var user = await _context.Users.FindAsync(requestingUserId);
            if (user == null) return false;

            if (user.Role == UserRole.Administrator) return true;
            if (document.UploadedByUserId == requestingUserId) return true;

            if (document.ProjectId.HasValue)
            {
                if (await IsUserProjectMemberOrPmAsync(requestingUserId, document.ProjectId.Value))
                {
                    return true;
                }
            }

            var isShared = await _context.DocumentShares.AnyAsync(ds => 
                ds.DocumentId == document.DocumentId && 
                (ds.SharedWithUserId == requestingUserId || 
                 (!string.IsNullOrEmpty(ds.SharedWithDepartment) && ds.SharedWithDepartment == user.Department))
            );

            return isShared;
        }

        private async Task LogAuditAsync(int userId, string action, string details)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };
                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch { }
        }

        private async Task NotifyProjectMembersAsync(int projectId, int uploaderId, Document document)
        {
            try
            {
                var pmUser = await _context.Projects
                    .Where(p => p.ProjectId == projectId)
                    .Select(p => p.ProjectManagerId)
                    .FirstOrDefaultAsync();

                var projectMembers = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId && pm.UserId != uploaderId)
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                if (pmUser != 0 && pmUser != uploaderId && !projectMembers.Contains(pmUser))
                {
                    projectMembers.Add(pmUser);
                }

                var uploader = await _context.Users.FindAsync(uploaderId);

                foreach (var userId in projectMembers)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = userId,
                        Title = "Nuevo documento de proyecto",
                        Message = $"{uploader?.DisplayName ?? "Un usuario"} ha cargado '{document.Title}' en el proyecto.",
                        Type = NotificationType.ProjectUpdate,
                        Priority = NotificationPriority.Informational
                    });
                }
            }
            catch { }
        }
    }
}
