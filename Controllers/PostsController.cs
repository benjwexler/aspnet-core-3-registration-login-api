using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Entities;
using WebApi.Models.Users;

namespace WebApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class PostsController : ControllerBase
  {
    private readonly DataContext _context;

    public PostsController(DataContext context)
    {
      _context = context;
    }

    // GET: api/Posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetPost()
    {
      return await _context.Users.Include(p => p.Posts).ToListAsync();
    }

    // GET: api/Posts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(long id)

    {
      Console.WriteLine("Welcome to the C# Station Tutorial!");
      // Console.ReadLine();

      try
      {
        var post = await _context.Posts.FindAsync(id);
        post.User = await _context.Users.FindAsync(post.UserID);
        post.User.PasswordHash = null;
        post.User.PasswordSalt = null;
        // Console.WriteLine(post.User.Username);
        // Console.ReadLine();
        if (post == null)
        {
          return NotFound();
        }

        return post;
      }

      catch (DbUpdateConcurrencyException)
      {
        return NotFound();
      }




    }

    // PUT: api/Posts/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for
    // more details see https://aka.ms/RazorPagesCRUD.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPost(long id, Post post)
    {
      if (id != post.Id)
      {
        return BadRequest();
      }

      _context.Entry(post).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!PostExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    // POST: api/Posts
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for
    // more details see https://aka.ms/RazorPagesCRUD.
    [HttpPost]
    public async Task<ActionResult<Post>> PostPost(Post post)
    {
      _context.Posts.Add(post);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetPost", new { id = post.Id }, post);
    }

    // DELETE: api/Posts/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<Post>> DeletePost(long id)
    {
      var post = await _context.Posts.FindAsync(id);
      if (post == null)
      {
        return NotFound();
      }

      _context.Posts.Remove(post);
      await _context.SaveChangesAsync();

      return post;
    }

    private bool PostExists(long id)
    {
      return _context.Posts.Any(e => e.Id == id);
    }
  }
}
