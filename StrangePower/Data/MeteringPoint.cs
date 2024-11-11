namespace StrangePower.Data
{
    using System.ComponentModel.DataAnnotations;

    public class MeteringPoint
    {
        public int Id { get; set; }
        public string MeteringpointId { get; set; }
        public string Address { get; set; }
    }
}