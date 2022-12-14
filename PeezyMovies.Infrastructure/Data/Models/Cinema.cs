namespace PeezyMovies.Infrastructure.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Cinema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Logo { get; set; }

        [Required]
        [MaxLength(GlobalConstants.Cinema.NameMaxLength)]
        public string Name { get; set; }

        [Required]
        [MaxLength(GlobalConstants.Cinema.DescriptionMaxLength)]
        public string Description { get; set; }

        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}
