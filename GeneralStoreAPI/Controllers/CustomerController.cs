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
        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // GET BY ID
        // api/Customer/{id}
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
