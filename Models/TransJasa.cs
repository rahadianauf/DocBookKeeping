using System;
using System.Collections.Generic;

namespace DocBookKeeping.Models;

public partial class TransJasa
{
    public string IdTrans { get; set; } = null!;

    public string TanggalInput { get; set; } = null!;

    public string IdJasa { get; set; } = null!;

    public double Harga { get; set; }

    public string? Keterangan { get; set; }
}
