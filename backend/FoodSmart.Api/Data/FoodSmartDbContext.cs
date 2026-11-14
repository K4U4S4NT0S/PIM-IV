using FoodSmart.Api.Models;
using FoodSmart.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Data;

public class FoodSmartDbContext : DbContext
{
    public FoodSmartDbContext(DbContextOptions<FoodSmartDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(180).IsRequired();
            entity.Property(user => user.Email).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Role).HasMaxLength(40).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table => table.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0"));
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Category).HasMaxLength(40).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(600).IsRequired();
            entity.Property(product => product.Emoji).HasMaxLength(20).IsRequired();
            entity.Property(product => product.ImageUrl).HasMaxLength(1000);
            entity.Property(product => product.Price).HasColumnType("decimal(10,2)").IsRequired();
            entity.HasIndex(product => new { product.IsActive, product.Category });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders", table => table.HasCheckConstraint("CK_Orders_Total_NonNegative", "[Total] >= 0"));
            entity.HasKey(order => order.Id);
            entity.Property(order => order.CustomerName).HasMaxLength(120).IsRequired();
            entity.Property(order => order.CustomerPhone).HasMaxLength(30).IsRequired();
            entity.Property(order => order.CustomerAddress).HasMaxLength(250);
            entity.Property(order => order.Status).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Total).HasColumnType("decimal(10,2)").IsRequired();
            entity.HasIndex(order => new { order.Status, order.CreatedAt });
            entity.HasIndex(order => order.CreatedAt);
            entity.HasOne(order => order.User)
                .WithMany()
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(order => order.Items)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems", table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            });
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Quantity).IsRequired();
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(10,2)").IsRequired();
            entity.HasIndex(item => item.OrderId);
            entity.HasIndex(item => item.ProductId);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public static class FoodSmartDatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodSmartDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.EnsureCreatedAsync();
        await EnsureSchemaAsync(db);

        var adminPassword = configuration["SeedAdmin:Password"];
        if (!await db.Users.AnyAsync() && !string.IsNullOrWhiteSpace(adminPassword))
        {
            db.Users.Add(new User
            {
                Name = "Administrador FoodSmart",
                Email = "admin@foodsmart.com",
                Role = "Admin",
                PasswordHash = passwordHasher.Hash(adminPassword),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(GetSeedProducts());
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureSchemaAsync(FoodSmartDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('Orders', 'UserId') IS NULL
BEGIN
    ALTER TABLE Orders ADD UserId int NULL;
END
");
    }

    private static IEnumerable<Product> GetSeedProducts()
    {
        return new[]
        {
            NewProduct("lanche", "X-Tudo Prime", "Blend bovino, cheddar, bacon, ovo, salada fresca e molho especial da casa.", 39.90m, "🍔", "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1000&q=88", true),
            NewProduct("lanche", "Double Smash", "Dois discos smash prensados na chapa com cheddar duplo, picles e maionese defumada.", 34.90m, "🍔", "https://images.unsplash.com/photo-1553979459-d2229ba7433b?auto=format&fit=crop&w=1000&q=88", true),
            NewProduct("lanche", "Chicken Crispy", "Frango crocante, queijo, salada e molho levemente picante.", 29.90m, "🍗", "https://images.unsplash.com/photo-1606755962773-d324e2d2404c?auto=format&fit=crop&w=1000&q=88", true),
            NewProduct("lanche", "Bacon Melt", "Hambúrguer artesanal, bacon crocante, cheddar cremoso e cebola caramelizada.", 36.90m, "🥓", "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Brasa Burger", "Carne grelhada, queijo prato, barbecue premium e onion rings.", 41.90m, "🔥", "https://images.unsplash.com/photo-1586190848861-99aa4a171e90?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Cheddar Volcano", "Muito cheddar, burger suculento, bacon em tiras e pão brioche.", 38.90m, "🧀", "https://images.unsplash.com/photo-1571091718767-18b5b1457add?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Veggie Smart", "Burger vegetal, rúcula, tomate, queijo e molho verde.", 31.90m, "🥬", "https://images.unsplash.com/photo-1520072959219-c595dc870360?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Costela BBQ", "Burger de costela, queijo, barbecue, crispy de cebola e picles.", 44.90m, "🥩", "https://images.unsplash.com/photo-1550317138-10000687a72b?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Classic Salad", "Clássico equilibrado com carne, queijo, alface, tomate e molho da casa.", 27.90m, "🥗", "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("lanche", "Pepper Fire", "Burger apimentado, jalapeño, cheddar, bacon e molho fire.", 40.90m, "🌶️", "https://images.unsplash.com/photo-1553979459-d2229ba7433b?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Coca-Cola Lata", "Refrigerante gelado para acompanhar o combo.", 7.90m, "🥤", "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Fanta Laranja", "Refrigerante sabor laranja, bem gelado.", 7.90m, "🥤", "https://images.unsplash.com/photo-1624517452488-04869289c4ca?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Guaraná Gelado", "Guaraná gelado para combo clássico.", 7.90m, "🥤", "https://images.unsplash.com/photo-1613478223719-2ab802602423?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Limonada da Casa", "Limonada fresca com toque cítrico.", 12.90m, "🍋", "https://images.unsplash.com/photo-1621263764928-df1444c5e859?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Milkshake Chocolate", "Milkshake cremoso de chocolate.", 18.90m, "🥤", "https://images.unsplash.com/photo-1572490122747-3968b75cc699?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("bebida", "Suco Natural", "Suco natural da fruta, feito na hora.", 11.90m, "🧃", "https://images.unsplash.com/photo-1600271886742-f049cd451bba?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Batata Rústica", "Batata crocante com tempero especial.", 14.90m, "🍟", "https://images.unsplash.com/photo-1576107232684-1279f390859f?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Onion Rings", "Anéis de cebola empanados.", 15.90m, "🧅", "https://images.unsplash.com/photo-1639024471283-03518883512d?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Cheddar Extra", "Porção extra de cheddar cremoso.", 6.90m, "🧀", "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Bacon Crocante", "Bacon crocante extra.", 7.90m, "🥓", "https://images.unsplash.com/photo-1528607929212-2636ec44253e?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Molho Especial", "Molho artesanal da casa.", 4.90m, "🥫", "https://images.unsplash.com/photo-1472476443507-c7a5948772fc?auto=format&fit=crop&w=1000&q=88", false),
            NewProduct("adicional", "Picles Artesanal", "Picles agridoce artesanal.", 4.90m, "🥒", "https://images.unsplash.com/photo-1620574387735-3624d75b2dbc?auto=format&fit=crop&w=1000&q=88", false)
        };
    }

    private static Product NewProduct(string category, string name, string description, decimal price, string emoji, string imageUrl, bool isFeatured)
    {
        return new Product
        {
            Category = category,
            Name = name,
            Description = description,
            Price = price,
            Emoji = emoji,
            ImageUrl = imageUrl,
            IsFeatured = isFeatured,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
