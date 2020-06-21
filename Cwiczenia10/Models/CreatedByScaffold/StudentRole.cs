using System;
using System.Collections.Generic;

namespace Cwiczenia10.Models.CreatedByScaffold
{
    public partial class StudentRole
    {
        public int? IdRole { get; set; }
        public string IndexNumber { get; set; }

        public virtual Roles IdRoleNavigation { get; set; }
        public virtual Student IndexNumberNavigation { get; set; }
    }
}
