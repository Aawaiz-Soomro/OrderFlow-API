using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Dtos;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
}

public class CustomerService(OrderFlowDbContext db) : ICustomerService
{
    public Task<List<CustomerDto>> GetAllAsync() =>
        db.Customers
          .AsNoTracking()
          .OrderBy(c => c.Name)
          .Select(c => new CustomerDto(c.Id, c.Name, c.Email))
          .ToListAsync();
}
