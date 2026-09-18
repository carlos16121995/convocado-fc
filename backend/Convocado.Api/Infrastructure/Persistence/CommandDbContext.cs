using Microsoft.EntityFrameworkCore;

namespace Convocado.Api.Infrastructure.Persistence;

public sealed class CommandDbContext(DbContextOptions<CommandDbContext> options) : ConvocadoDbContext(options);
