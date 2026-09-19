namespace DocBookKeeping.Models;

public partial class BiayaOperasional
{
    public string IdBiaya { get; set; } = null!;
    public string Kategori { get; set; } = null!;
    public decimal Nominal { get; set; }
    public string Tanggal { get; set; } = null!;
    public string? Keterangan { get; set; }
}