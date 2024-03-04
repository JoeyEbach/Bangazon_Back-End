using System.ComponentModel.DataAnnotations;

namespace Bangazon.DTOs
{
    public class ClosedCartDto
    {
        public int Id { get; set; }
        public string PaymentType { get; set; }
        public DateTime dateCreated { get; set; }
        public bool Shipping { get; set; }
        public bool Closed { get; set; }
    }
}

