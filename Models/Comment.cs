using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Blogger_Project.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment cannot be empty")]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Comment must be between 2 to 1000 characters")]
        public string Content { get; set; }
        public DateTime CommentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Required(ErrorMessage ="The User Name is Required")]
        public string UserName { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

    }
}
