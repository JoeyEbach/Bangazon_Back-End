using System.ComponentModel.DataAnnotations;

namespace Bangazon.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Uid { get; set; }
        public bool Seller { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
