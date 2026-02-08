using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using SystemApi.DAL.Data;
using SystemApi.DAL.IRepository;
using SystemApi.Entities;

namespace SystemApi.DAL.Repository
{
    public class AuthRepository : IAuthRepository
    {
        public readonly AppDbContext _dbcontext;
        public AuthRepository(AppDbContext Dbcontext)
        {
            _dbcontext = Dbcontext;


        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using IDbConnection db =
                new SqlConnection(_dbcontext.Database.GetDbConnection().ConnectionString);

            using SqlCommand cmd = new SqlCommand("sp_LoginUser", (SqlConnection)db);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);

            db.Open();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read()) return null;

            return new User
            {
                Id = Guid.Parse(reader["Id"].ToString()),
                Name = reader["Name"].ToString(),
                Email = reader["Email"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                Role = reader["Role"].ToString()
            };
        }
        public async Task RegisterUserAsync(Entities.User user)
        {
             using IDbConnection db =
                new SqlConnection(_dbcontext.Database.GetDbConnection().ConnectionString);

            using SqlCommand cmd = new SqlCommand("sp_RegisterUser", (SqlConnection)db);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", user.Name);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
            cmd.Parameters.AddWithValue("@Role", user.Role);

            db.Open();
            await cmd.ExecuteNonQueryAsync();
        }

    }
}
