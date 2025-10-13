using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Keyless]
public partial class VwMenu
{
    [Column("ID_Menu")]
    public int IdMenu { get; set; }

    [Column("Name_Menu")]
    [StringLength(50)]
    public string? NameMenu { get; set; }

    [Column("ID_Role")]
    public int IdRole { get; set; }

    [StringLength(150)]
    public string? RoleName { get; set; }

    [Column("IS_ALLOW")]
    public bool? IsAllow { get; set; }
}
