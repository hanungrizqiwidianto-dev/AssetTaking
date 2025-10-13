using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Keyless]
public partial class VwUser
{
    [Column("ID_Role")]
    public int IdRole { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("NAME")]
    [StringLength(550)]
    [Unicode(false)]
    public string? Name { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Email { get; set; }

    [Column("DSTRCT_CODE")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DstrctCode { get; set; }

    [StringLength(150)]
    public string? RoleName { get; set; }
}
