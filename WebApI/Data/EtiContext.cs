using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Data2
{
    public class EtiContext : DbContext
    {
        public EtiContext(DbContextOptions<EtiContext> options) : base(options)
        {
        }
        
        public DbSet<PrintRequest> PrintRequests { get; set; }
        public DbSet<RequestFile> RequestFiles { get; set; }

        public DbSet<ApplicationCredential> ApplicationCredential { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación 1:N entre PrintRequest y RequestFiles
            modelBuilder.Entity<RequestFile>()
                .HasOne(f => f.PrintRequest)
                .WithMany(p => p.RequestFiles)
                .HasForeignKey(f => f.PrintRequestId);

            // Propiedad compuesta (ClientData)
            modelBuilder.Entity<PrintRequest>()
                .OwnsOne(p => p.Data);
        }
    }


    // Modelo de ejemplo
    [Table("Orders", Schema = "Orders")]
    public class Orders
    {
        public int Id { get; set; }

        [Display(Name = "Código del Producto")]
        public string ProductId { get; set; } = string.Empty;
        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }
        [Display(Name = "Cliente")]
        public string ClientName { get; set; } = string.Empty;
        [Display(Name = "Direccion")]
        public string Address { get; set; } = string.Empty;
    }

    public class ApplicationCredential
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Table("Requests", Schema = "dbo")]
    public class PrintRequest
    {
        [Key]
        public Guid PrintRequestId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string RequestNumber { get; set; }

        [Required, MaxLength(20)]
        public string DeliveryType { get; set; } // SITE | LOCAL | NATIONAL

        [Required]
        public ClientData Data { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<RequestFile> RequestFiles { get; set; } = new();
    }

    [Owned]
    public class ClientData
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required, MaxLength(200)]
        public string ClientName { get; set; }

        [Required, MaxLength(50)]
        public string ClientCode { get; set; }

        [MaxLength(100)]
        public string? AddressLine1 { get; set; }

        [MaxLength(100)]
        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        public string? AddressLine3 { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Attent { get; set; }
    }

    [Table("RequestFiles", Schema = "dbo")]
    public class RequestFile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(PrintRequest))]
        public Guid PrintRequestId { get; set; }

        [Required, MaxLength(10)]
        public string Type { get; set; } // EDI | PRINT

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public int TotalLabels { get; set; }

        [Required, Url]
        public string Url { get; set; }

        public PrintRequest PrintRequest { get; set; }
    }

}