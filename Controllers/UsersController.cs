﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Entities;
using WebApi.Models.Users;
using WebApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;


namespace WebApi.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class UsersController : ControllerBase
  {
    private IUserService _userService;
    private IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly DataContext _context;

    public UsersController(
        IUserService userService,
        DataContext context,
        IMapper mapper,
        IOptions<AppSettings> appSettings)
    {
      _userService = userService;
      _mapper = mapper;
      _appSettings = appSettings.Value;
      _context = context;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody]AuthenticateModel model)
    {
      var user = _userService.Authenticate(model.Username, model.Password);

      if (user == null)
        return BadRequest(new { message = "Username or password is incorrect" });

      return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody]RegisterModel model)
    {
      // map model to entity
      var user = _mapper.Map<User>(model);

      try
      {
        // create user
        _userService.Create(user, model.Password);
        var _user = _userService.Authenticate(model.Username, model.Password);
        return Ok(new
        {
          Id = _user.Id,
          Username = _user.Username,
          FirstName = _user.FirstName,
          LastName = _user.LastName,
          Status = "Ok",
        });
      }
      catch (AppException ex)
      {
        // return error message if there was an exception
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
      var users = _userService.GetAll();
      var model = _mapper.Map<IList<UserModel>>(users);
      return Ok(model);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public IActionResult GetById(int id, int offset = 0, int limit = 2)
    {
      
      var user = _userService.GetById(id);
      var model = _mapper.Map<UserModel>(user);
      var postz = _context.Posts.Where(p => p.UserID == id);
      postz = postz.OrderByDescending(post => post.UpdatedAt).Skip(offset).Take(limit);
      foreach (var post in postz)
      {
        post.User.PasswordHash = null;
        post.User.PasswordSalt = null;
      }
      // var mappedPosts = _mapper.Map<PostWithoutUserId>(postz);
      model.Posts = postz;
      // model.Posts = _context.Posts.Where(p => p.UserID == id);
      return Ok(model);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody]UpdateModel model)
    {
      var currentUserId = int.Parse(User.Identity.Name);
      if (id != currentUserId)
        return Forbid();
      // map model to entity and set id
      var user = _mapper.Map<User>(model);
      user.Id = id;

      try
      {
        // update user 
        _userService.Update(user, model.Password);
        return Ok();
      }
      catch (AppException ex)
      {
        // return error message if there was an exception
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
      var currentUserId = int.Parse(User.Identity.Name);
      if (id != currentUserId)
        return Forbid();

      _userService.Delete(id);
      return Ok();
    }
  }
}
