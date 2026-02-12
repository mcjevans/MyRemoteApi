using OpenIddict.Abstractions;
using Microsoft.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRemoteApi.Data;
using MyRemoteApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context) => _context = context;

    // GET: api/tasks (Read All)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoTask>>> GetTasks()
        => await _context.Tasks.ToListAsync();

    // POST: api/tasks (Create)
    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="task">The task object to create.</param>
    /// <response code="201">Returns the newly created task</response>
    /// <response code="400">If the task is null or invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoTask>> CreateTask(TodoTask task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
    }

    // DELETE: api/tasks/5 (Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Retrieves all tasks from the database.
    /// </summary>
    /// <returns>A list of tasks.</returns>
    // [HttpGet]
    // public async Task<ActionResult<IEnumerable<TodoTask>>> GetTasks()
    // {
    //     return await _context.Tasks.ToListAsync();
    // }



/*
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request.IsAuthorizationCodeGrantType())
        {
            // Authenticate the user based on the code
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        return BadRequest();
    }
*/    
    [HttpPost("~/connect/token")]
public async Task<IActionResult> Exchange()
{
    var request = HttpContext.GetOpenIddictServerRequest() ??
        throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

    // This method is provided by OpenIddict.Abstractions
    if (request.IsAuthorizationCodeGrantType())
    {
        // Handle the code exchange...
    }
    
    // You might also need these:
    if (request.IsPasswordGrantType()) { /* ... */ }
    if (request.IsRefreshTokenGrantType()) { /* ... */ }

    return BadRequest(new OpenIddictResponse
    {
        Error = OpenIddictConstants.Errors.UnsupportedGrantType,
        ErrorDescription = "The specified grant type is not supported."
    });
}
}