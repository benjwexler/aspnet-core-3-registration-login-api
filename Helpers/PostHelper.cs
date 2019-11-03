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

namespace WebApi.Helpers
{
  public static class PostHelper
  {
    public static async Task<ActionResult<Post>> getPostInfo(DataContext context, long id)
    {
      try
      {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
        {
          return post;
        }
        post.User = await context.Users.FindAsync(post.UserID);
        post.User.PasswordHash = null;
        post.User.PasswordSalt = null;
        return post;
      }

      catch (DbUpdateConcurrencyException)
      {
        return null;
      }
    }
  }
}