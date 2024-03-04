using System.ComponentModel.DataAnnotations;

namespace Bangazon.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public int CustomerId { get; set; }
        public string? PaymentType { get; set; }
        public DateTime? dateCreated { get; set; }
        public bool? Shipping { get; set; }
        public bool Closed { get; set; }
        public ICollection<Product> Products { get; set; }
        public decimal? OrderTotal
            {
                get
                {
                    if (Products != null)
                    {
                    return Products.Sum(p => p.Price);
                    }
                    return null;
                }
            }
    }
}

