using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Keyless]
public partial class VwRMenu
{
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(150)]
    public string? RoleName { get; set; }

    [Column("ID_Menu")]
    public int IdMenu { get; set; }

    [Column("Name_Menu")]
    [StringLength(50)]
    public string? NameMenu { get; set; }

    [Column("Sub_Menu")]
    public int? SubMenu { get; set; }

    [Column("Icon_Menu")]
    [StringLength(150)]
    public string? IconMenu { get; set; }

    [Column("Link_Menu")]
    [StringLength(250)]
    public string? LinkMenu { get; set; }

    public int? Order { get; set; }
}
