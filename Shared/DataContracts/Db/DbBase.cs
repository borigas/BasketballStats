﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class DbBase
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}
