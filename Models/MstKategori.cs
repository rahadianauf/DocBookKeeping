using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstKategori
{
    public int Id { get; set; }

    public string Kategori { get; set; } = null!;

    [NotMapped]
    public int No { get; set; }
}
