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
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Transaction
        /// <summary>
        /// Adds the new Transaction model to the database.
        /// </summary>
        /// <param name="model">New Transaction model to be added</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> PostTransaction([FromBody] Transaction model)
        {
            if(model is null)
                return BadRequest("Your request body cannot be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check that customer exists
            var customerEntity = await _context.Customers.FindAsync(model.CustomerId);
            if(customerEntity is null)
            {
                return BadRequest($"The target Customer with the ID of {model.CustomerId} does not exist.");
            }
            else
            {
                model.Customer = customerEntity;
            }

            // Check that all products exist and have enough inventory to cover order
            model.ProductSKUs = GetListOfIntegers(model.CombinedProductSKUString);
            model.ItemCounts = GetListOfIntegers(model.CombinedItemCountString);
            for(int i = 0; i < model.ProductSKUs.Count; i++)
            {
                int sku = model.ProductSKUs[i];
                var productEntity = await _context.Products.FindAsync(sku);
                if (productEntity is null)
                {
                    return BadRequest($"The target Product with the SKU of {sku} does not exist.");
                }
                else
                {
                    model.Products.Add(productEntity);
                }
                Product p = model.Products.ToList()[i];
                // Also check that this won't exceed the inventory
                if (!p.IsInStock || p.NumberInInventory < model.ItemCounts[i])
                {
                    // Not enough product to cover transaction
                    return BadRequest($"There is not enough product (SKU: {sku})in stock to complete this transaction.");
                }
            }

            // Set date of transaction
            model.DateOfTransaction = DateTime.Now;

            // Add transaction and reduce product in stock
            _context.Transactions.Add(model);
            for (int i = 0; i < model.ProductSKUs.Count; i++)
            {
                model.Products.ToList()[i].NumberInInventory -= model.ItemCounts[i];
            }

            int changesSaved = await _context.SaveChangesAsync();
            if(changesSaved > 0)
                return Ok("Transaction was created.");

            return InternalServerError();
        }


        // GET ALL
        // api/Transaction
        /// <summary>
        /// Gets all Transaction models in the database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Transaction> transactions = await _context.Transactions.ToListAsync();
            foreach(Transaction t in transactions)
            {
                t.ProductSKUs = GetListOfIntegers(t.CombinedProductSKUString);
                t.ItemCounts = GetListOfIntegers(t.CombinedItemCountString);
            }
            return Ok(transactions);
        }


        // GET ALL by CustomerId
        // api/Transaction?customerId={customerId}
        /// <summary>
        /// Gets the Transaction model with the corresponding Customer ID from the database.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetAllByCustomerId([FromUri]int customerId)
        {
            List<Transaction> transactions = await _context.Transactions.Where(t => t.CustomerId == customerId).ToListAsync();
            foreach (Transaction t in transactions)
            {
                t.ProductSKUs = GetListOfIntegers(t.CombinedProductSKUString);
                t.ItemCounts = GetListOfIntegers(t.CombinedItemCountString);
            }
            return Ok(transactions);
        }


        // GET by Transaction Id
        // api/Transaction/{id}
        /// <summary>
        /// Gets the Transaction model with the corresponding ID from the database.
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetById([FromUri] int id)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);
            transaction.ProductSKUs = GetListOfIntegers(transaction.CombinedProductSKUString);
            transaction.ItemCounts = GetListOfIntegers(transaction.CombinedItemCountString);

            if (transaction is null)
                return NotFound();

            return Ok(transaction);
        }


        // PUT
        // api/Transaction/{id}
        /// <summary>
        /// Updates the Transaction model with the corresponding ID in the database.
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="updatedTransaction">Updated Transaction model</param>
        /// <returns></returns>
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

            // Check that all products exist and have enough inventory to cover order
            originalTransaction.ProductSKUs = GetListOfIntegers(originalTransaction.CombinedProductSKUString);
            originalTransaction.ItemCounts = GetListOfIntegers(originalTransaction.CombinedItemCountString);
            updatedTransaction.ProductSKUs = GetListOfIntegers(updatedTransaction.CombinedProductSKUString);
            updatedTransaction.ItemCounts = GetListOfIntegers(updatedTransaction.CombinedItemCountString);
            for (int i = 0; i < updatedTransaction.ProductSKUs.Count; i++)
            {
                int sku = updatedTransaction.ProductSKUs[i];
                var productEntity = await _context.Products.FindAsync(sku);
                if (productEntity is null)
                {
                    return BadRequest($"The target Product with the SKU of {sku} does not exist.");
                }
                else
                {
                    updatedTransaction.Products.Add(productEntity);
                }
            }

            // Check that this new transaction won't exceed the inventory
            // Note: this can't be accomplished in the above for loop because not all products have been retrieved at that point.
            for (int i = 0; i < updatedTransaction.ProductSKUs.Count; i++)
            {
                int sku = updatedTransaction.ProductSKUs[i];

                // Also check that this new transaction won't exceed the inventory
                int previousAmountOrdered = 0;
                for(int j = 0; j < originalTransaction.ProductSKUs.Count; j++)
                {
                    int originalSKU = originalTransaction.ProductSKUs[j];
                    if (sku == originalSKU)
                        previousAmountOrdered += originalTransaction.ItemCounts[j];
                }

                if(updatedTransaction.ItemCounts[i] - previousAmountOrdered > updatedTransaction.Products.ToList()[i].NumberInInventory)
                    return BadRequest($"There is not enough product (SKU: {sku})in stock to complete this transaction.");
            }

            // Return old items "to shelf"
            for (int j = 0; j < originalTransaction.Products.Count; j++)
            {
                originalTransaction.Products.ToList()[j].NumberInInventory += originalTransaction.ItemCounts[j];
            }

            // Update transaction
            originalTransaction.CustomerId = updatedTransaction.CustomerId;
            originalTransaction.Customer = updatedTransaction.Customer;
            originalTransaction.CombinedProductSKUString = updatedTransaction.CombinedProductSKUString;
            originalTransaction.ProductSKUs = updatedTransaction.ProductSKUs;
            originalTransaction.Products = updatedTransaction.Products;
            originalTransaction.CombinedItemCountString = updatedTransaction.CombinedItemCountString;
            originalTransaction.ItemCounts = updatedTransaction.ItemCounts;
            originalTransaction.DateOfTransaction = DateTime.Now;       // Note: this assumes that the transaction date must be updated to reflect changes

            // Fulfill new order (note: originalTransaction has already taken the new data from updatedTransaction
            for(int i = 0; i < originalTransaction.Products.Count; i++)
            {
                originalTransaction.Products.ToList()[i].NumberInInventory -= originalTransaction.ItemCounts[i];
            }

            // Save
            if (await _context.SaveChangesAsync() > 0)
                return Ok($"Transaction #{originalTransaction.Id} was updated.");

            return InternalServerError();
        }


        // DELETE
        // api/Transaction/{id}
        /// <summary>
        /// Deletes the Transaction model with the corresponding ID from the database.
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTransaction([FromUri] int id)
        {
            // Get original transaction
            Transaction transaction = await _context.Transactions.FindAsync(id);
            if (transaction is null)
                return NotFound();

            transaction.ProductSKUs = GetListOfIntegers(transaction.CombinedProductSKUString);
            transaction.ItemCounts = GetListOfIntegers(transaction.CombinedItemCountString);

            // Return old items "to shelf"
            for (int j = 0; j < transaction.Products.Count; j++)
            {
                transaction.Products.ToList()[j].NumberInInventory += transaction.ItemCounts[j];
            }

            _context.Transactions.Remove(transaction);

            if (await _context.SaveChangesAsync() > 0)
                return Ok("Transactiton was deleted.");

            return InternalServerError();
        }

        private List<int> GetListOfIntegers(string commaDelimString)
        {
            try
            {
                string[] listOfStrings = commaDelimString.Split(',');
                List<int> listOfInts = new List<int>();

                foreach(string s in listOfStrings)
                {
                    listOfInts.Add(int.Parse(s.Trim()));
                }

                return listOfInts;
            }
            catch
            {
                return null;
            }
        }
    }
}
