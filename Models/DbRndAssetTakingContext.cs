using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

public partial class DbRndAssetTakingContext : DbContext
{
    public DbRndAssetTakingContext(DbContextOptions<DbRndAssetTakingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblMAkse> TblMAkses { get; set; }

    public virtual DbSet<TblMAssetCategory> TblMAssetCategories { get; set; }

    public virtual DbSet<TblTAssetIn> TblTAssetIns { get; set; }

    public virtual DbSet<TblTAssetOut> TblTAssetOuts { get; set; }

    public virtual DbSet<TblMRole> TblMRoles { get; set; }

    public virtual DbSet<TblMUser> TblMUsers { get; set; }

    public virtual DbSet<TblRMasterKaryawanAll> TblRMasterKaryawanAlls { get; set; }

    public virtual DbSet<TblRMenu> TblRMenus { get; set; }

    public virtual DbSet<TblRSubMenu> TblRSubMenus { get; set; }

    public virtual DbSet<TblTAsset> TblTAssets { get; set; }

    public virtual DbSet<VwMAkse> VwMAkses { get; set; }

    public virtual DbSet<VwMenu> VwMenus { get; set; }

    public virtual DbSet<VwRMenu> VwRMenus { get; set; }

    public virtual DbSet<VwUser> VwUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblRMenu>(entity =>
        {
            entity.Property(e => e.IdMenu).ValueGeneratedNever();
        });

        modelBuilder.Entity<TblRSubMenu>(entity =>
        {
            entity.Property(e => e.IdSubMenu).ValueGeneratedNever();
        });

        modelBuilder.Entity<VwMAkse>(entity =>
        {
            entity.ToView("VW_M_AKSES");
        });

        modelBuilder.Entity<VwMenu>(entity =>
        {
            entity.ToView("VW_MENU");
        });

        modelBuilder.Entity<VwRMenu>(entity =>
        {
            entity.ToView("VW_R_MENU");
        });

        modelBuilder.Entity<VwUser>(entity =>
        {
            entity.ToView("VW_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
