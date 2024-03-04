using System.ComponentModel.DataAnnotations;

namespace Bangazon.DTOs
{
    public class OrderUpdateDto
    {
        public string PaymentType { get; set; }
        public bool Shipping { get; set; }
        public bool Closed { get; set; }
    }
}

