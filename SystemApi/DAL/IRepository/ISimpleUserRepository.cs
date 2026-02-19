using SystemApi.DTO;
using SystemApi.Entities;

public interface ISimpleUserRepository
{
    Task<List<SimpleUser>> GetAllAsync();
}
