using MCPServer.Entities;

namespace MCPServer.Services
{
    public interface IPersonsRepository
    {
        bool UpdateIsActive(int id, bool activo);
        Person? GetById(int id);
        List<Person> GetAll();
    }
}
