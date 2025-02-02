using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Models.Users;
using WebApi.Helpers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace WebApi.Services
{
  public interface IUserService
  {
    User getFacebookUser(string username);
    UserModel authenticateFacebookUser(string username);
    UserModel Authenticate(string username, string password);
    IEnumerable<User> GetAll();
    User GetById(int id);
    User Create(User user, string password, bool allowNullPassword = false);
    void Update(User user, string password = null);
    void Delete(int id);
  }

  public class UserService : IUserService
  {
    private DataContext _context;
    private readonly AppSettings _appSettings;

    public UserService(DataContext context, IOptions<AppSettings> appSettings)
    {
      _context = context;
      _appSettings = appSettings.Value;
    }

    public User getFacebookUser(string username)
    {
      var user = _context.Users.SingleOrDefault(x => x.Username == username);
      if (user == null)
        return null;
      return user;
    }

    public UserModel authenticateFacebookUser(string username)
    {
      var user = _context.Users.SingleOrDefault(x => x.Username == username);
      if (user == null)
        return null;

      // authentication successful so generate jwt token
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
          {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
          }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);

      var _user = new UserModel
      {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Token = tokenHandler.WriteToken(token),
      };
      return _user;
    }
    public UserModel Authenticate(string username, string password)
    {
      if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return null;

      var user = _context.Users.SingleOrDefault(x => x.Username == username);


      // check if username exists
      if (user == null)
        return null;

      // check if password is correct
      if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return null;

      // authentication successful so generate jwt token
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
          {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
          }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);

      var _user = new UserModel
      {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Token = tokenHandler.WriteToken(token),
      };
      return _user;
    }

    public IEnumerable<User> GetAll()
    {
      return _context.Users;
    }

    public User GetById(int id)
    {
      return _context.Users.Find(id);
    }

    public User Create(User user, string password, bool allowNullPassword)
    {
      // validation
      if (string.IsNullOrWhiteSpace(password) && !allowNullPassword)
        throw new AppException("Password is required");

      if (_context.Users.Any(x => x.Username == user.Username))
        throw new AppException("Username \"" + user.Username + "\" is already taken");

      byte[] passwordHash, passwordSalt;
      CreatePasswordHash(password, out passwordHash, out passwordSalt);

      user.PasswordHash = passwordHash;
      user.PasswordSalt = passwordSalt;

      _context.Users.Add(user);
      _context.SaveChanges();

      return user;
    }

    public void Update(User userParam, string password = null)
    {
      var user = _context.Users.Find(userParam.Id);

      if (user == null)
        throw new AppException("User not found");

      // update username if it has changed
      if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
      {
        // throw error if the new username is already taken
        if (_context.Users.Any(x => x.Username == userParam.Username))
          throw new AppException("Username " + userParam.Username + " is already taken");

        user.Username = userParam.Username;
      }

      // update user properties if provided
      if (!string.IsNullOrWhiteSpace(userParam.FirstName))
        user.FirstName = userParam.FirstName;

      if (!string.IsNullOrWhiteSpace(userParam.LastName))
        user.LastName = userParam.LastName;

      // update password if provided
      if (!string.IsNullOrWhiteSpace(password))
      {
        byte[] passwordHash, passwordSalt;
        CreatePasswordHash(password, out passwordHash, out passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
      }

      _context.Users.Update(user);
      _context.SaveChanges();
    }

    public void Delete(int id)
    {
      var user = _context.Users.Find(id);
      if (user != null)
      {
        _context.Users.Remove(user);
        _context.SaveChanges();
      }
    }

    // private helper methods

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      // if (password == null) throw new ArgumentNullException("password");
      if (password == null)
      {
        passwordSalt = null;
        passwordHash = null;
        return;
      }
      // if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
      if (password == null) throw new ArgumentNullException("password");
      if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
      if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
      if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

      using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
      {
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        for (int i = 0; i < computedHash.Length; i++)
        {
          if (computedHash[i] != storedHash[i]) return false;
        }
      }

      return true;
    }
  }
}