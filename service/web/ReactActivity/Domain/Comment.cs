using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Comments", Schema = "reactactivity")]
    public class Comment
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public AppUser Author { get; set; }
        public Activity Activity { get; set; }
        //TODO: Because set not allow to use timezone in PostgreSQL, so set UTC time will not be see as UTC time in PostgreSQL
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}