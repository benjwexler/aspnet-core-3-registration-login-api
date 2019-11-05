using WebApi.Entities;
using System;

namespace WebApi.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int UserID { get; set; }

        public virtual User User { get; set; }
        public DateTime UpdatedAt { get; set; }
        
    }
}