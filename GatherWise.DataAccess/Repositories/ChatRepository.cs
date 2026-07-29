using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminOwnerChatMessage>> GetChatHistoryByReportIdAsync(int reportId)
        {
            return await _context.Set<AdminOwnerChatMessage>()
                .Where(m => m.UserReportId == reportId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task AddMessageAsync(AdminOwnerChatMessage message)
        {
            await _context.Set<AdminOwnerChatMessage>().AddAsync(message);
            await _context.SaveChangesAsync();
        }
    }
}