using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GeneralStoreAPI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Required, ForeignKey(nameof(Product))]
        public int ProductSKU { get; set; }
        public virtual Product Product { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "Error: the field 'ItemCount' must be positive.")]
        public int ItemCount { get; set; }

        public DateTime DateOfTransaction { get; set; }
    }
}