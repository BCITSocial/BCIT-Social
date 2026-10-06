using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvpDatabase.Models
{
    public class Profile
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string Program { get; set; }
        public string Campus { get; set; }
        
        public virtual User User { get; set; }
    }
}