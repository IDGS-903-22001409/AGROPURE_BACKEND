using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Services
{
    public class ProductService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public ProductService(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Materials)
                    .ThenInclude(pm => pm.Material)
                        .ThenInclude(m => m.Supplier)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Materials)
                    .ThenInclude(pm => pm.Material)
                        .ThenInclude(m => m.Supplier)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            var product = _mapper.Map<Product>(createDto);
            product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Agregar materiales si se proporcionaron
            if (createDto.Materials.Any())
            {
                var productMaterials = createDto.Materials.Select(m => new ProductMaterial
                {
                    ProductId = product.Id,
                    MaterialId = m.MaterialId,
                    Quantity = m.Quantity
                }).ToList();

                _context.ProductMaterials.AddRange(productMaterials);
                await _context.SaveChangesAsync();
            }

            return await GetProductByIdAsync(product.Id) ?? throw new InvalidOperationException("Error al crear el producto");
        }

        public async Task<ProductDto> UpdateProductAsync(int id, CreateProductDto updateDto)
        {
            var product = await _context.Products
                .Include(p => p.Materials)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                throw new KeyNotFoundException("Producto no encontrado");
            }

            _mapper.Map(updateDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            // Actualizar materiales
            _context.ProductMaterials.RemoveRange(product.Materials);

            if (updateDto.Materials.Any())
            {
                var newMaterials = updateDto.Materials.Select(m => new ProductMaterial
                {
                    ProductId = product.Id,
                    MaterialId = m.MaterialId,
                    Quantity = m.Quantity
                }).ToList();

                _context.ProductMaterials.AddRange(newMaterials);
            }

            await _context.SaveChangesAsync();
            return await GetProductByIdAsync(id) ?? throw new InvalidOperationException("Error al actualizar el producto");
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}