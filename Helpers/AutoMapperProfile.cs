using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
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
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

            // Material mappings
            CreateMap<Material, MaterialDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));

            // Supplier mappings
            CreateMap<Supplier, SupplierDto>();
        }
    }
}
