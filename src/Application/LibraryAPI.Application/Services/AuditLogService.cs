using AutoMapper;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        private async Task EnrichtWithUserNamesAsync(IEnumerable<AuditLogDto> dtos)
        {
            var userIds = dtos.Where(d => d.UserId != null).Select(d => d.UserId!).Distinct().ToList();
            if (!userIds.Any()) return;

            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
            var userDict = users.ToDictionary(u => u.Id, u => u.FullName);

            foreach (var dto in dtos)
            {
                if (dto.UserId != null && userDict.TryGetValue(dto.UserId, out var name))
                {
                    dto.UserName = name;
                }
            }
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAuditLogsAsync()
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
            await EnrichtWithUserNamesAsync(dtos);
            return dtos;
        }

        public async Task<AuditLogDto?> GetAuditLogByIdAsync(int id)
        {
            var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
            if (log == null) return null;

            var dto = _mapper.Map<AuditLogDto>(log);
            await EnrichtWithUserNamesAsync(new[] { dto });
            return dto;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByTableAsync(string tableName)
        {
            var logs = await _unitOfWork.AuditLogs.FindAsync(a => a.TableName == tableName);
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
            await EnrichtWithUserNamesAsync(dtos);
            return dtos;
        }
    }
}
