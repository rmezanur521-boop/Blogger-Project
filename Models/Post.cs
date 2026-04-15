using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Blogger_Project.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 to 150 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 to 5000 characters")]
        public string Content { get; set; }

        [StringLength(50)]
        public string Author { get; set; }
        [ValidateNever]
        [Display(Name = "Feature Image")]
        public string FeatureImagePath { get; set; } 

        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        [ForeignKey("Category")]
        [Display(Name ="Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        public ICollection<Comment> Comments { get; set; }

    }
}