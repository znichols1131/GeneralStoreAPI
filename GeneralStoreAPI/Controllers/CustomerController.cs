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
        private readonly GeneralStoreDbContext _context = new GeneralStoreDbContext();

        // POST
        // api/Customer
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
        public async Task<IHttpActionResult> GetAll()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }


    }
}
