using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Dtos;
using OrderFlow.Application.Services;

namespace OrderFlow.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    // Example: GET /api/reports/sales?startDate=2026-01-01&endDate=2026-12-31
    [HttpGet("sales")]
    public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary(
        [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate) =>
        Ok(await reportService.GetSalesSummaryAsync(startDate, endDate));
}
