using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor http)
    {
        _db = db; _http = http;
    }

    public async Task LogAsync(string action, string entity, string? recordId, string? oldValue = null, string? newValue = null, string result = "Success")
    {
        var userId = _http.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _db.AuditLogs.Add(new AuditLog {
            UserId = userId, Action = action, EntityName = entity, RecordId = recordId,
            OldValue = oldValue, NewValue = newValue, Result = result,
            IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();
    }
}