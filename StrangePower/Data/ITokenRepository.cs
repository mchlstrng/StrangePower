namespace StrangePower.Data
{
    public interface ITokenRepository
    {
        Task<Token?> GetTokenAsync();
        Task UpdateTokenAsync(Token token);
    }
}