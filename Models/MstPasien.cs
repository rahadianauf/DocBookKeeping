using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstPasien
{
    public string IdPasien { get; set; } = null!;

    public string NamaPasien { get; set; } = null!;

    public string? NoTelepon { get; set; }

    public string? Alamat { get; set; }

    public string? TanggalDaftar { get; set; }
    
    [NotMapped]
    public int No { get; set; }
}
