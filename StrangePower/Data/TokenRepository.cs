using Microsoft.EntityFrameworkCore;

namespace StrangePower.Data
{
    public class TokenRepository(TokenDbContext context) : ITokenRepository
    {
        private readonly TokenDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Token?> GetTokenAsync()
        {
            return await _context.Tokens.OrderByDescending(t => t.Expiry).FirstOrDefaultAsync();
        }

        public async Task UpdateTokenAsync(Token token)
        {
            var existingToken = await _context.Tokens.FirstOrDefaultAsync();
            if (existingToken != null)
            {
                existingToken.AccessToken = token.AccessToken;
                existingToken.Expiry = token.Expiry;
                _context.Tokens.Update(existingToken);
            }
            else
            {
                _context.Tokens.Add(token);
            }

            await _context.SaveChangesAsync();
        }
    }
}