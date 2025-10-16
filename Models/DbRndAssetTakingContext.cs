using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

public partial class DbRndAssetTakingContext : DbContext
{
    public DbRndAssetTakingContext()
    {
    }

    public DbRndAssetTakingContext(DbContextOptions<DbRndAssetTakingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblMAkse> TblMAkses { get; set; } = null!;

    public virtual DbSet<TblMAssetCategory> TblMAssetCategories { get; set; } = null!;

    public virtual DbSet<TblMDstrctLocation> TblMDstrctLocations { get; set; } = null!;

    public virtual DbSet<TblMRole> TblMRoles { get; set; } = null!;

    public virtual DbSet<TblMStateCategory> TblMStateCategories { get; set; } = null!;

    public virtual DbSet<TblMUser> TblMUsers { get; set; } = null!;

    public virtual DbSet<TblRAssetPo> TblRAssetPos { get; set; } = null!;

    public virtual DbSet<TblRAssetSerial> TblRAssetSerials { get; set; } = null!;

    public virtual DbSet<TblRMasterKaryawanAll> TblRMasterKaryawanAlls { get; set; } = null!;

    public virtual DbSet<TblRMenu> TblRMenus { get; set; } = null!;

    public virtual DbSet<TblRPriceRange> TblRPriceRanges { get; set; } = null!;

    public virtual DbSet<TblRProject> TblRProjects { get; set; } = null!;

    public virtual DbSet<TblRSubMenu> TblRSubMenus { get; set; } = null!;

    public virtual DbSet<TblTAsset> TblTAssets { get; set; } = null!;

    public virtual DbSet<TblTAssetIn> TblTAssetIns { get; set; } = null!;

    public virtual DbSet<TblTAssetOut> TblTAssetOuts { get; set; } = null!;

    public virtual DbSet<VwMAkse> VwMAkses { get; set; } = null!;

    public virtual DbSet<VwMenu> VwMenus { get; set; } = null!;

    public virtual DbSet<VwRMenu> VwRMenus { get; set; } = null!;

    public virtual DbSet<VwUser> VwUsers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Note: Connection string should be configured in DI container or appsettings.json
            optionsBuilder.UseSqlServer("Server=.\\MSSQLSERVER01;Database=DB_RND_ASSET_TAKING;Trusted_Connection=true;TrustServerCertificate=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblMAkse>(entity =>
        {
            entity.HasKey(e => new { e.IdMenu, e.IdRole });

            entity.ToTable("TBL_M_AKSES");

            entity.Property(e => e.IdMenu).HasColumnName("ID_Menu");
            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.IsAllow).HasColumnName("IS_ALLOW");
        });

        modelBuilder.Entity<TblMAssetCategory>(entity =>
        {
            entity.ToTable("TBL_M_ASSET_CATEGORY");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.KategoriBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kategori_barang");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.PriceRange).HasColumnName("price_range");
        });

        modelBuilder.Entity<TblMDstrctLocation>(entity =>
        {
            entity.ToTable("TBL_M_DSTRCT_LOCATION");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("district");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
        });

        modelBuilder.Entity<TblMRole>(entity =>
        {
            entity.ToTable("TBL_M_ROLE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleName).HasMaxLength(150);
        });

        modelBuilder.Entity<TblMStateCategory>(entity =>
        {
            entity.ToTable("TBL_M_STATE_CATEGORY");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("state");
        });

        modelBuilder.Entity<TblMUser>(entity =>
        {
            entity.HasKey(e => new { e.IdRole, e.Username });

            entity.ToTable("TBL_M_USER");

            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblRAssetPo>(entity =>
        {
            entity.ToTable("TBL_R_ASSET_PO");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.PoItem)
                .HasMaxLength(50)
                .HasColumnName("po_item");
            entity.Property(e => e.PoNumber)
                .HasMaxLength(50)
                .HasColumnName("po_number");

            entity.HasOne(d => d.Asset).WithMany(p => p.TblRAssetPos)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TblRAssetSerial>(entity =>
        {
            entity.HasKey(e => e.SerialId).HasName("PK__TBL_M_AS__E5445948CE87E810");

            entity.ToTable("TBL_R_ASSET_SERIAL");

            entity.HasIndex(e => e.AssetId, "IX_AssetSerials_AssetId");

            entity.HasIndex(e => e.SerialNumber, "IX_AssetSerials_SerialNumber");

            entity.HasIndex(e => e.SerialNumber, "UQ__TBL_M_AS__BED14FEE302ED873").IsUnique();

            entity.Property(e => e.SerialId).HasColumnName("serial_id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasColumnName("modified_by");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50)
                .HasColumnName("serial_number");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Asset).WithMany(p => p.TblRAssetSerials)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TBL_M_ASS__modif__778AC167");

            entity.HasOne(d => d.State).WithMany(p => p.TblRAssetSerials)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TblRMasterKaryawanAll>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TBL_R_MASTER_KARYAWAN_ALL");

            entity.Property(e => e.DeptCode)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("DEPT_CODE");
            entity.Property(e => e.DeptDesc)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("DEPT_DESC");
            entity.Property(e => e.DstrctCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DSTRCT_CODE");
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EMPLOYEE_ID");
            entity.Property(e => e.Name)
                .HasMaxLength(550)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.PosTitle)
                .HasMaxLength(550)
                .IsUnicode(false)
                .HasColumnName("POS_TITLE");
            entity.Property(e => e.PositionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("POSITION_ID");
        });

        modelBuilder.Entity<TblRMenu>(entity =>
        {
            entity.HasKey(e => e.IdMenu);

            entity.ToTable("TBL_R_MENU");

            entity.Property(e => e.IdMenu)
                .ValueGeneratedNever()
                .HasColumnName("ID_Menu");
            entity.Property(e => e.Akses)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IconMenu)
                .HasMaxLength(150)
                .HasColumnName("Icon_Menu");
            entity.Property(e => e.LinkMenu)
                .HasMaxLength(250)
                .HasColumnName("Link_Menu");
            entity.Property(e => e.NameMenu)
                .HasMaxLength(50)
                .HasColumnName("Name_Menu");
            entity.Property(e => e.SubMenu).HasColumnName("Sub_Menu");
        });

        modelBuilder.Entity<TblRPriceRange>(entity =>
        {
            entity.ToTable("TBL_R_PRICE_RANGE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.RangeEnd).HasColumnName("range_end");
            entity.Property(e => e.RangeStart).HasColumnName("range_start");
        });

        modelBuilder.Entity<TblRProject>(entity =>
        {
            entity.ToTable("TBL_R_PROJECT");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("project");
        });

        modelBuilder.Entity<TblRSubMenu>(entity =>
        {
            entity.HasKey(e => e.IdSubMenu);

            entity.ToTable("TBL_R_SUB_MENU");

            entity.Property(e => e.IdSubMenu)
                .ValueGeneratedNever()
                .HasColumnName("ID_Sub_Menu");
            entity.Property(e => e.Akses)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdMenu).HasColumnName("ID_Menu");
            entity.Property(e => e.LinkSubMenu)
                .HasMaxLength(250)
                .HasColumnName("Link_Sub_Menu");
            entity.Property(e => e.SubMenuDescription)
                .HasMaxLength(150)
                .HasColumnName("Sub_Menu_Description");
        });

        modelBuilder.Entity<TblTAsset>(entity =>
        {
            entity.ToTable("TBL_T_ASSET");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AcceptedAt)
                .HasColumnType("datetime")
                .HasColumnName("accepted_at");
            entity.Property(e => e.AcceptedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accepted_by");
            entity.Property(e => e.AssetId)
                .ValueGeneratedOnAdd()
                .HasColumnName("asset_id");
            entity.Property(e => e.AssetInId).HasColumnName("asset_in_id");
            entity.Property(e => e.AssetOutId).HasColumnName("asset_out_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.DstrctIn)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("dstrct_in");
            entity.Property(e => e.DstrctOut)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("dstrct_out");
            entity.Property(e => e.Foto)
                .IsUnicode(false)
                .HasColumnName("foto");
            entity.Property(e => e.KategoriBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kategori_barang");
            entity.Property(e => e.KodeBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kode_barang");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.NamaBarang)
                .IsUnicode(false)
                .HasColumnName("nama_barang");
            entity.Property(e => e.NomorAsset)
                .IsUnicode(false)
                .HasColumnName("nomor_asset");
            entity.Property(e => e.PoNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("po_number");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.SentAt)
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.SentBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sent_by");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TanggalMasuk)
                .HasColumnType("datetime")
                .HasColumnName("tanggal_masuk");
        });

        modelBuilder.Entity<TblTAssetIn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TBL_M_ASSET_IN");

            entity.ToTable("TBL_T_ASSET_IN");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptedAt)
                .HasColumnType("datetime")
                .HasColumnName("accepted_at");
            entity.Property(e => e.AcceptedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accepted_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.DstrctIn)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("dstrct_in");
            entity.Property(e => e.Foto)
                .IsUnicode(false)
                .HasColumnName("foto");
            entity.Property(e => e.KategoriBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kategori_barang");
            entity.Property(e => e.KodeBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kode_barang");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.NamaBarang)
                .IsUnicode(false)
                .HasColumnName("nama_barang");
            entity.Property(e => e.NomorAsset)
                .IsUnicode(false)
                .HasColumnName("nomor_asset");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.SentAt)
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.SentBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sent_by");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("state");
        });

        modelBuilder.Entity<TblTAssetOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TBL_M_ASSET_OUT");

            entity.ToTable("TBL_T_ASSET_OUT");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptedAt)
                .HasColumnType("datetime")
                .HasColumnName("accepted_at");
            entity.Property(e => e.AcceptedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accepted_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.DstrctOut)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("dstrct_out");
            entity.Property(e => e.Foto)
                .IsUnicode(false)
                .HasColumnName("foto");
            entity.Property(e => e.KategoriBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kategori_barang");
            entity.Property(e => e.KodeBarang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kode_barang");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.NamaBarang)
                .IsUnicode(false)
                .HasColumnName("nama_barang");
            entity.Property(e => e.NomorAsset)
                .IsUnicode(false)
                .HasColumnName("nomor_asset");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.SentAt)
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.SentBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sent_by");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("state");
        });

        modelBuilder.Entity<VwMAkse>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_M_AKSES");

            entity.Property(e => e.IdMenu).HasColumnName("ID_Menu");
            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.NameMenu)
                .HasMaxLength(50)
                .HasColumnName("Name_Menu");
            entity.Property(e => e.RoleName).HasMaxLength(150);
        });

        modelBuilder.Entity<VwMenu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_MENU");

            entity.Property(e => e.IdMenu).HasColumnName("ID_Menu");
            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.IsAllow).HasColumnName("IS_ALLOW");
            entity.Property(e => e.NameMenu)
                .HasMaxLength(50)
                .HasColumnName("Name_Menu");
            entity.Property(e => e.RoleName).HasMaxLength(150);
        });

        modelBuilder.Entity<VwRMenu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_R_MENU");

            entity.Property(e => e.IconMenu)
                .HasMaxLength(150)
                .HasColumnName("Icon_Menu");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdMenu).HasColumnName("ID_Menu");
            entity.Property(e => e.LinkMenu)
                .HasMaxLength(250)
                .HasColumnName("Link_Menu");
            entity.Property(e => e.NameMenu)
                .HasMaxLength(50)
                .HasColumnName("Name_Menu");
            entity.Property(e => e.RoleName).HasMaxLength(150);
            entity.Property(e => e.SubMenu).HasColumnName("Sub_Menu");
        });

        modelBuilder.Entity<VwUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_User");

            entity.Property(e => e.DstrctCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DSTRCT_CODE");
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.Name)
                .HasMaxLength(550)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.RoleName).HasMaxLength(150);
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
