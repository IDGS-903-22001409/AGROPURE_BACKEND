using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings - CORREGIDO
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString())); // CONVIERTE ENUM A STRING

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => PasswordHelper.HashPassword(src.Password)));

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews.Where(r => r.IsApproved)))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Where(r => r.IsApproved).Any() ?
                    src.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0));

            CreateMap<CreateProductDto, Product>();

            // ProductMaterial mappings
            CreateMap<ProductMaterial, ProductMaterialDto>()
                .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material.Name))
                .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.Material.UnitCost))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Material.Unit));

            // Quote mappings
            CreateMap<Quote, QuoteDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

            CreateMap<CreateQuoteDto, Quote>();

            // Review mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

            // Material mappings
            CreateMap<Material, MaterialDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));
            CreateMap<CreateMaterialDto, Material>();

            // Supplier mappings
            CreateMap<Supplier, SupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();

            // Sale mappings
            CreateMap<Sale, SaleDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CreateSaleDto, Sale>();
        }
    }
}