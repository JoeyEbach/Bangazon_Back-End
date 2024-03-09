using Bangazon.Models;
using Bangazon.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddNpgsql<BangazonDbContext>(builder.Configuration["BangazonDbConnectionString"]);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();


//***getUsers: return all users
app.MapGet("/api/users", (BangazonDbContext db) =>
{
    return db.Users.ToList();
});

//check user
app.MapPost("/api/checkuser/{uid}", (BangazonDbContext db, string uid) =>
{
    User checkUser = db.Users.FirstOrDefault(u => u.Uid == uid);
    if (checkUser == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(checkUser);
});

//***getUser: return all information for a single user
app.MapGet("/api/users/{id}", (BangazonDbContext db, int id) =>
{
    return db.Users.FirstOrDefault(u => u.Id == id);
});

//***newUser: return all the information for the newly created user.
app.MapPost("/api/users/new", (BangazonDbContext db, UserDto userObj) =>
{
    try
    {
        User newUser = new(){ Username = userObj.Username, Email = userObj.Email, PhoneNumber = userObj.PhoneNumber, Uid = userObj.Uid, Seller = userObj.Seller };
        db.Users.Add(newUser);
        db.SaveChanges();
        return Results.Created($"/api/users/new/{newUser.Id}", newUser);
    }
    catch
    {
        return Results.BadRequest();
    }
});

//***editUser: return the single user object containing the updated information
app.MapPut("/api/users/update/{id}", (BangazonDbContext db, int id, UserUpdateDto user) =>
{
    User userToUpdate = db.Users.SingleOrDefault(u => u.Id == id);
    if (userToUpdate == null)
    {
        return Results.NotFound();
    }
    userToUpdate.Username = user.Username;
    userToUpdate.Email = user.Email;
    userToUpdate.PhoneNumber = user.PhoneNumber;
    userToUpdate.Seller = user.Seller;

    db.SaveChanges();
    return Results.NoContent();
});

//***getUserSellers: return a list of all users whose seller key = true
app.MapGet("/api/users/sellers", (BangazonDbContext db) =>
{
    return db.Users.Where(u => u.Seller == true).ToList();
});

//***getProducts: return all information for all products, as well as the user object that corresponds with the sellerId of each product
app.MapGet("/api/products", (BangazonDbContext db) =>
{
    return db.Products
    .Include(p => p.Category)
    .Include(p => p.Seller)
    .Include(p => p.Orders)
    .ToList();
});

//***getProduct: return all information for a single product, as well as the user object that corresponds with the sellerId of the product
app.MapGet("/api/products/{id}", (BangazonDbContext db, int id) =>
{
    return db.Products.Include(p => p.Seller).Where(p => p.Id == id);
});

//***getProductsBySellerId: return a list of all products that pertain to the given sellerId
app.MapGet("/api/products/seller/{sellerId}", (BangazonDbContext db, int sellerId) =>
{
    return db.Users.Include(u => u.Products).ThenInclude(p => p.Category).FirstOrDefault(u => u.Id == sellerId);
});

//***getProductsSoldBySellerId: return a list of all products that have been part of a completed order, and have a sellerId that matches the seller's user.Id
app.MapGet("/api/products/sold/{sellerId}", (BangazonDbContext db, int sellerId) =>
{
    return db.Products
    .Include(p => p.Orders)
    .Where(p => p.SellerId == sellerId && p.Orders.Any(o => o.Closed))
    .ToList();
});

//cartCheck when users log in 
app.MapPost("/api/cart/new/{id}", (BangazonDbContext db, int id) =>
{
        Order openOrder = db.Orders.SingleOrDefault(o => o.CustomerId == id && !o.Closed);
        if (openOrder == null)
        {
        try
        {
            Order cart = new Order();
            cart.CustomerId = id;
            cart.Closed = false;
            cart.Shipping = false;
            cart.dateCreated = new DateTime();
            cart.PaymentType = "none";
            db.Orders.Add(cart);
            db.SaveChanges();
        }
        catch
        {
            return Results.BadRequest();
        }
        }
        return Results.NoContent();

});

//getCart
app.MapGet("/api/cart/{userId}", (BangazonDbContext db, int userId) =>
{
    return db.Orders
            .Include(o => o.Products)
            .ThenInclude(p => p.Category)
            .Include(o => o.Products)
            .ThenInclude(p => p.Seller)
            .SingleOrDefault(o => o.CustomerId == userId && o.Closed == false);
});

//closeCart
app.MapPut("/api/cart/close", (BangazonDbContext db, ClosedCartDto dto) =>
{
    Order cart = db.Orders
                .Include(o => o.Products)
                .SingleOrDefault(o => o.Id == dto.Id && o.Closed == false);

    if (cart == null)
    {
        return Results.NotFound("Cart not found");
    }
    if (cart.Products.Count < 1)
    {
        return Results.BadRequest("Cart has no products");
    }
    cart.Closed = true;
    cart.dateCreated = DateTime.Now;
    cart.PaymentType = dto.PaymentType;
    cart.Shipping = dto.Shipping;
    db.SaveChanges();
    return Results.Ok(cart);
});

//***getOrder: return all information for a single order, as well as all of the product objects associated with the order, and the user objects associated with the products
app.MapGet("/api/orders/{orderId}", (BangazonDbContext db, int orderId) =>
{
    return db.Orders
        .Include(o => o.Products)
        .ThenInclude(p => p.Seller)
        .FirstOrDefault(o => o.Id == orderId);
});

//***getOrdersBySellerId: return a list of all orders that contain a product where the sellerId matches the sellerId parameter. Additionally, the seller's product objects should be included with the information
app.MapGet("/api/orders/seller/{sellerId}", (BangazonDbContext db, int sellerId) =>
{
    return db.Orders
            .Include(o => o.Products)
            .Where(o => o.Products.Any(p => p.SellerId == sellerId))
            .Select(o => new Order
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Closed = o.Closed,
                dateCreated = o.dateCreated,
                PaymentType = o.PaymentType,
                Shipping = o.Shipping,
                Products = o.Products.Where(p => p.SellerId == sellerId).ToList()
            })
            .ToList();
});

//***getCompletedOrdersByCustomerId: return a list of all closed orders where the customerId matches the customerId parameter
app.MapGet("/api/orders/customer/{customerId}/completed", (BangazonDbContext db, int customerId) =>
{
    return db.Orders.Include(o => o.Products).Where(o => o.CustomerId == customerId && o.Closed == true).ToList();
});

//***getCategories: returns all categories
app.MapGet("/api/categories", (BangazonDbContext db) =>
{
    return db.Categories.ToList();
});

//***getCategory: returns a single category
app.MapGet("/api/categories/{categoryId}", (BangazonDbContext db, int categoryId) =>
{
    return db.Categories.FirstOrDefault(c => c.Id == categoryId);
});

//SellerProductsInCat: returns the total number of products a seller has in a given category.
app.MapGet("/api/products/{categoryId}/{sellerId}", (BangazonDbContext db, int categoryId, int sellerId) =>
{
    return db.Products.Where(p => p.CategoryId == categoryId && p.SellerId == sellerId).ToList();
});

//20Products
app.MapGet("/api/products/first20", (BangazonDbContext db) =>
{
    return db.Products
        .Include(p => p.Seller)
        .Include(p => p.Category)
        .OrderByDescending(p => p.Id).Take(20).ToList();
});
//***newOrderProduct: returns a new order product relationship
app.MapPost("/api/order/addProduct", (BangazonDbContext db, OrderProductDto orderProduct) =>
{
    var orderToUpdate = db.Orders
        .Include(o => o.Products)
        .FirstOrDefault(o => o.Id == orderProduct.OrderId);
    var productAdd = db.Products.FirstOrDefault(p => p.Id == orderProduct.ProductId);

    try
    {
        orderToUpdate.Products.Add(productAdd);
        db.SaveChanges();
        return Results.NoContent();
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

//***deleteOrderProduct: deletes an existing order product relationship
app.MapDelete("/api/order/{OrderId}/deleteProduct/{ProductId}", (BangazonDbContext db, int OrderId, int ProductId ) =>
{
    var orderToUpdate = db.Orders
        .Include(o => o.Products)
        .FirstOrDefault(o => o.Id == OrderId);
    var productDelete = db.Products.FirstOrDefault(p => p.Id == ProductId);

    try
    {
        orderToUpdate.Products.Remove(productDelete);
        db.SaveChanges();
        return Results.NoContent();
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

//search for products
app.MapGet("/api/products/search/{userInput}", (BangazonDbContext db, string userInput) =>
{
    string searchTerm = userInput.ToLower();
    return db.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Where(p => p.Title.ToLower().Contains(searchTerm) ||
                        p.Description.ToLower().Contains(searchTerm) ||
                        p.Category.Name.ToLower().Contains(searchTerm) ||
                        p.Seller.Username.ToLower().Contains(searchTerm)).ToList();
});

//search for sellers
app.MapGet("/api/sellers/search/{userInput}", (BangazonDbContext db, string userInput) =>
{
    string searchTerm = userInput.ToLower();
    var sellerList = db.Users.Include(u => u.Products).Where(u => u.Seller == true).ToList();

    return sellerList
            .Where(s => s.Username.ToLower().Contains(searchTerm) ||
                        s.PhoneNumber.ToLower().Contains(searchTerm) ||
                        s.Email.ToLower().Contains(searchTerm));
});

// fetches all categories and the first 3 products in each category
app.MapGet("/api/categories/first3", (BangazonDbContext db) =>
{
    return db.Categories.Include(c => c.Products.Take(3)).ToList();
});



app.Run();

