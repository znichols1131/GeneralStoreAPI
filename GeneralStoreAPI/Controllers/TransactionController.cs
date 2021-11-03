using GeneralStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeneralStoreAPI.Controllers
{
    public class TransactionController : ApiController
    {
        private readonly GeneralStoreDbContext _context = new GeneralStoreDbContext();

        // POST
        // api/Transaction
        [HttpPost]
        public async Task<IHttpActionResult> PostTransaction([FromBody] Transaction model)
        {
            if(model is null)
                return BadRequest("Your request body cannot be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get related objects
            var customerEntity = await _context.Customers.FindAsync(model.CustomerId);
            if(customerEntity is null)
            {
                return BadRequest($"The target Customer with the ID of {model.CustomerId} does not exist.");
            }else
            {
                model.Customer = customerEntity;
            }

            var productEntity = await _context.Products.FindAsync(model.ProductSKU);
            if (productEntity is null)
            {
                return BadRequest($"The target Product with the SKU of {model.ProductSKU} does not exist.");
            }
            else
            {
                model.Product = productEntity;
            }


            if (!model.Product.IsInStock || model.Product.NumberInInventory < model.ItemCount)
            {
                // Not enough product to cover transaction
                return BadRequest("There is not enough product in stock to complete this transaction.");
            }

            // Set date of transaction
            model.DateOfTransaction = DateTime.Now;

            // Add transaction and reduce product in stock
            _context.Transactions.Add(model);
            model.Product.NumberInInventory -= model.ItemCount;
            if(await _context.SaveChangesAsync() == 1)
                return Ok("Product was created.");

            return InternalServerError();
        }

        // GET ALL
        // api/Transaction
        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Transaction> transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }


    }
}
