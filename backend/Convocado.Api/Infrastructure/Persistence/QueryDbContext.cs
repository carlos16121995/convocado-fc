using Microsoft.EntityFrameworkCore;

namespace Convocado.Api.Infrastructure.Persistence;

public sealed class QueryDbContext(DbContextOptions<QueryDbContext> options) : ConvocadoDbContext(options);
