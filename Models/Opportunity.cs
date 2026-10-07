using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class Opportunity : IValidatableObject
{
    public int OpportunityId { get; set; }
    [Required, StringLength(150)] public string OpportunityName { get; set; } = "";
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }

    [Range(typeof(decimal), "0.01", "100000000000")]
    public decimal Amount { get; set; }

    [Required, StringLength(40)] public string Stage { get; set; } = "Qualification";
    [Range(0, 100)] public int Probability { get; set; }

    [DataType(DataType.Date)]
    public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);

    [Required, StringLength(30)] public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; }

    public Customer? Customer { get; set; }

    public decimal WeightedPipeline => Amount * Probability / 100m;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == "Open" && ExpectedCloseDate.Date < DateTime.Today)
            yield return new ValidationResult("Expected Close Date cannot be in the past.", new[] { nameof(ExpectedCloseDate) });
        if (Status == "Open" && Amount <= 0)
            yield return new ValidationResult("Opportunity Amount must be greater than 0.", new[] { nameof(Amount) });
    }
}