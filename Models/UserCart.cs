using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BikeVille.Models
{
    public class UserCart
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name {get;set;}
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int OrderQty { get; set; }
        [Required]
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime AddedAt {get;set;} = DateTime.UtcNow;
         [ForeignKey("CustomerId")]
        public virtual Customer Customer {get;set;}
         [ForeignKey("ProductId")]
        public virtual Product Product {get;set;}

    }
}