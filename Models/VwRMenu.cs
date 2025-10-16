using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class VwRMenu
{
    public int Id { get; set; }

    public string? RoleName { get; set; }

    public int IdMenu { get; set; }

    public string? NameMenu { get; set; }

    public int? SubMenu { get; set; }

    public string? IconMenu { get; set; }

    public string? LinkMenu { get; set; }

    public int? Order { get; set; }
}
