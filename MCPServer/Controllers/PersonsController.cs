using MCPServer.Entities;
using MCPServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCPServer.Controllers
{
    [ApiController]
    [Route("api/persons")]
    public class PersonsController(IPersonsRepository personsRepository)
    {
        [HttpGet]
        public List<Person> Get()
        {
            return personsRepository.GetAll();
        }
    }
}
