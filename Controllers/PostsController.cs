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
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using Microsoft.Extensions.Logging;

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

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPost(int offset = 0, int limit = 10)
    {
      var posts = await _context.Posts.OrderByDescending(post => post.UpdatedAt).Skip(offset).Take(limit).Include(p => p.User).ToListAsync();
      foreach (var post in posts)
      {
        post.User = post.User.WithoutPassword();
      }
      return posts;
    }

    // GET: api/Posts/5
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(long id)

    {
      try
      {
        var post = await _context.Posts.FindAsync(id);
        post.User = await _context.Users.FindAsync(post.UserID);
        post.User.PasswordHash = null;
        post.User.PasswordSalt = null;
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
    public async Task<ActionResult<Post>> PutPost(long id, Post post)
    {

      if (id != post.Id)
      {
        return BadRequest();
      }
      _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
      var postBeforeUpdate = await _context.Posts.FindAsync(id);
      var currentUserId = int.Parse(User.Identity.Name);
      if (postBeforeUpdate.UserID != currentUserId || postBeforeUpdate.UserID != post.UserID)
        return Forbid();
      _context.Entry(post).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      var postInfo = await PostHelper.getPostInfo(_context, id);

      return postInfo != null ? postInfo : NotFound();

    }

    // POST: api/Posts
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for
    // more details see https://aka.ms/RazorPagesCRUD.
    [HttpPost]
    public async Task<ActionResult<Post>> PostPost(Post post)
    {
      Console.WriteLine("User.Identity.Name");
      Console.WriteLine(User.Identity.Name);
      var currentUserId = int.Parse(User.Identity.Name);
      if (post.UserID != currentUserId)
        return Forbid();

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

      var currentUserId = int.Parse(User.Identity.Name);
      if (post.UserID != currentUserId)
        return Forbid();

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
