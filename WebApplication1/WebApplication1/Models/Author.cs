using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models {
    public class Author {
        public int AuthorId { get; set; }
        [Required]
        public string Name { get; set; }

    }
}