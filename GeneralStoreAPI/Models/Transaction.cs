using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace GeneralStoreAPI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual Customer Customer { get; set; }

        [Required]
        public string CombinedProductSKUString { get; set; }

        [ApiExplorerSettings(IgnoreApi = true)]
        public List<int> ProductSKUs { get; set; } = new List<int>();

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual ICollection<Product> Products { get; set; }

        [Required]
        public string CombinedItemCountString { get; set; }

        [ApiExplorerSettings(IgnoreApi = true)]
        public List<int> ItemCounts { get; set; } = new List<int>();

        public DateTime DateOfTransaction { get; set; }

        public Transaction()
        {
            Products = new HashSet<Product>();
        }
    }
}