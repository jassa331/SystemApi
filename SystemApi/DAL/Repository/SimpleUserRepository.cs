using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using SystemApi.DAL.Data;
using SystemApi.DTO;
using SystemApi.Entities;

public class SimpleUserRepository : ISimpleUserRepository
{
    private readonly AppDbContext _context;

    public SimpleUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SimpleUser>> GetAllAsync()
    {
        var list = new List<SimpleUser>();

        using SqlConnection con = new SqlConnection(
            _context.Database.GetDbConnection().ConnectionString);

        using SqlCommand cmd = new SqlCommand("sp_GetSimpleUsers", con);
        cmd.CommandType = CommandType.StoredProcedure;

        con.Open();
        using var reader = await cmd.ExecuteReaderAsync();

        while (reader.Read())
        {
            list.Add(new SimpleUser
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString()
            });
        }

        return list;
    }
}
