using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    DbSet<Memo> Memos => Set<Memo>();
}
