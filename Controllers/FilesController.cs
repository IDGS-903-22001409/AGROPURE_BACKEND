using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;

        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
        }
        
        [HttpPost("upload-product-image")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> UploadProductImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No se ha seleccionado ninguna imagen" });
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Solo se permiten imágenes (JPG, PNG, GIF)" });
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "El archivo excede el tamaño máximo permitido (5MB)" });
                }
                                
                var fileName = $"{Guid.NewGuid()}{extension}";
                var imageUrl = $"/images/products/{fileName}";

                _logger.LogInformation($"Imagen validada correctamente: {file.FileName}");

                return Ok(new
                {
                    fileName = fileName,
                    imageUrl = imageUrl,
                    originalName = file.FileName,
                    size = file.Length,
                    message = "Imagen validada correctamente (simulado)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar imagen del producto");
                return BadRequest(new { message = "Error al procesar la imagen" });
            }
        }
        
        [HttpPost("validate-file")]
        [Authorize]
        public ActionResult ValidateFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No se ha seleccionado ningún archivo" });
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Tipo de archivo no permitido" });
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "El archivo excede el tamaño máximo permitido (10MB)" });
                }

                return Ok(new
                {
                    valid = true,
                    fileName = file.FileName,
                    size = file.Length,
                    type = file.ContentType,
                    message = "Archivo válido"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar archivo");
                return BadRequest(new { message = "Error al validar el archivo" });
            }
        }
    }
}