public interface IPasswordHasher
{
    string HashPassword(string password);
    bool Verify(string password, string hash);
}