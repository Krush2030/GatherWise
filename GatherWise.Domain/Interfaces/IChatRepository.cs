using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task<IEnumerable<AdminOwnerChatMessage>> GetChatHistoryByReportIdAsync(int reportId);
        Task AddMessageAsync(AdminOwnerChatMessage message);
    }
}