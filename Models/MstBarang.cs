using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstBarang
{
    public string IdBarang { get; set; } = null!;

    public string NamaBarang { get; set; } = null!;

    public int IdKategori { get; set; }

    public int IdSatuan { get; set; }

    public virtual MstKategori? IdKategoriNavigation { get; set; }

    public virtual MstSatuan? IdSatuanNavigation { get; set; }

    [NotMapped]
    public int No { get; set; }
}
