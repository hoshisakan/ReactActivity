using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Activities", Schema = "reactactivity")]
    public class Activity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ICollection<ActivityAttendee> Attendees { get; set; } = new List<ActivityAttendee>();
    }
}