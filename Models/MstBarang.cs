using System;
using System.Collections.Generic;

namespace DocBookKeeping.Models;

public partial class MstBarang
{
    public string IdBarang { get; set; } = null!;

    public string NamaBarang { get; set; } = null!;

    public int? IdKategori { get; set; }

    public int IdSatuan { get; set; }

    public double? HargaJual { get; set; }

    public int? StokMinimum { get; set; }

    public virtual ICollection<TransBarang> TransBarangs { get; set; } = new List<TransBarang>();
}
