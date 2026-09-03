using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DocBookKeeping.Models;

public partial class DocBookKeepingContext : DbContext
{
    public DocBookKeepingContext()
    {
    }

    public DocBookKeepingContext(DbContextOptions<DocBookKeepingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstBarang> MstBarangs { get; set; }

    public virtual DbSet<MstKategori> MstKategoris { get; set; }

    public virtual DbSet<MstJasa> MstJasas { get; set; }

    public virtual DbSet<MstPasien> MstPasiens { get; set; }

    public virtual DbSet<MstPemasok> MstPemasoks { get; set; }

    public virtual DbSet<MstSatuan> MstSatuans { get; set; }

    public virtual DbSet<MstUser> MstUsers { get; set; }

    public virtual DbSet<TransBarang> TransBarangs { get; set; }

    public virtual DbSet<TransJasa> TransJasas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Fallback kalau context dibuat tanpa DI (misal dari EF Core CLI tools).
        // Saat berjalan normal lewat aplikasi, options sudah di-inject dari DI container.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite(DocBookKeeping.AppPaths.ConnectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MstBarang>(entity =>
        {
            entity.HasKey(e => e.IdBarang);

            entity.ToTable("mst_barang");

            entity.HasIndex(e => e.NamaBarang, "IX_mst_barang_nama_barang").IsUnique();

            entity.Property(e => e.IdBarang).HasColumnName("id_barang");
            entity.Property(e => e.HargaJual)
                .HasDefaultValue(0.0)
                .HasColumnName("harga_jual");
            entity.Property(e => e.IdKategori).HasColumnName("id_kategori");
            entity.Property(e => e.IdSatuan).HasColumnName("id_satuan");
            entity.Property(e => e.NamaBarang).HasColumnName("nama_barang");
            entity.Property(e => e.StokMinimum)
                .HasDefaultValue(0)
                .HasColumnName("stok_minimum");
        });

        modelBuilder.Entity<MstKategori>(entity =>
        {
            entity.ToTable("mst_kategori");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Kategori).HasColumnName("kategori");
        });

        modelBuilder.Entity<MstJasa>(entity =>
        {
            entity.HasKey(e => e.IdJasa);

            entity.ToTable("mst_Jasa");

            entity.HasIndex(e => e.NamaJasa, "IX_mst_jasa_nama_jasa").IsUnique();

            entity.Property(e => e.IdJasa)
            .HasColumnName("id_jasa")
            .ValueGeneratedNever();
            entity.Property(e => e.NamaJasa).HasColumnName("nama_jasa");
            entity.Property(e => e.IdKategori).HasColumnName("id_kategori");
            entity.HasOne(d => d.IdKategoriNavigation)
                        .WithMany()
                        .HasForeignKey(d => d.IdKategori)
                        .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MstPasien>(entity =>
        {
            entity.HasKey(e => e.IdPasien);

            entity.ToTable("mst_pasien");

            entity.Property(e => e.IdPasien).HasColumnName("id_pasien").ValueGeneratedNever();
            entity.Property(e => e.Alamat).HasColumnName("alamat");
            entity.Property(e => e.NamaPasien).HasColumnName("nama_pasien");
            entity.Property(e => e.NoTelepon).HasColumnName("no_telepon");
            entity.Property(e => e.TanggalDaftar)
                .HasDefaultValueSql("DATE('now')")
                .HasColumnName("tanggal_daftar");
        });

        modelBuilder.Entity<MstPemasok>(entity =>
        {
            entity.HasKey(e => e.IdPemasok);

            entity.ToTable("mst_pemasok");

            entity.Property(e => e.IdPemasok).HasColumnName("id_pemasok");
            entity.Property(e => e.Alamat).HasColumnName("alamat");
            entity.Property(e => e.Kontak).HasColumnName("kontak");
            entity.Property(e => e.NamaPemasok).HasColumnName("nama_pemasok");
        });

        modelBuilder.Entity<MstSatuan>(entity =>
        {
            entity.ToTable("mst_satuan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Keterangan).HasColumnName("keterangan");
            entity.Property(e => e.Satuan).HasColumnName("satuan");
        });

        modelBuilder.Entity<MstUser>(entity =>
        {
            entity.ToTable("mst_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<TransBarang>(entity =>
        {
            entity.HasKey(e => e.IdTrans);

            entity.ToTable("trans_barang");

            entity.Property(e => e.IdTrans).HasColumnName("id_trans");
            entity.Property(e => e.HargaBeli).HasColumnName("harga_beli");
            entity.Property(e => e.IdBarang).HasColumnName("id_barang");
            entity.Property(e => e.IdPemasok).HasColumnName("id_pemasok");
            entity.Property(e => e.Jumlah).HasColumnName("jumlah");
            entity.Property(e => e.Keterangan).HasColumnName("keterangan");
            entity.Property(e => e.NilaiBeli).HasColumnName("nilai_beli");
            entity.Property(e => e.TanggalBeli).HasColumnName("tanggal_beli");
            entity.Property(e => e.TanggalInput)
                .HasDefaultValueSql("DATE('now')")
                .HasColumnName("tanggal_input");
            entity.Property(e => e.TanggalKadaluwarsa).HasColumnName("tanggal_kadaluwarsa");

            entity.HasOne(d => d.IdBarangNavigation).WithMany(p => p.TransBarangs)
                .HasForeignKey(d => d.IdBarang)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdPemasokNavigation).WithMany(p => p.TransBarangs).HasForeignKey(d => d.IdPemasok);
        });

        modelBuilder.Entity<TransJasa>(entity =>
        {
            entity.HasKey(e => e.IdTrans);

            entity.ToTable("trans_jasa");

            entity.Property(e => e.IdTrans).HasColumnName("id_trans");
            entity.Property(e => e.Harga).HasColumnName("harga");
            entity.Property(e => e.IdJasa).HasColumnName("id_jasa");
            entity.Property(e => e.Keterangan).HasColumnName("keterangan");
            entity.Property(e => e.TanggalInput)
                .HasDefaultValueSql("DATE('now')")
                .HasColumnName("tanggal_input");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
