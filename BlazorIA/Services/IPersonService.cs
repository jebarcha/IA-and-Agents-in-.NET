using BlazorIA.Entities;
using System.ComponentModel;

namespace BlazorIA.Services
{
    [Description("Service to interact with people")]
    public interface IPersonService
    {
        [Description("Get a list of all people")]
        Task<IEnumerable<Person>> GetAll();
    }
}