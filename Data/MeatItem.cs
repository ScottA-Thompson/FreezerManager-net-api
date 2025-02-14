using System;
using System.ComponentModel.DataAnnotations;

namespace FreezerManager.Models
{
    public class MeatItem
    {
        public int Id {get; set;} //primary key
    
        [Required]
        public string Type {get; set;} 
        [Required]
        public string Cut {get; set;} 
        [Range(1, double.MaxValue, ErrorMessage = "Weight must be greater than 0")]
        public double Weight {get; set;} 
        [Required]
        public StorageLocation Storage {get; set;} = StorageLocation.Freezer;
        public DateTime DateAdded {get; set;} = DateTime.Now;
    }
}
