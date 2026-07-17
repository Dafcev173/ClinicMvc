using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>Преглед на сите CRUD активности - достапен само за Administrator.</summary>
[Authorize(Roles = "Administrator")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogRepository _auditLogRepository;

    private const int PageSize = 15;

    public AuditLogsController(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>GET: /AuditLogs?page=2 - странирана листа, 15 записи по страница.</summary>
    public async Task<IActionResult> Index(int page = 1)
    {
        var validPage = page < 1 ? 1 : page;

        var logs       = await _auditLogRepository.GetPagedAsync(validPage, PageSize);
        var totalCount = await _auditLogRepository.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        ViewBag.Pagination = new PaginationInfo
        {
            CurrentPage = validPage,
            TotalPages  = totalPages == 0 ? 1 : totalPages,
            TotalCount  = totalCount,
            PageSize    = PageSize
        };

        return View(logs);
    }
}
