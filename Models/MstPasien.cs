using System;
using System.Collections.Generic;

namespace DocBookKeeping.Models;

public partial class MstPasien
{
    public int IdPasien { get; set; }

    public string NamaPasien { get; set; } = null!;

    public string? NoTelepon { get; set; }

    public string? Alamat { get; set; }

    public string? TanggalDaftar { get; set; }
}
