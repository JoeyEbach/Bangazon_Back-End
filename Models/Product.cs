using System.ComponentModel.DataAnnotations;
using Bangazon.Models;

namespace Bangazon.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int SellerId { get; set; }
        public User Seller { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}

