using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class Customer
{
    public int CustomerId { get; set; }

    [Required, StringLength(30)]
    public string CustomerCode { get; set; } = "";

    [Required, StringLength(120)]
    public string CustomerName { get; set; } = "";

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = "";

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = "";

    [StringLength(120)] public string CompanyName { get; set; } = "";
    [StringLength(250)] public string Address { get; set; } = "";
    [StringLength(80)] public string City { get; set; } = "";
    [StringLength(80)] public string State { get; set; } = "";
    [Required, StringLength(30)] public string Status { get; set; } = "Active";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}