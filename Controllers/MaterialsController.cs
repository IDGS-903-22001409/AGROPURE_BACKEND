using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MaterialsController : ControllerBase
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public MaterialsController(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<MaterialDto>>> GetMaterials()
        {
            try
            {
                var materials = await _context.Materials
                    .Include(m => m.Supplier)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                var materialDtos = _mapper.Map<List<MaterialDto>>(materials);
                return Ok(materialDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDto>> GetMaterial(int id)
        {
            try
            {
                var material = await _context.Materials
                    .Include(m => m.Supplier)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

                if (material == null)
                {
                    return NotFound(new { message = "Material no encontrado" });
                }

                var materialDto = _mapper.Map<MaterialDto>(material);
                return Ok(materialDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] CreateMaterialDto createDto)
        {
            try
            {
                // Verificar que el proveedor existe
                var supplier = await _context.Suppliers.FindAsync(createDto.SupplierId);
                if (supplier == null || !supplier.IsActive)
                {
                    return BadRequest(new { message = "Proveedor no válido" });
                }

                var material = _mapper.Map<Material>(createDto);
                material.CreatedAt = DateTime.UtcNow;

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();

                // Recargar con el proveedor incluido
                var createdMaterial = await _context.Materials
                    .Include(m => m.Supplier)
                    .FirstAsync(m => m.Id == material.Id);

                var materialDto = _mapper.Map<MaterialDto>(createdMaterial);
                return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, materialDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(int id, [FromBody] CreateMaterialDto updateDto)
        {
            try
            {
                var material = await _context.Materials
                    .Include(m => m.Supplier)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

                if (material == null)
                {
                    return NotFound(new { message = "Material no encontrado" });
                }

                // Verificar que el proveedor existe
                if (updateDto.SupplierId != material.SupplierId)
                {
                    var supplier = await _context.Suppliers.FindAsync(updateDto.SupplierId);
                    if (supplier == null || !supplier.IsActive)
                    {
                        return BadRequest(new { message = "Proveedor no válido" });
                    }
                }

                _mapper.Map(updateDto, material);
                await _context.SaveChangesAsync();

                // Recargar con el proveedor incluido
                await _context.Entry(material).Reference(m => m.Supplier).LoadAsync();

                var materialDto = _mapper.Map<MaterialDto>(material);
                return Ok(materialDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMaterial(int id)
        {
            try
            {
                var material = await _context.Materials.FindAsync(id);
                if (material == null)
                {
                    return NotFound(new { message = "Material no encontrado" });
                }

                material.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Material eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}