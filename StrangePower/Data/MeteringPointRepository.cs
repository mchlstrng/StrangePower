namespace StrangePower.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public interface IMeteringPointRepository
    {
        Task SaveMeteringPointAsync(MeteringPoint meteringPoint);
        Task<List<MeteringPoint>> GetMeteringPointsAsync();
        Task<MeteringPoint?> GetActiveMeteringPointAsync();
        Task SetActiveMeteringPointAsync(MeteringPoint meteringPoint);
    }


    public class MeteringPointRepository : IMeteringPointRepository
    {
        private readonly TokenDbContext _context;

        public MeteringPointRepository(TokenDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task SaveMeteringPointAsync(MeteringPoint meteringPoint)
        {
            _context.MeteringPoints.Add(meteringPoint);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MeteringPoint>> GetMeteringPointsAsync()
        {
            return await _context.MeteringPoints.ToListAsync();
        }

        public async Task<MeteringPoint?> GetActiveMeteringPointAsync()
        {
            return await _context.MeteringPoints.FirstOrDefaultAsync();
        }

        public async Task SetActiveMeteringPointAsync(MeteringPoint meteringPoint)
        {
            var existingActiveMeteringPoint = await _context.MeteringPoints.FirstOrDefaultAsync();
            if (existingActiveMeteringPoint != null)
            {
                _context.MeteringPoints.Remove(existingActiveMeteringPoint);
            }

            _context.MeteringPoints.Add(meteringPoint);
            await _context.SaveChangesAsync();
        }
    }
}