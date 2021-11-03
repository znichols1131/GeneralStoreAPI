using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeneralStoreAPI.Models
{
    public class Product
    {
        public int SKU { get; set; }
        public string Name { get; set; }
        public double Cost { get; set; }
        public int NumberInInventory { get; set; }
        public bool IsInStock 
        {
            get
            {
                return NumberInInventory > 0;
            }
        }
    }
}