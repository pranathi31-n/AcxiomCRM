using AcxiomCRM.Data;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardData> GetAsync(string? userId, bool managerScope)
    {
        var customers = _db.Customers.AsQueryable();
        var leads = _db.Leads.AsQueryable();
        var opps = _db.Opportunities.AsQueryable();

        if (!managerScope && userId != null)
        {
            customers = customers.Where(x => x.CreatedBy == userId);
            leads = leads.Where(x => x.AssignedTo == userId);
            opps = opps.Where(x => x.AssignedTo == userId);
        }

        // SQLite does not support SumAsync on decimal values.
        // Load the open opportunity amounts first, then calculate the sum in .NET.
        var pipelineAmounts = await opps
            .Where(x => x.Status == "Open")
            .Select(x => x.Amount)
            .ToListAsync();

        var pipeline = pipelineAmounts.Sum();

        return new DashboardData
        {
            Customers = await customers.CountAsync(),

            Leads = await leads.CountAsync(),

            OpenLeads = await leads.CountAsync(
                x => x.Status != "Lost" && x.Status != "Converted"),

            Opportunities = await opps.CountAsync(),

            OpenOpportunities = await opps.CountAsync(
                x => x.Status == "Open"),

            Won = await opps.CountAsync(
                x => x.Status == "Won"),

            Lost = await opps.CountAsync(
                x => x.Status == "Lost"),

            Pipeline = pipeline,

            LeadStatus = await leads
                .GroupBy(x => x.Status)
                .Select(g => new ChartPoint(g.Key, g.Count()))
                .ToListAsync(),

            OpportunityStage = await opps
                .GroupBy(x => x.Stage)
                .Select(g => new ChartPoint(g.Key, g.Count()))
                .ToListAsync()
        };
    }
}

public record ChartPoint(string Label, int Value);

public class DashboardData
{
    public int Customers { get; set; }

    public int Leads { get; set; }

    public int OpenLeads { get; set; }

    public int Opportunities { get; set; }

    public int OpenOpportunities { get; set; }

    public int Won { get; set; }

    public int Lost { get; set; }

    public decimal Pipeline { get; set; }

    public List<ChartPoint> LeadStatus { get; set; } = [];

    public List<ChartPoint> OpportunityStage { get; set; } = [];
}