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
    public class SuppliersController : ControllerBase
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public SuppliersController(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<SupplierDto>>> GetSuppliers()
        {
            try
            {
                var suppliers = await _context.Suppliers
                    .Include(s => s.Materials)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var supplierDtos = suppliers.Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    MaterialsCount = s.Materials.Count(m => m.IsActive)
                }).ToList();

                return Ok(supplierDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Include(s => s.Materials)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (supplier == null)
                {
                    return NotFound(new { message = "Proveedor no encontrado" });
                }

                var supplierDto = new SupplierDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive,
                    CreatedAt = supplier.CreatedAt,
                    MaterialsCount = supplier.Materials.Count(m => m.IsActive)
                };

                return Ok(supplierDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto createDto)
        {
            try
            {
                var supplier = _mapper.Map<Supplier>(createDto);
                supplier.CreatedAt = DateTime.UtcNow;

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                var supplierDto = _mapper.Map<SupplierDto>(supplier);
                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplierDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SupplierDto>> UpdateSupplier(int id, [FromBody] CreateSupplierDto updateDto)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null || !supplier.IsActive)
                {
                    return NotFound(new { message = "Proveedor no encontrado" });
                }

                _mapper.Map(updateDto, supplier);
                await _context.SaveChangesAsync();

                var supplierDto = _mapper.Map<SupplierDto>(supplier);
                return Ok(supplierDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                {
                    return NotFound(new { message = "Proveedor no encontrado" });
                }

                supplier.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Proveedor eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}