using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models;
namespace EventEase_st10157545_POE.Services
{
    public class AuditService
    {
        private readonly EventEaseDbContext _context;

        public AuditService(EventEaseDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int specialistId, string action, string table, int recordId, string? details = null)
        {
            _context.AuditLog.Add(new AuditLogViewModel { 
                SpecialistID = specialistId,
                Action = action,
                TablesAffected = table,
                RecordID = recordId,
                Timestamp = DateTime.UtcNow,
                Details = details
            });

            await _context.SaveChangesAsync();
        }
    }
}
