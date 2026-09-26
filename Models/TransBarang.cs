using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class TransBarang
{
    public string IdTrans { get; set; } = null!;

    public string IdBarang { get; set; } = null!;

    public int? IdPemasok { get; set; }

    public string TanggalInput { get; set; } = null!;

    public string TanggalTransaksi { get; set; } = null!;
    
    public string? Tag { get; set; }

    public string? Sumber {get; set;}
    
    public string? TujuanKeluar {get; set;}
    
    public string? IdTransProduksi {get; set;}

    public int Jumlah { get; set; }

    public decimal HargaSatuan { get; set; }

    public decimal Nilai { get; set; }

    public string? TanggalKadaluwarsa { get; set; }

    public string? Keterangan { get; set; }

    public virtual MstBarang? IdBarangNavigation { get; set; }

    public virtual MstPemasok? IdPemasokNavigation { get; set; }

    [NotMapped]
    public int No { get; set; }
}
