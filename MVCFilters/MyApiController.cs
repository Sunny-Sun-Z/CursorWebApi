using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MyApi.Controllers
{
    // ROUTING ATTRIBUTE: Sets the base route for all actions in this controller
    [Route("api/[controller]")]
    // AUTHORIZATION FILTER: Requires authentication for all actions in this controller
    [Authorize]
    // RESOURCE FILTER: Runs before/after model binding (rarely used directly)
    [ServiceFilter(typeof(MyResourceFilter))]
    public class ProductsController : ControllerBase
    {
        // ACTION FILTER: Runs before/after this action
        [ServiceFilter(typeof(MyActionFilter))]
        // EXCEPTION FILTER: Handles exceptions thrown by this action
        [TypeFilter(typeof(MyExceptionFilter))]
        // RESULT FILTER: Runs before/after the action result is executed
        [ServiceFilter(typeof(MyResultFilter))]
        // ROUTING ATTRIBUTE: Maps this action to GET /api/products/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // ... action logic ...
            if (id < 0)
                throw new Exception("Invalid ID!"); // Will be caught by exception filter

            return Ok(new { Id = id, Name = "Sample Product" });
        }

        // ROUTING ATTRIBUTE: Maps this action to POST /api/products
        [HttpPost]
        // AUTHORIZATION FILTER: Allows anonymous access to this action (overrides controller-level [Authorize])
        [AllowAnonymous]
        public IActionResult Create([FromBody] Product product)
        {
            // ... action logic ...
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
    }
}
