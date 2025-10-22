using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace WebApi.Data
{

    [Table("ApplicationCredentials")]
    public class ApplicationCredentials
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Password { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    [Table("Requests")]
    public class PrintRequest
    {
        [Key]
        public Guid PrintRequestId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string RequestNumber { get; set; }

        [Required, MaxLength(20)]
        public string DeliveryType { get; set; }

        [Required]
        public ClientData ClientData { get; set; }  

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

    [Table("RequestFiles")]
    public class RequestFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid PrintRequestId { get; set; }

        [Required, MaxLength(10)]
        public string Type { get; set; } 

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public int TotalLabels { get; set; }

        [Required, Url]
        public string Url { get; set; }

        public PrintRequest PrintRequest { get; set; }
    }

    [Table("PrintStatuses")]
    public class PrintStatus
    {
        [Key]
        public int Id { get; set; }

        public Guid PrintRequestId { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public int Code { get; set; }

        [MaxLength(100)]
        public string TrackingId { get; set; }
    }


    public class DeliveryDto
    {
        [Required]
        public string Type { get; set; } = "Address";

        public string? Comment { get; set; }

        [Required]
        public string AddressId { get; set; } = string.Empty;

        public DateTime? ExpectedDate { get; set; }
    }

    public class SalesOrder
    {
        public Guid SalesOrderId { get; set; } = Guid.NewGuid();
        public ICollection<SalesOrderLine> InvoiceLines { get; set; } = new List<SalesOrderLine>();
        public string ReferenceAtCustomer { get; set; } = string.Empty;
        public string LineComment1 { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int OrderQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? AddressId { get; set; }
        public DateTime ExpectedDate { get; set; }
        public Guid TransactionId { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionMethod { get; set; } = "N/A";
        public string OrderState { get; set; } = "Pending";
    }

    public class SalesOrderLine
    {
        public int Id { get; set; }              
        public Guid SalesOrderId { get; set; }  
        public SalesOrder SalesOrder { get; set; } = null!;

        public string InvoicePriceLineId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Type { get; set; } = "SalesOrder";
    }
   
    [Table("AllowedPrintStatuses")]
    public class AllowedPrintStatus
    {
        [Key]

        public int Code { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;
    }




}