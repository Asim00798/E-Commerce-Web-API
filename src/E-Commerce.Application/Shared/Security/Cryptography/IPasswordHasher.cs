namespace E_Commerce.Application.Shared.Security.Cryptography;

public interface IPasswordHasher
{
    string HashPassword(string plainPassword);
}