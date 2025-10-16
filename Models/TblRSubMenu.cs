using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblRSubMenu
{
    public int IdSubMenu { get; set; }

    public int? IdMenu { get; set; }

    public string? SubMenuDescription { get; set; }

    public string? LinkSubMenu { get; set; }

    public string? Akses { get; set; }
}
