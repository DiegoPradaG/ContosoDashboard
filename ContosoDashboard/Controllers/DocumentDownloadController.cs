using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoDashboard.Services;

namespace ContosoDashboard.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/documents")]
    public class DocumentDownloadController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IFileStorageService _fileStorageService;

        public DocumentDownloadController(IDocumentService documentService, IFileStorageService fileStorageService)
        {
            _documentService = documentService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var document = await _documentService.GetDocumentByIdAsync(id, userId);
            if (document == null)
            {
                return Forbid("No tienes permiso para acceder a este documento o no existe.");
            }

            try
            {
                var fileStream = await _fileStorageService.DownloadAsync(document.FilePath);
                return File(fileStream, document.FileType, document.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("El archivo físico no se encuentra en el almacenamiento.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al descargar el archivo: {ex.Message}");
            }
        }
    }
}
