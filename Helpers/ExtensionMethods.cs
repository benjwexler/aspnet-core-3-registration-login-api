using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Helpers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<User> WithoutPasswords(this IEnumerable<User> users) 
        {
            if (users == null) return null;

            return users.Select(x => x.WithoutPassword());
        }

        public static User WithoutPassword(this User user) 
        {
            if (user == null) return null;

            user.PasswordHash = null;
            user.PasswordSalt = null;
            return user;
        }

      //   public partial Boolean OnlyAllowIfIdEqualsCurrentUser()
      //    var currentUserId = int.Parse(User.Identity.Name);
      // if (id != int.Parse(currentUserId))
      //   return Forbid();
    }


}