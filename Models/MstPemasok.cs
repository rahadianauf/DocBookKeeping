using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstPemasok
{
    public int IdPemasok { get; set; }

    public string NamaPemasok { get; set; } = null!;

    public string? Kontak { get; set; }

    public string? Alamat { get; set; }

    public virtual ICollection<TransBarang> TransBarangs { get; set; } = new List<TransBarang>();

    [NotMapped]
    public int No { get; set; }
}
