using System.ComponentModel.DataAnnotations;

namespace Bangazon.DTOs
{
	public class UserDto
	{
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Uid { get; set; }
        public bool Seller { get; set; }
    }
}

