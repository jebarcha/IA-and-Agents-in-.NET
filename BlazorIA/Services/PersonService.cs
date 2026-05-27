using BlazorIA.Data;
using BlazorIA.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorIA.Services
{
    public class PersonService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IPersonService
    {
        public async Task<IEnumerable<Person>> GetAll()
        {
            using var context = dbContextFactory.CreateDbContext();
            return await context.People.ToListAsync();
        }
    }
}
