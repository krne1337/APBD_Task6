namespace APBD_Task6.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductWarehouse> ProductWarehouses { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(o => o.IdProduct);

            modelBuilder.Entity<ProductWarehouse>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(pw => pw.IdOrder);

            modelBuilder.Entity<ProductWarehouse>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(pw => pw.IdProduct);

            modelBuilder.Entity<ProductWarehouse>()
                .HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(pw => pw.IdWarehouse);
        }
    }

}
