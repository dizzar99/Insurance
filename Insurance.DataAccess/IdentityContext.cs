﻿using Insurance.DataAccess.IdentityModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Insurance.DataAccess
{
    public class IdentityContext : IdentityDbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<DbRefreshToken> RefreshTokens { get; set; }
    }
}
