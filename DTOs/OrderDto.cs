using System.ComponentModel.DataAnnotations;

namespace Bangazon.DTOs
{
	public class OrderDto
	{
        public int CustomerId { get; set; }
        public string PaymentType { get; set; }
        public DateTime dateCreated { get; set; }
        public bool Shipping { get; set; }
        public bool Closed { get; set; }
    }
}

