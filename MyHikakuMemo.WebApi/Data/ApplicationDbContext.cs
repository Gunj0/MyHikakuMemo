using Microsoft.EntityFrameworkCore;
using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Memo> Memos => Set<Memo>();
}
