using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class VwUser
{
    public int IdRole { get; set; }

    public string Username { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? DstrctCode { get; set; }

    public string? RoleName { get; set; }
}
