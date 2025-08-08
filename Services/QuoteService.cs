using Microsoft.EntityFrameworkCore;
using AGROPURE.Models.Enums;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;
using AGROPURE.Helpers;

namespace AGROPURE.Services
{
    public class QuoteService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;
        private readonly CostingService _costingService;
        private readonly EmailService _emailService;
        private readonly ILogger<QuoteService> _logger;

        public QuoteService(
            AgroContext context,
            IMapper mapper,
            CostingService costingService,
            EmailService emailService,
            ILogger<QuoteService> logger)
        {
            _context = context;
            _mapper = mapper;
            _costingService = costingService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<QuoteDto> CreatePublicQuoteAsync(CreatePublicQuoteDto createDto)
        {
            try
            {
                _logger.LogInformation("Creando cotización pública para producto {ProductId}, cantidad {Quantity}",
                    createDto.ProductId, createDto.Quantity);

                // Verificar que el producto existe
                var product = await _context.Products.FindAsync(createDto.ProductId);
                if (product == null || !product.IsActive)
                {
                    _logger.LogWarning("Producto {ProductId} no encontrado o inactivo", createDto.ProductId);
                    throw new KeyNotFoundException("Producto no encontrado");
                }

                // Calcular precios usando el servicio de costeo
                _logger.LogInformation("Calculando precios para producto {ProductId}", createDto.ProductId);
                var costCalculation = await _costingService.CalculateProductCostAsync(createDto.ProductId, createDto.Quantity);

                var quote = new Quote
                {
                    UserId = null, // Cotización pública sin usuario
                    ProductId = createDto.ProductId,
                    CustomerName = createDto.CustomerName,
                    CustomerEmail = createDto.CustomerEmail,
                    CustomerPhone = createDto.CustomerPhone,
                    CustomerAddress = createDto.CustomerAddress,
                    CustomerCompany = createDto.CustomerCompany,
                    Quantity = createDto.Quantity,
                    UnitPrice = costCalculation.UnitPrice,
                    TotalCost = costCalculation.TotalCost,
                    Notes = createDto.Notes,
                    RequestDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(30),
                    Status = QuoteStatus.Pending,
                    IsPublicQuote = true
                };

                _logger.LogInformation("Guardando cotización en base de datos");
                _context.Quotes.Add(quote);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cotización #{QuoteId} creada exitosamente", quote.Id);

                // Obtener la cotización completa para retornar
                var createdQuote = await GetQuoteByIdAsync(quote.Id);
                if (createdQuote == null)
                {
                    throw new InvalidOperationException("Error al recuperar la cotización creada");
                }

                // Enviar emails de forma asíncrona SIN ESPERAR
                SendEmailsInBackground(createdQuote);

                return createdQuote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cotización pública");
                throw;
            }
        }

        public async Task<QuoteDto> CreateQuoteAsync(CreateQuoteDto createDto, int userId)
        {
            try
            {
                _logger.LogInformation("Creando cotización para usuario {UserId}, producto {ProductId}",
                    userId, createDto.ProductId);

                // Obtener información del usuario
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("Usuario no encontrado");
                }

                // Verificar que el producto existe
                var product = await _context.Products.FindAsync(createDto.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new KeyNotFoundException("Producto no encontrado");
                }

                // Calcular precios usando el servicio de costeo
                var costCalculation = await _costingService.CalculateProductCostAsync(createDto.ProductId, createDto.Quantity);

                // Crear cotización con datos del usuario logueado
                var quote = new Quote
                {
                    UserId = userId,
                    ProductId = createDto.ProductId,
                    CustomerName = !string.IsNullOrEmpty(createDto.CustomerName) ? createDto.CustomerName : $"{user.FirstName} {user.LastName}",
                    CustomerEmail = !string.IsNullOrEmpty(createDto.CustomerEmail) ? createDto.CustomerEmail : user.Email,
                    CustomerPhone = !string.IsNullOrEmpty(createDto.CustomerPhone) ? createDto.CustomerPhone : user.Phone,
                    CustomerAddress = !string.IsNullOrEmpty(createDto.CustomerAddress) ? createDto.CustomerAddress : user.Address,
                    CustomerCompany = user.Company,
                    Quantity = createDto.Quantity,
                    UnitPrice = costCalculation.UnitPrice,
                    TotalCost = costCalculation.TotalCost,
                    Notes = createDto.Notes,
                    RequestDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(30),
                    Status = QuoteStatus.Pending,
                    IsPublicQuote = false
                };

                _context.Quotes.Add(quote);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cotización #{QuoteId} creada exitosamente para usuario {UserId}", quote.Id, userId);

                var createdQuote = await GetQuoteByIdAsync(quote.Id);
                if (createdQuote == null)
                {
                    throw new InvalidOperationException("Error al recuperar la cotización creada");
                }

                // Enviar emails en background SIN ESPERAR
                SendEmailsInBackground(createdQuote);

                return createdQuote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cotización para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<List<QuoteDto>> GetAllQuotesAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las cotizaciones");

                var quotes = await _context.Quotes
                    .Include(q => q.User)
                    .Include(q => q.Product)
                    .OrderByDescending(q => q.RequestDate)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} cotizaciones", quotes.Count);

                return quotes.Select(q => new QuoteDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    ProductId = q.ProductId,
                    CustomerName = q.CustomerName,
                    CustomerEmail = q.CustomerEmail,
                    CustomerPhone = q.CustomerPhone,
                    CustomerAddress = q.CustomerAddress,
                    CustomerCompany = q.CustomerCompany,
                    Quantity = q.Quantity,
                    UnitPrice = q.UnitPrice,
                    TotalCost = q.TotalCost,
                    Status = q.Status,
                    Notes = q.Notes,
                    AdminNotes = q.AdminNotes,
                    RequestDate = q.RequestDate,
                    ResponseDate = q.ResponseDate,
                    ExpiryDate = q.ExpiryDate,
                    IsPublicQuote = q.IsPublicQuote,
                    ProductName = q.Product.Name,
                    UserFullName = q.User != null ? $"{q.User.FirstName} {q.User.LastName}" : null
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las cotizaciones");
                throw;
            }
        }

        public async Task<List<QuoteDto>> GetQuotesByUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Obteniendo cotizaciones para usuario {UserId}", userId);

                var quotes = await _context.Quotes
                    .Include(q => q.User)
                    .Include(q => q.Product)
                    .Where(q => q.UserId == userId)
                    .OrderByDescending(q => q.RequestDate)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} cotizaciones para usuario {UserId}", quotes.Count, userId);

                var quoteDtos = quotes.Select(q => new QuoteDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    ProductId = q.ProductId,
                    CustomerName = q.CustomerName,
                    CustomerEmail = q.CustomerEmail,
                    CustomerPhone = q.CustomerPhone,
                    CustomerAddress = q.CustomerAddress,
                    CustomerCompany = q.CustomerCompany,
                    Quantity = q.Quantity,
                    UnitPrice = q.UnitPrice,
                    TotalCost = q.TotalCost,
                    Status = q.Status,
                    Notes = q.Notes,
                    AdminNotes = q.AdminNotes,
                    RequestDate = q.RequestDate,
                    ResponseDate = q.ResponseDate,
                    ExpiryDate = q.ExpiryDate,
                    IsPublicQuote = q.IsPublicQuote,
                    ProductName = q.Product?.Name ?? "Producto no disponible",
                    UserFullName = q.User != null ? $"{q.User.FirstName} {q.User.LastName}" : null
                }).ToList();

                return quoteDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cotizaciones para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<QuoteDto?> GetQuoteByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo cotización {QuoteId}", id);

                var quote = await _context.Quotes
                    .Include(q => q.User)
                    .Include(q => q.Product)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quote == null)
                {
                    _logger.LogWarning("Cotización {QuoteId} no encontrada", id);
                    return null;
                }

                return _mapper.Map<QuoteDto>(quote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cotización {QuoteId}", id);
                throw;
            }
        }

        public async Task<QuoteDto> UpdateQuoteStatusAsync(int id, UpdateQuoteStatusDto updateDto, int adminUserId)
        {
            try
            {
                _logger.LogInformation("Actualizando estado de cotización {QuoteId} a {Status} por admin {AdminUserId}",
                    id, updateDto.Status, adminUserId);

                var quote = await _context.Quotes
                    .Include(q => q.User)
                    .Include(q => q.Product)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quote == null)
                {
                    throw new KeyNotFoundException("Cotización no encontrada");
                }

                // Validar transición de estado
                if (!IsValidStatusTransition(quote.Status, updateDto.Status))
                {
                    throw new InvalidOperationException($"No se puede cambiar el estado de {quote.Status} a {updateDto.Status}");
                }

                // Verificar si la cotización ha expirado
                if (quote.ExpiryDate.HasValue && quote.ExpiryDate.Value < DateTime.UtcNow && updateDto.Status == QuoteStatus.Approved)
                {
                    throw new InvalidOperationException("No se puede aprobar una cotización expirada");
                }

                var previousStatus = quote.Status;
                quote.Status = updateDto.Status;
                quote.AdminNotes = updateDto.AdminNotes;
                quote.ResponseDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de cotización {QuoteId} actualizado de {PreviousStatus} a {NewStatus}",
                    id, previousStatus, updateDto.Status);

                // Enviar notificación por email en background
                Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendQuoteStatusUpdateAsync(quote.CustomerEmail, quote.Id, updateDto.Status);
                        _logger.LogInformation("Email de actualización enviado para cotización #{QuoteId}", quote.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando email de actualización para cotización #{QuoteId}", quote.Id);
                    }
                });

                return _mapper.Map<QuoteDto>(quote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado de cotización {QuoteId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteQuoteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando cotización {QuoteId}", id);

                var quote = await _context.Quotes.FindAsync(id);
                if (quote == null)
                {
                    _logger.LogWarning("Cotización {QuoteId} no encontrada para eliminar", id);
                    return false;
                }

                // Solo permitir eliminar cotizaciones pendientes o rechazadas
                if (quote.Status == QuoteStatus.Approved || quote.Status == QuoteStatus.Completed)
                {
                    throw new InvalidOperationException("No se pueden eliminar cotizaciones aprobadas o completadas");
                }

                _context.Quotes.Remove(quote);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cotización {QuoteId} eliminada exitosamente", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cotización {QuoteId}", id);
                throw;
            }
        }

        public async Task ApproveQuoteAndCreateUserAsync(int quoteId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Aprobando cotización {QuoteId} y creando usuario", quoteId);

                var quote = await _context.Quotes
                    .Include(q => q.Product)
                    .FirstOrDefaultAsync(q => q.Id == quoteId);

                if (quote == null)
                {
                    throw new KeyNotFoundException("Cotización no encontrada");
                }

                // Validar que la cotización puede ser aprobada
                if (quote.Status != QuoteStatus.Pending)
                {
                    throw new InvalidOperationException("Solo se pueden aprobar cotizaciones pendientes");
                }

                if (!quote.IsPublicQuote)
                {
                    throw new InvalidOperationException("Solo se puede crear usuario para cotizaciones públicas");
                }

                // Verificar si ya existe un usuario con ese email
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == quote.CustomerEmail);

                if (existingUser == null)
                {
                    // Crear nuevo usuario
                    var tempPassword = GenerateRandomPassword();
                    var nameParts = quote.CustomerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var newUser = new User
                    {
                        FirstName = nameParts.FirstOrDefault() ?? quote.CustomerName,
                        LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                        Email = quote.CustomerEmail,
                        PasswordHash = PasswordHelper.HashPassword(tempPassword),
                        Phone = quote.CustomerPhone,
                        Company = quote.CustomerCompany,
                        Address = quote.CustomerAddress,
                        Role = UserRole.Customer,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Actualizar la cotización con el nuevo usuario
                    quote.UserId = newUser.Id;
                    quote.Status = QuoteStatus.Approved;
                    quote.ResponseDate = DateTime.UtcNow;
                    quote.AdminNotes = "Cotización aprobada y usuario creado automáticamente";

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Usuario creado y cotización {QuoteId} aprobada para {Email}", quoteId, quote.CustomerEmail);

                    // Enviar email de bienvenida en background
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(newUser.Email, quote.CustomerName, tempPassword);
                            _logger.LogInformation("Email de bienvenida enviado para nueva cuenta {Email}", newUser.Email);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error enviando email de bienvenida para cotización #{QuoteId}", quoteId);
                        }
                    });
                }
                else
                {
                    // Usuario ya existe, solo aprobar cotización
                    quote.UserId = existingUser.Id;
                    quote.Status = QuoteStatus.Approved;
                    quote.ResponseDate = DateTime.UtcNow;
                    quote.AdminNotes = "Cotización aprobada - usuario ya existía";
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Cotización {QuoteId} aprobada para usuario existente {Email}", quoteId, quote.CustomerEmail);

                    // Enviar notificación de aprobación
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendQuoteStatusUpdateAsync(quote.CustomerEmail, quote.Id, QuoteStatus.Approved);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error enviando notificación de aprobación para cotización #{QuoteId}", quoteId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error aprobando cotización {QuoteId} y creando usuario", quoteId);
                throw;
            }
        }

        // MÉTODO PRIVADO para enviar emails en background sin bloquear
        private void SendEmailsInBackground(QuoteDto quote)
        {
            Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Enviando emails en background para cotización #{QuoteId}", quote.Id);

                    // Enviar email al admin
                    await _emailService.SendQuoteNotificationToAdminAsync(quote);
                    _logger.LogInformation("Email al admin enviado para cotización #{QuoteId}", quote.Id);

                    // Enviar confirmación al cliente
                    await _emailService.SendQuoteRequestNotificationAsync(quote.CustomerEmail, quote.Id);
                    _logger.LogInformation("Email de confirmación enviado para cotización #{QuoteId}", quote.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando emails en background para cotización #{QuoteId}: {ErrorMessage}",
                        quote.Id, ex.Message);
                }
            });
        }

        private bool IsValidStatusTransition(QuoteStatus currentStatus, QuoteStatus newStatus)
        {
            return currentStatus switch
            {
                QuoteStatus.Pending => newStatus is QuoteStatus.Approved or QuoteStatus.Rejected,
                QuoteStatus.Approved => newStatus is QuoteStatus.Completed or QuoteStatus.Rejected,
                QuoteStatus.Rejected => false, // No se puede cambiar desde rechazado
                QuoteStatus.Completed => false, // No se puede cambiar desde completado
                _ => false
            };
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Método para obtener estadísticas de cotizaciones
        public async Task<QuoteStatsDto> GetQuoteStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var thirtyDaysAgo = now.AddDays(-30);

                var stats = new QuoteStatsDto
                {
                    TotalQuotes = await _context.Quotes.CountAsync(),
                    PendingQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Pending),
                    ApprovedQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Approved),
                    RejectedQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Rejected),
                    CompletedQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Completed),
                    PublicQuotes = await _context.Quotes.CountAsync(q => q.IsPublicQuote),
                    QuotesThisMonth = await _context.Quotes.CountAsync(q => q.RequestDate >= thirtyDaysAgo),
                    TotalRevenue = await _context.Quotes
                        .Where(q => q.Status == QuoteStatus.Approved || q.Status == QuoteStatus.Completed)
                        .SumAsync(q => q.TotalCost),
                    AverageQuoteValue = await _context.Quotes
                        .Where(q => q.Status != QuoteStatus.Rejected)
                        .AverageAsync(q => q.TotalCost),
                    ExpiredQuotes = await _context.Quotes
                        .CountAsync(q => q.ExpiryDate < now && q.Status == QuoteStatus.Pending)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de cotizaciones");
                throw;
            }
        }
    }

    // DTO para estadísticas
    public class QuoteStatsDto
    {
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int ApprovedQuotes { get; set; }
        public int RejectedQuotes { get; set; }
        public int CompletedQuotes { get; set; }
        public int PublicQuotes { get; set; }
        public int QuotesThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageQuoteValue { get; set; }
        public int ExpiredQuotes { get; set; }
    }
}