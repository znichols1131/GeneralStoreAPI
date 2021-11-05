using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GeneralStoreAPI.Models
{
    public class Product
    {
        [Key]
        public int SKU { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Cost { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "Error: the field 'NumberInInventory' must be positive.")]
        public int NumberInInventory { get; set; }

        // I don't really want to keep track of this
        //public virtual ICollection<Transaction> Transactions { get; set; }

        //public Product()
        //{
        //    Transactions = new HashSet<Transaction>();
        //}
        
        public bool IsInStock 
        {
            get
            {
                return NumberInInventory > 0;
            }
        }
    }
}