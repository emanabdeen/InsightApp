using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("AspNetUser")]
public partial class Account : IdentityUser<Guid>
{
    [InverseProperty("Account")]
    public virtual Employee? Employee { get; set; }

    [InverseProperty("Account")]
    public virtual Member? Member { get; set; }
}
