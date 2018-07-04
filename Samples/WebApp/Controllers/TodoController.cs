using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Application;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : WebController<TodoItem, long>
    {
        public TodoController(IAppController<TodoItem, long> appController, ApiHelper<TodoItem, long> helper) 
            : base(appController, helper)
        {
        //    if (_context.TodoItems.Count() == 0)
        //    {
        //        _context.TodoItems.Add(new TodoItem { Name = "Item1" });
        //        _context.SaveChanges();
        //    }
        }

        [HttpGet]
        public override Task<ActionResult<IEnumerable<TodoItem>>> GetAsync()
        {
            return base.GetAsync();
        }

        [HttpGet("{id}", Name = "GetTodo")]
        public override Task<ActionResult<TodoItem>> GetByIdAsync(long id)
        {
            return base.GetByIdAsync(id);
        }
        
        /// <summary>
        /// Creates a TodoItem.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "id": 1,
        ///        "name": "Item1",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <param name="item"></param>
        /// <returns>A newly created TodoItem</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>            
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public Task<ActionResult<TodoItem>> PostAsync(TodoItem item)
        {
            //return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
            return base.PostAsync();
        }

        [HttpPut("{id}")]
        public Task<ActionResult<TodoItem>> PutByIdAsync(long id, TodoItem item)
        {
            //if (item == null || item.Id != id)
            //{
            //    return BadRequest();
            //}
            return base.PutByIdAsync(id);
        }

        /// <summary>
        /// Deletes a specific TodoItem.
        /// </summary>
        /// <param name="id"></param>        
        [HttpDelete("{id}")]
        public override Task<IActionResult> DeleteByIdAsync(long id)
        {
            return base.DeleteByIdAsync(id);
        }
    }
}