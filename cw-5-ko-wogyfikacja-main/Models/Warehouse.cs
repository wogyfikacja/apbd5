using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet5.Models
{
    public class Warehouse
    {
        public int IdOrder{ get; set; }
        public int IdWarehouse { get; set; }
        public int IdProduct { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}