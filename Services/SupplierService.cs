using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Services
{
    public class SupplierService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public SupplierService(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SupplierDto>> GetAllSuppliersAsync()
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

            return supplierDtos;
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Materials)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (supplier == null) return null;

            return new SupplierDto
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
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto)
        {
            var supplier = _mapper.Map<Supplier>(createDto);
            supplier.CreatedAt = DateTime.UtcNow;

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int id, CreateSupplierDto updateDto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null || !supplier.IsActive)
            {
                throw new KeyNotFoundException("Proveedor no encontrado");
            }

            _mapper.Map(updateDto, supplier);
            await _context.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return false;

            supplier.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}