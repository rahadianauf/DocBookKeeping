using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocBookKeeping.Models;

public partial class MstUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    [NotMapped]
    public int No { get; set; }
}
