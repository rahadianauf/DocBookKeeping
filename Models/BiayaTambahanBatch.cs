namespace DocBookKeeping.Models;

public partial class BiayaTambahanBatch
{
    public int Id { get; set; }

    public string IdTransMasuk { get; set; } = null!;

    public string Komponen { get; set; } = null!;

    public decimal Nilai { get; set; }

    public string? Keterangan { get; set; }
}