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
    public class CustomerController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Customer
        /// <summary>
        /// Adds the new Customer model to the database.
        /// </summary>
        /// <param name="model">New Customer model to be added</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> PostCustomer([FromBody] Customer model)
        {
            if(model is null)
                return BadRequest("Your request body cannot be empty");
            
            if(ModelState.IsValid)
            {
                // Store the model in the database
                _context.Customers.Add(model);
                await _context.SaveChangesAsync();
                return Ok("Customer was created.");
            }

            return BadRequest(ModelState);
        }

        // GET ALL
        // api/Customer
        /// <summary>
        /// Gets all Customer models in the database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // GET BY ID
        // api/Customer/{id}
        /// <summary>
        /// Gets the Customer model with the corresponding ID from the database.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetById([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound();

            return Ok(customer);
        }

        // PUT
        // api/Customer/{id}
        /// <summary>
        /// Updates the Customer model with the corresponding ID in the database.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="updatedCustomer">Updated Customer model</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IHttpActionResult> UpdateById([FromUri] int id, [FromBody] Customer updatedCustomer)
        {
            if (updatedCustomer is null)
                return BadRequest("Your request body cannot be empty"); 
            
            if (id != updatedCustomer.Id)
                return BadRequest("Ids do not match");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Customer originalCustomer = await _context.Customers.FindAsync(id);

            if (originalCustomer is null)
                return NotFound();

            originalCustomer.FirstName = updatedCustomer.FirstName;
            originalCustomer.LastName = updatedCustomer.LastName;

            await _context.SaveChangesAsync();

            return Ok($"Customer {originalCustomer.FullName} was updated.");
        }

        // DELETE
        // api/Customer/{id}
        /// <summary>
        /// Deletes the Customer model with the corresponding ID from the database.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCustomer([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound();

            _context.Customers.Remove(customer);

            if (await _context.SaveChangesAsync() == 1)
                return Ok("Customer was deleted.");

            return InternalServerError();
        }
    }
}
