using WebApi.Entities;

namespace WebApi.Models
{
    public class PostWithoutUserId
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        // public int UserID { get; set; }

        // public virtual User User { get; set; }
        
    }
}