using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeneralStoreAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public int ProductSKU { get; set; }
        public virtual Product Product { get; set; }
        public int ItemCount { get; set; }
        public DateTime DateOfTransaction { get; set; }
    }
}