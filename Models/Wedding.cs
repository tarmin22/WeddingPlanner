using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        // auto-implemented properties need to match the columns in your table
        // the [Key] attribute is used to mark the Model property being used for your table's Primary Key
        [Key]
        public int WeddingId { get; set; }
        public int UserId { get; set; }
        // MySQL VARCHAR and TEXT types can be represeted by a string
        [Required]
        [MinLength(2)]
        public string Wedder1 { get; set; }
        [Required]
        [MinLength(2)]
        public string Wedder2 { get; set; }
        public DateTime WedDate { get; set; }
        [Required]
        [MinLength(10)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<WedUser> WeddingGuest { get; set; }
        public User Creator { get; set; }
    }
}