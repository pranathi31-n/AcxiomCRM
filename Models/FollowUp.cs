using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class FollowUp : IValidatableObject
{
    public int FollowUpId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }

    [DataType(DataType.Date)]
    public DateTime FollowUpDate { get; set; } = DateTime.Today;

    [Required, StringLength(40)] public string FollowUpType { get; set; } = "Call";
    [StringLength(500)] public string Remarks { get; set; } = "";
    [Required, StringLength(30)] public string Status { get; set; } = "Planned";
    public string? AssignedTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == "Planned" && FollowUpDate.Date < DateTime.Today)
            yield return new ValidationResult("Follow-up date cannot be earlier than today.", new[] { nameof(FollowUpDate) });
        if (CustomerId is null && LeadId is null)
            yield return new ValidationResult("Select a customer or lead.");
    }
}