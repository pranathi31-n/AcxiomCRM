using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class Lead
{
    public int LeadId { get; set; }
    [Required, StringLength(30)] public string LeadCode { get; set; } = "";
    [Required, StringLength(120)] public string LeadName { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(20)] public string Phone { get; set; } = "";
    [StringLength(120)] public string CompanyName { get; set; } = "";
    [Required, StringLength(60)] public string Source { get; set; } = "Website";
    [Required, StringLength(30)] public string Status { get; set; } = "New";
    [Range(0, 1000000000)] public decimal ExpectedValue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; }
}