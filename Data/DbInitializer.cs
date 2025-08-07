using AGROPURE.Helpers;
using AGROPURE.Models.Entities;
using AGROPURE.Models.Enums;

namespace AGROPURE.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AgroContext context)
        {
            // Verificar si ya hay datos
            if (context.Users.Any())
            {
                return; // La base de datos ya fue inicializada
            }

            // Crear usuario administrador
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "AGROPURE",
                Email = "admin@agropure.com",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Crear usuario cliente de prueba
            var testCustomer = new User
            {
                FirstName = "Juan",
                LastName = "Pérez",
                Email = "juan@example.com",
                PasswordHash = PasswordHelper.HashPassword("customer123"),
                Phone = "4771234567",
                Company = "Granja Los Pinos",
                Address = "Av. Principal 123, León, Guanajuato",
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, testCustomer);

            // Crear proveedores
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "TechSensors MX",
                    ContactPerson = "Carlos Rodríguez",
                    Email = "ventas@techsensors.mx",
                    Phone = "4771111111",
                    Address = "Zona Industrial, León, Gto",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Supplier
                {
                    Name = "Componentes Electrónicos SA",
                    ContactPerson = "Ana García",
                    Email = "info@componentes.com",
                    Phone = "4772222222",
                    Address = "Centro Industrial, Guanajuato, Gto",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            // Crear materiales
            var materials = new List<Material>
            {
                new Material
                {
                    Name = "Sensor pH",
                    Description = "Sensor digital de pH para agua",
                    UnitCost = 450.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Material
                {
                    Name = "Sensor Turbidez",
                    Description = "Sensor de turbidez del agua",
                    UnitCost = 380.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Material
                {
                    Name = "Microcontrolador ESP32",
                    Description = "Microcontrolador con WiFi y Bluetooth",
                    UnitCost = 120.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Material
                {
                    Name = "Válvula Solenoide",
                    Description = "Válvula para control de flujo de agua",
                    UnitCost = 280.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Material
                {
                    Name = "Carcasa Impermeable",
                    Description = "Carcasa IP67 para protección",
                    UnitCost = 200.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Material
                {
                    Name = "Sistema de Filtración",
                    Description = "Sistema básico de filtros para agua",
                    UnitCost = 850.00m,
                    Unit = "pcs",
                    SupplierId = suppliers[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Materials.AddRange(materials);
            context.SaveChanges();

            // Crear productos con imágenes
            var products = new List<Product>
            {
                new Product
                {
                    Name = "AGROPURE Sistema Básico",
                    Description = "Sistema básico de monitoreo de calidad de agua para riego",
                    DetailedDescription = "Sistema IoT completo que incluye sensores de pH y turbidez, microcontrolador con conectividad WiFi, y aplicación móvil para monitoreo en tiempo real. Ideal para pequeñas parcelas y huertos urbanos.",
                    ImageUrl = "/images/products/agropure-basic.jpg",
                    BasePrice = 8500.00m,
                    Category = "Sistema Básico",
                    TechnicalSpecs = "WiFi 802.11n, Sensores digitales, Alimentación 12V, Rango pH 0-14",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "AGROPURE Sistema Avanzado",
                    Description = "Sistema avanzado con tratamiento automático de agua",
                    DetailedDescription = "Sistema IoT completo que incluye monitoreo avanzado, sistema de tratamiento automático con válvulas de control, filtración integrada y dashboard completo. Perfecto para operaciones agrícolas medianas y grandes.",
                    ImageUrl = "/images/products/agropure-advanced.jpg",
                    BasePrice = 15500.00m,
                    Category = "Sistema Avanzado",
                    TechnicalSpecs = "WiFi + Bluetooth, Múltiples sensores, Válvulas automáticas, Sistema de filtración integrado",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "AGROPURE Sensor pH Individual",
                    Description = "Sensor de pH independiente para integración personalizada",
                    DetailedDescription = "Sensor digital de pH de alta precisión para medición continua de la acidez del agua. Compatible con sistemas existentes y fácil instalación.",
                    ImageUrl = "/images/products/ph-sensor.jpg",
                    BasePrice = 1200.00m,
                    Category = "Sensores",
                    TechnicalSpecs = "Precisión ±0.1 pH, Rango 0-14 pH, Salida digital, Compensación automática de temperatura",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();

            // Crear BOM (Bill of Materials) para productos
            var productMaterials = new List<ProductMaterial>
            {
                // AGROPURE Sistema Básico
                new ProductMaterial { ProductId = products[0].Id, MaterialId = materials[0].Id, Quantity = 1 }, // Sensor pH
                new ProductMaterial { ProductId = products[0].Id, MaterialId = materials[1].Id, Quantity = 1 }, // Sensor Turbidez
                new ProductMaterial { ProductId = products[0].Id, MaterialId = materials[2].Id, Quantity = 1 }, // ESP32
                new ProductMaterial { ProductId = products[0].Id, MaterialId = materials[4].Id, Quantity = 1 }, // Carcasa

                // AGROPURE Sistema Avanzado
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[0].Id, Quantity = 2 }, // Sensor pH x2
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[1].Id, Quantity = 2 }, // Sensor Turbidez x2
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[2].Id, Quantity = 1 }, // ESP32
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[3].Id, Quantity = 2 }, // Válvula x2
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[4].Id, Quantity = 1 }, // Carcasa
                new ProductMaterial { ProductId = products[1].Id, MaterialId = materials[5].Id, Quantity = 1 }, // Sistema Filtración

                // Sensor pH Individual
                new ProductMaterial { ProductId = products[2].Id, MaterialId = materials[0].Id, Quantity = 1 }, // Sensor pH
                new ProductMaterial { ProductId = products[2].Id, MaterialId = materials[2].Id, Quantity = 1 }  // ESP32
            };

            context.ProductMaterials.AddRange(productMaterials);

            // Crear FAQs para productos
            var productFaqs = new List<ProductFaq>
            {
                // FAQs para Sistema Básico
                new ProductFaq
                {
                    ProductId = products[0].Id,
                    Question = "¿Qué incluye el sistema básico?",
                    Answer = "El sistema básico incluye sensores de pH y turbidez, microcontrolador ESP32 con WiFi, carcasa impermeable, y acceso a la aplicación móvil para monitoreo en tiempo real.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductFaq
                {
                    ProductId = products[0].Id,
                    Question = "¿Necesito conocimientos técnicos para instalarlo?",
                    Answer = "No, el sistema está diseñado para instalación fácil. Incluye manual paso a paso y soporte técnico gratuito durante el primer mes.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductFaq
                {
                    ProductId = products[0].Id,
                    Question = "¿Funciona con cualquier tipo de riego?",
                    Answer = "Sí, es compatible con sistemas de riego por goteo, aspersión y inundación. Se adapta a tuberías de 1/2\" a 2\".",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // FAQs para Sistema Avanzado
                new ProductFaq
                {
                    ProductId = products[1].Id,
                    Question = "¿Cuál es la diferencia con el sistema básico?",
                    Answer = "El sistema avanzado incluye tratamiento automático del agua, válvulas de control automático, sistema de filtración integrado, y monitoreo de múltiples parámetros adicionales.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductFaq
                {
                    ProductId = products[1].Id,
                    Question = "¿Requiere mantenimiento especial?",
                    Answer = "Mantenimiento mínimo: limpieza de sensores cada 3 meses y cambio de filtros cada 6 meses. El sistema envía recordatorios automáticos.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // FAQs para Sensor Individual
                new ProductFaq
                {
                    ProductId = products[2].Id,
                    Question = "¿Puedo integrar este sensor con mi sistema actual?",
                    Answer = "Sí, el sensor tiene salida digital estándar y es compatible con la mayoría de sistemas de automatización. Incluye documentación de integración.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.ProductFaqs.AddRange(productFaqs);

            // Crear una cotización pública de ejemplo
            var samplePublicQuote = new Quote
            {
                UserId = null, // Cotización pública
                ProductId = products[0].Id,
                CustomerName = "María González",
                CustomerEmail = "maria@granjaecologica.com",
                CustomerPhone = "4779876543",
                CustomerAddress = "Rancho La Esperanza, Silao, Guanajuato",
                CustomerCompany = "Granja Ecológica La Esperanza",
                Quantity = 3,
                UnitPrice = 8075.00m, // Con descuento del 5% por 3+ unidades
                TotalCost = 24225.00m,
                Status = QuoteStatus.Pending,
                Notes = "Solicito cotización para tres sistemas básicos para diferentes secciones de mi granja orgánica",
                RequestDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsPublicQuote = true
            };

            context.Quotes.Add(samplePublicQuote);

            // Crear una cotización de usuario registrado
            var sampleUserQuote = new Quote
            {
                UserId = testCustomer.Id,
                ProductId = products[1].Id,
                CustomerName = "Juan Pérez",
                CustomerEmail = "juan@example.com",
                CustomerPhone = "4771234567",
                CustomerAddress = "Av. Principal 123, León, Guanajuato",
                CustomerCompany = "Granja Los Pinos",
                Quantity = 1,
                UnitPrice = 15500.00m,
                TotalCost = 15500.00m,
                Status = QuoteStatus.Approved,
                Notes = "Interesado en el sistema avanzado para automatizar completamente el riego",
                RequestDate = DateTime.UtcNow.AddDays(-5),
                ResponseDate = DateTime.UtcNow.AddDays(-2),
                ExpiryDate = DateTime.UtcNow.AddDays(25),
                IsPublicQuote = false
            };

            context.Quotes.Add(sampleUserQuote);

            // Crear una venta de ejemplo
            var sampleSale = new Sale
            {
                UserId = testCustomer.Id,
                ProductId = products[0].Id,
                QuoteId = null,
                OrderNumber = "ORD-20241201-1234",
                Quantity = 1,
                UnitPrice = 8500.00m,
                TotalAmount = 8500.00m,
                Status = OrderStatus.Delivered,
                Notes = "Venta directa - cliente satisfecho",
                SaleDate = DateTime.UtcNow.AddDays(-10),
                DeliveryDate = DateTime.UtcNow.AddDays(-3)
            };

            context.Sales.Add(sampleSale);

            // Crear compras de ejemplo
            var samplePurchases = new List<Purchase>
            {
                new Purchase
                {
                    SupplierId = suppliers[0].Id,
                    MaterialId = materials[0].Id,
                    PurchaseNumber = "PUR-20241201-1001",
                    Quantity = 50,
                    UnitCost = 450.00m,
                    TotalCost = 22500.00m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-15),
                    DeliveryDate = DateTime.UtcNow.AddDays(-10),
                    Notes = "Compra inicial de sensores pH"
                },
                new Purchase
                {
                    SupplierId = suppliers[1].Id,
                    MaterialId = materials[2].Id,
                    PurchaseNumber = "PUR-20241205-1002",
                    Quantity = 100,
                    UnitCost = 120.00m,
                    TotalCost = 12000.00m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-12),
                    DeliveryDate = DateTime.UtcNow.AddDays(-7),
                    Notes = "Stock de microcontroladores ESP32"
                }
            };

            context.Purchases.AddRange(samplePurchases);

            // Crear review de ejemplo (solo para productos comprados)
            var sampleReview = new Review
            {
                UserId = testCustomer.Id,
                ProductId = products[0].Id, // Producto que compró
                Rating = 5,
                Comment = "Excelente sistema, muy fácil de instalar y usar. La aplicación móvil es muy intuitiva y me ha ayudado a optimizar el riego de mis cultivos.",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            context.Reviews.Add(sampleReview);

            context.SaveChanges();
        }
    }
}
