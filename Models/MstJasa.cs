using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstJasa
{
    public string IdJasa { get; set; } = null!;

    public string NamaJasa { get; set; } = null!;

    public int IdKategori { get; set; }
    public virtual MstKategori? IdKategoriNavigation { get; set; }
    [NotMapped]
    public int No { get; set; }
}
