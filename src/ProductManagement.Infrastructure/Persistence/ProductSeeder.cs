using Microsoft.EntityFrameworkCore;

using ProductManagement.Domain.Products;

namespace ProductManagement.Infrastructure.Persistence;

public static class ProductSeeder
{
    private static readonly (string Sku, string Name, string? Description, decimal Price, int Stock)[] SeedCatalog =
    [
        ("KB-100", "Wireless Keyboard", "Slim wireless keyboard with USB receiver.", 29.99m, 150),
        ("MS-200", "Wireless Mouse", "Ergonomic wireless mouse with adjustable DPI.", 19.99m, 200),
        ("HUB-300", "USB-C Hub", "6-in-1 USB-C hub with HDMI and SD card reader.", 34.99m, 45),
        ("CBL-USB-C-1M", "USB-C Charging Cable", "1-meter braided USB-C charging cable.", 9.99m, 0),
        ("KB-MECH-87", "Mechanical Keyboard", "87-key mechanical keyboard with hot-swappable switches.", 89.99m, 30),
        ("MS-ERGO-1", "Ergonomic Mouse", "Vertical ergonomic mouse for wrist comfort.", 39.99m, 5),
        ("MON-24-FHD", "24-Inch Monitor", "24-inch full HD IPS monitor.", 129.99m, 12),
        ("MON-27-4K", "27-Inch 4K Monitor", "27-inch 4K UHD monitor with USB-C input.", 349.99m, 8),
        ("CAM-1080", "1080p Webcam", "Full HD webcam with built-in microphone.", 44.99m, 60),
        ("HS-NC-500", "Noise Cancelling Headset", "Over-ear headset with active noise cancellation.", 79.99m, 3),
        ("SPK-BT-10", "Bluetooth Speaker", "Portable Bluetooth speaker with 10-hour battery.", 49.99m, 0),
        ("STAND-LAP-1", "Laptop Stand", "Adjustable aluminum laptop stand.", 24.99m, 120),
        ("MAT-DESK-XL", "Extra-Large Desk Mat", "Extra-large desk mat with stitched edges.", 22.99m, 300),
        ("DESK-CONV-1", "Standing Desk Converter", "Height-adjustable standing desk converter.", 199.99m, 4),
        ("CHAIR-ERGO-1", "Ergonomic Office Chair", "Mesh-back ergonomic office chair with lumbar support.", 249.99m, 2),
        ("DOCK-USB-C-1", "USB-C Docking Station", "Dual-monitor USB-C docking station.", 149.99m, 25),
        ("CHG-PAD-10W", "Wireless Charging Pad", "10W fast wireless charging pad.", 19.99m, 100),
        ("SSD-1TB-PORT", "Portable SSD 1TB", "1TB USB-C portable solid-state drive.", 99.99m, 40),
        ("HDD-4TB-EXT", "External Hard Drive 4TB", "4TB USB 3.0 external hard drive.", 109.99m, 15),
        ("ADPT-USB-C-7IN1", "USB-C Multiport Adapter", "7-in-1 USB-C adapter with HDMI and Ethernet.", 39.99m, 55),
        ("CBL-HDMI-2M", "HDMI Cable 2M", "2-meter high-speed HDMI cable.", 8.99m, 500),
        ("CBL-CAT6-5M", "Ethernet Cable Cat6 5M", "5-meter Cat6 Ethernet cable.", 7.99m, 250),
        ("SLEEVE-LAP-15", "Laptop Sleeve 15-Inch", "Padded protective sleeve for 15-inch laptops.", 17.99m, 80),
        ("KB-NUMPAD-1", "Wireless Numpad", "Compact wireless numeric keypad.", 14.99m, 1),
    ];

    public static IReadOnlyList<Product> CreateSeedProducts() =>
        [.. SeedCatalog.Select(p => Product.Create(p.Sku, p.Name, p.Description, p.Price, p.Stock))];

    public static DbContextOptionsBuilder UseProductSeeding(this DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSeeding(Seed).UseAsyncSeeding(SeedAsync);

    public static void Seed(DbContext context, bool _)
    {
        if (context.Set<Product>().Any())
        {
            return;
        }

        context.Set<Product>().AddRange(CreateSeedProducts());
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, bool _, CancellationToken cancellationToken)
    {
        if (await context.Set<Product>().AnyAsync(cancellationToken))
        {
            return;
        }

        await context.Set<Product>().AddRangeAsync(CreateSeedProducts(), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
