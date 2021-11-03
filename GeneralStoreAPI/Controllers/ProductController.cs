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
    public class ProductController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Product
        /// <summary>
        /// Adds the new Product model to the database.
        /// </summary>
        /// <param name="model">New Product model to be added</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> PostProduct([FromBody] Product model)
        {
            if (model is null)
                return BadRequest("Your request body cannot be empty");

            if (ModelState.IsValid)
            {
                // Store the model in the database
                _context.Products.Add(model);
                await _context.SaveChangesAsync();
                return Ok("Product was created.");
            }

            return BadRequest(ModelState);
        }

        // GET ALL
        // api/Product
        /// <summary>
        /// Gets all Product models in the database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET BY ID
        // api/Product/{id}
        /// <summary>
        /// Gets the Product model with the corresponding SKU from the database.
        /// </summary>
        /// <param name="id">Product SKU</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetBySKU([FromUri] int id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product is null)
                return NotFound();

            return Ok(product);
        }

        // PUT
        // api/Product/{id}
        /// <summary>
        /// Updates the Product model with the corresponding SKU in the database.
        /// </summary>
        /// <param name="id">Product SKU</param>
        /// <param name="updatedProduct">Updated Product model</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IHttpActionResult> UpdateBySKU([FromUri] int id, [FromBody] Product updatedProduct)
        {
            if (updatedProduct is null)
                return BadRequest("Your request body cannot be empty");

            if (id != updatedProduct.SKU)
                return BadRequest("SKUs do not match");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Product originalProduct = await _context.Products.FindAsync(id);

            if (originalProduct is null)
                return NotFound();

            originalProduct.Name = updatedProduct.Name;
            originalProduct.Cost = updatedProduct.Cost;
            originalProduct.NumberInInventory = updatedProduct.NumberInInventory;

            await _context.SaveChangesAsync();

            return Ok($"Product {originalProduct.Name} was updated.");
        }

        // DELETE
        // api/Product/{id}
        /// <summary>
        /// Deletes the Product model with the corresponding SKU from the database.
        /// </summary>
        /// <param name="id">Product SKU</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteProduct([FromUri] int id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product is null)
                return NotFound();

            _context.Products.Remove(product);

            if (await _context.SaveChangesAsync() == 1)
                return Ok("Product was deleted.");

            return InternalServerError();
        }
    }
}
