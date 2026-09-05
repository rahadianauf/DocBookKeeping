using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstPasien
{
    public string IdTrans { get; set; } = null!;

    public string? IdPasien { get; set; }

    public string IdJasa { get; set; } = null!;

    public decimal Harga { get; set; }

    public string? Keterangan { get; set; }

    public string? Tag { get; set; }

    public string TanggalInput { get; set; } = null!;

    public virtual MstPasien? IdPasienNavigation { get; set; }

    public virtual MstJasa? IdJasaNavigation { get; set; }

    [NotMapped]
    public int No { get; set; }
}
