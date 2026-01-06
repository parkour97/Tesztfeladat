using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Interfaces;
using Server.Model;
using Server.Model.Entity;
using Server.Model.View;
using System;

namespace Server.Services
{
    public class UserService : IUserService
    {
        private readonly ServerDataContext context;
        private readonly PasswordHasher<User> passwordHasher;

        public UserService(ServerDataContext context)
        {
            this.context = context;
            passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
        {
            if (await context.Users.AnyAsync(u =>
                u.UserName == request.Username || u.Email == request.Email))
            {
                return ServiceResult.Fail("Username or email address already exists");
            }

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email
            };

            user.Password = passwordHasher.HashPassword(user, request.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return ServiceResult.Ok("Registration successful");
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u =>
                u.UserName == request.UsernameOrEmail ||
                u.Email == request.UsernameOrEmail);

            if (user == null)
                return null;

            var result = passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return null;

            // Itt általában JWT tokent generálunk
            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            return $"SUCCESFUL-LOGIN-{user.UserName}";
        }
    }
}
