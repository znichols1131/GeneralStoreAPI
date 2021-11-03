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

        // GET ALL by CustomerId
        // api/Transaction?customerId={customerId}
        [HttpGet]
        public async Task<IHttpActionResult> GetAllByCustomerId([FromUri]int customerId)
        {
            List<Transaction> transactions = await _context.Transactions.Where(t => t.CustomerId == customerId).ToListAsync();
            return Ok(transactions);
        }

        // GET by Transaction Id
        // api/Transaction/{id}
        [HttpGet]
        public async Task<IHttpActionResult> GetById([FromUri] int id)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);

            if (transaction is null)
                return NotFound();

            return Ok(transaction);
        }

        // PUT
        // api/Transaction/{id}
        [HttpPut]
        public async Task<IHttpActionResult> UpdateById([FromUri] int id, [FromBody] Transaction updatedTransaction)
        {
            if(updatedTransaction is null)
                return BadRequest("Your request body cannot be empty");

            if (id != updatedTransaction.Id)
                return BadRequest("IDs do not match.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get original transaction
            Transaction originalTransaction = await _context.Transactions.FindAsync(id);
            if (originalTransaction is null)
                return NotFound();

            // Get related objects
            var customerEntity = await _context.Customers.FindAsync(updatedTransaction.CustomerId);
            if (customerEntity is null)
            {
                return BadRequest($"The target Customer with the ID of {updatedTransaction.CustomerId} does not exist.");
            }
            else
            {
                updatedTransaction.Customer = customerEntity;
            }

            var productEntity = await _context.Products.FindAsync(updatedTransaction.ProductSKU);
            if (productEntity is null)
            {
                return BadRequest($"The target Product with the SKU of {updatedTransaction.ProductSKU} does not exist.");
            }
            else
            {
                updatedTransaction.Product = productEntity;
            }

            // Logic for checking product availability
            if(originalTransaction.ProductSKU == updatedTransaction.ProductSKU)
            {
                // Same product, only need to check for additional product
                if(updatedTransaction.ItemCount - originalTransaction.ItemCount > originalTransaction.Product.NumberInInventory)
                    return BadRequest("There is not enough product in stock to complete this transaction.");
            }
            else
            {
                // Different product, need to check that the new product has enough inventory
                if(updatedTransaction.ItemCount > updatedTransaction.Product.NumberInInventory)
                    return BadRequest("There is not enough product in stock to complete this transaction.");
            }

            // Return old items "to shelf"
            originalTransaction.Product.NumberInInventory += originalTransaction.ItemCount;

            // Update transaction
            originalTransaction.CustomerId = updatedTransaction.CustomerId;
            originalTransaction.Customer = updatedTransaction.Customer;
            originalTransaction.ProductSKU = updatedTransaction.ProductSKU;
            originalTransaction.Product = updatedTransaction.Product;
            originalTransaction.ItemCount = updatedTransaction.ItemCount;
            originalTransaction.DateOfTransaction = DateTime.Now;       // Note: this assumes that the transaction date must be updated to reflect changes

            // Fulfill new order
            originalTransaction.Product.NumberInInventory -= originalTransaction.ItemCount;

            // Save
            if (await _context.SaveChangesAsync() == 1)
                return Ok("Transaction #{originalTransaction.Id} was updated.");

            return InternalServerError();
        }

        // DELETE
        // api/Transaction/{id}
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTransaction([FromUri] int id)
        {
            // Get original transaction
            Transaction transaction = await _context.Transactions.FindAsync(id);
            if (transaction is null)
                return NotFound();

            // Return old items "to shelf"
            transaction.Product.NumberInInventory += transaction.ItemCount;

            _context.Transactions.Remove(transaction);

            if (await _context.SaveChangesAsync() == 1)
                return Ok("Transactiton was deleted.");

            return InternalServerError();
        }
    }
}
