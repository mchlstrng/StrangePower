using StrangePower.Data;

namespace StrangePower.Services
{
    public interface IMeteringPointsService
    {
        Task<List<MeteringPoint>> GetMeteringPointsAsync();
        Task SaveMeteringPointAsync(MeteringPoint meteringPoint);
    }

    public class MeteringPointsService : IMeteringPointsService
    {
        private readonly IHttpClientTokenService _httpClientTokenService;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public MeteringPointsService(IHttpClientTokenService httpClientTokenService, IMeteringPointRepository meteringPointRepository)
        {
            _httpClientTokenService = httpClientTokenService;
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task<List<MeteringPoint>> GetMeteringPointsAsync()
        {
            var httpClient = await _httpClientTokenService.GetHttpClientWithTokenAsync();
            var response = await httpClient.GetFromJsonAsync<MeteringPointsResponse>("customerapi/api/meteringpoints/meteringpoints?includeAll=false");

            if (response == null)
            {
                return new List<MeteringPoint>();
            }

            var meteringPoints = response.result.Select(result => new MeteringPoint
            {
                MeteringpointId = result.meteringPointId,
                Address = $"{result.streetName} {result.buildingNumber}, {result.cityName} {result.postcode}"
            }).ToList();

            return meteringPoints;
        }

        public async Task SaveMeteringPointAsync(MeteringPoint meteringPoint)
        {
            await _meteringPointRepository.SaveMeteringPointAsync(meteringPoint);
        }
    }
}


public class MeteringPointsResponse
{
    public Result[] result { get; set; }
}

public class Result
{
    public string streetCode { get; set; }
    public string streetName { get; set; }
    public string buildingNumber { get; set; }
    public object floorId { get; set; }
    public object roomId { get; set; }
    public string citySubDivisionName { get; set; }
    public string municipalityCode { get; set; }
    public object locationDescription { get; set; }
    public string settlementMethod { get; set; }
    public string meterReadingOccurrence { get; set; }
    public string firstConsumerPartyName { get; set; }
    public object secondConsumerPartyName { get; set; }
    public string meterNumber { get; set; }
    public DateTime consumerStartDate { get; set; }
    public string meteringPointId { get; set; }
    public string typeOfMP { get; set; }
    public string balanceSupplierName { get; set; }
    public string postcode { get; set; }
    public string cityName { get; set; }
    public bool hasRelation { get; set; }
    public object consumerCVR { get; set; }
    public object dataAccessCVR { get; set; }
    public Childmeteringpoint[] childMeteringPoints { get; set; }
}

public class Childmeteringpoint
{
    public string parentMeteringPointId { get; set; }
    public string meteringPointId { get; set; }
    public string typeOfMP { get; set; }
    public string meterReadingOccurrence { get; set; }
    public object meterNumber { get; set; }
}
