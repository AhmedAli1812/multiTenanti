namespace HMS.Application.Abstractions.Security;
public interface IRefreshTokenHasher
{
    string Hash(string token);
    bool Verify(string token, string hash);
}