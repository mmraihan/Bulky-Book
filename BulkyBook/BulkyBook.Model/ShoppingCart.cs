using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BulkyBook.Models
{
   public class ShoppingCart //Products to be added to shoppping cart, cart to be saved in DB
    {
        public ShoppingCart()
        {
           Count = 1;
        }

        [Key]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } //what user has many items added in the shopping cart
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId ")]
        public Product Product { get; set; }

        [Range(1,1000,ErrorMessage ="Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; } //based on count selected, price will be loaded to UI

    }
}
