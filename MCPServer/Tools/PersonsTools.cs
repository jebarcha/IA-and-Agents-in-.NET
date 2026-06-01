using MCPServer.DTOs;
using MCPServer.Entities;
using MCPServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.Tools
{
    [McpServerToolType]
    public class PersonsTools(IPersonsRepository personsRepository)
    {
        [McpServerTool, Description("Get a list of all registered persons.")]
        public List<Person> GetAll()
        {
            var persons = personsRepository.GetAll();
            return persons;
        }

        [McpServerTool, Description("Get a person by Id.")]
        public Person? GetById(
        [Description("Identificador único de la persona.")] int id)
        {
            var persona = personsRepository.GetById(id);
            return persona;
        }

        [McpServerTool, Description("Activate or Deactivate a person by Id.")]
        public OperationResultDTO UpdateIsActive(
        [Description("Id of the person.")] int id,
        [Description("Specify if the person is active (true) o inactive (false).")] bool isActive)
        {
            var isUpdated = personsRepository.UpdateIsActive(id, isActive);

            if (!isUpdated)
            {
                return new OperationResultDTO(false, $"Cannot update the person with id {id}. Verify that the person exists.");
            }

            return new OperationResultDTO(true, "The change was made successfully.");
        }

    }
}
