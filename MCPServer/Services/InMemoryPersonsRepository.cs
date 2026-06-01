using MCPServer.Entities;

namespace MCPServer.Services
{
    public class InMemoryPersonsRepository : IPersonsRepository
    {
        private List<Person> _persons;

        public InMemoryPersonsRepository()
        {
            _persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                Name = "Jose Barajas",
                Email = "jose.barajas@email.com",
                Salary = 50000,
                IsActive = true
            },
            new Person
            {
                Id = 2,
                Name = "Claudia Rodríguez",
                Email = "claudia.rodriguez@email.com",
                Salary = 65000,
                IsActive = true
            },
            new Person
            {
                Id = 3,
                Name = "Carlos Rodríguez",
                Email = "carlos.rodriguez@email.com",
                Salary = 45000,
                IsActive = false
            }
        };
        }
        public List<Person> GetAll()
        {
            return _persons;
        }

        public Person? GetById(int id)
        {
            return _persons.FirstOrDefault(p => p.Id == id);
        }

        public bool UpdateIsActive(int id, bool activo)
        {
            var person = _persons.FirstOrDefault(p => p.Id == id);

            if (person is null)
            {
                return false;
            }

            person.IsActive = activo;
            return true;
        }
    }
}
