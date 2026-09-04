using System;
using System.Collections.Generic;

namespace DocBookKeeping.Models;

public partial class MstSatuan
{
    public int Id { get; set; }
    public string Kode { get; set; } = null!;
    public string Satuan { get; set; } = null!;

    public string? Keterangan { get; set; }
}
