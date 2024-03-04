using Microsoft.EntityFrameworkCore;
using Bangazon.Models;

public class BangazonDbContext : DbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>().HasData(new User[]
		{
			new User {Id = 1, Username = "Bobjohn", Email = "bob@john.com", PhoneNumber = "654-234-124", Seller = true, Uid = "1a456saldkn3532"},
            new User {Id = 2, Username = "CarolFaye", Email = "carol@faye.com", PhoneNumber = "124-257-253", Seller = false, Uid = "1a456saldkh42017"},
			new User {Id = 3, Username = "Hunter11", Email = "11@hunter.com", PhoneNumber = "615-256-127", Seller = true, Uid = "1a456salfr6078"}
        });

        modelBuilder.Entity<Product>()
        .HasOne(p => p.Seller)
        .WithMany(u => u.Products)
        .HasForeignKey(p => p.SellerId);

        modelBuilder.Entity<Product>().HasData(new Product[]
		{
			new Product {Id = 1, Title = "Walkman", Price = 15.99M, Description = "It's in good condition!", Quantity = 5, CategoryId = 1, SellerId = 1},
            new Product {Id = 2, Title = "TypeWriter", Price = 75.00M, Description = "Works like new!", Quantity = 7, CategoryId = 2, SellerId = 3},
            new Product {Id = 3, Title = "Frisbee", Price = 10.00M, Description = "The frisbee used by the pros!", Quantity = 34, CategoryId = 3, SellerId = 1},
            new Product {Id = 4, Title = "Notepad", Price = 7.00M, Description = "Take notes on the go!", Quantity = 78, CategoryId = 2, SellerId = 3},
            new Product {Id = 5, Title = "Flat Screen TV", Price = 799.00M, Description = "Your favorite movies never looked so good!", Quantity = 3, CategoryId = 1, SellerId = 1}
        });

        modelBuilder.Entity<Order>()
        .HasMany(o => o.Products)
        .WithMany(p => p.Orders);

		modelBuilder.Entity<Order>().HasData(new Order[]
		{
			new Order {Id = 1, CustomerId = 2, Closed = true, dateCreated = new DateTime(2023, 12, 07), PaymentType = "Visa", Shipping = true},
            new Order {Id = 2, CustomerId = 1, Closed = true, dateCreated = new DateTime(2024, 01, 12), PaymentType = "MasterCard", Shipping = false},
            new Order {Id = 3, CustomerId = 2, Closed = false, dateCreated = new DateTime(2024, 02, 19), PaymentType = "Visa", Shipping = true}
        });

		modelBuilder.Entity<Category>().HasData(new Category[]
		{
			new Category {Id = 1, Name = "Electronics"},
            new Category {Id = 2, Name = "Office Supplies"},
            new Category {Id = 3, Name = "Outdoors"}
        });

	}
 

    public BangazonDbContext(DbContextOptions<BangazonDbContext> context) : base(context)
	{

	}	
}

