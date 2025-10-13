using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Keyless]
[Table("TBL_R_MASTER_KARYAWAN_ALL")]
public partial class TblRMasterKaryawanAll
{
    [Column("EMPLOYEE_ID")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EmployeeId { get; set; }

    [Column("NAME")]
    [StringLength(550)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column("POSITION_ID")]
    [StringLength(50)]
    [Unicode(false)]
    public string? PositionId { get; set; }

    [Column("POS_TITLE")]
    [StringLength(550)]
    [Unicode(false)]
    public string? PosTitle { get; set; }

    [Column("DSTRCT_CODE")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DstrctCode { get; set; }

    [Column("DEPT_CODE")]
    [StringLength(500)]
    [Unicode(false)]
    public string? DeptCode { get; set; }

    [Column("DEPT_DESC")]
    [StringLength(500)]
    [Unicode(false)]
    public string? DeptDesc { get; set; }

    [Column("EMAIL")]
    [StringLength(500)]
    [Unicode(false)]
    public string? Email { get; set; }
}
