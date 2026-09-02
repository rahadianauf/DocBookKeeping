using System;
using System.Collections.Generic;

namespace DocBookKeeping.Models;

public partial class TransBarang
{
    public string IdTrans { get; set; } = null!;

    public string IdBarang { get; set; } = null!;

    public int? IdPemasok { get; set; }

    public string TanggalInput { get; set; } = null!;

    public string TanggalBeli { get; set; } = null!;

    public string TanggalKadaluwarsa { get; set; } = null!;

    public int Jumlah { get; set; }

    public double HargaBeli { get; set; }

    public double NilaiBeli { get; set; }

    public string? Keterangan { get; set; }

    public virtual MstBarang IdBarangNavigation { get; set; } = null!;

    public virtual MstPemasok? IdPemasokNavigation { get; set; }
}
