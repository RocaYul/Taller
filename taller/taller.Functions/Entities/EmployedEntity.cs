using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace taller.Functions.Entities
{
    class EmployedEntity : TableEntity
    {
        public int IdEmployed { get; set; }
        public DateTime InputOutput { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
