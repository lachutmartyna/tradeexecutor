using Microsoft.EntityFrameworkCore;
using TradeExecutor.api.Models;

namespace TradeExecutor.api.Data;

public class TradeDbContext(DbContextOptions<TradeDbContext> options) : DbContext(options)
{
    public DbSet<Trade> Trades { get; set; }
}