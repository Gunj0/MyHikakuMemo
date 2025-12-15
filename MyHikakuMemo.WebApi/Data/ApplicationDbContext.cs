using Microsoft.EntityFrameworkCore;
using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Data;

public class ApplicationDbContext(DbContextOptions options)
    : DbContext(options)
{
    DbSet<Memo> Memos => Set<Memo>();
}
