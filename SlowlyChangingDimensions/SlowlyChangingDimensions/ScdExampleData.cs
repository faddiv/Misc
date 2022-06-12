using System.ComponentModel.DataAnnotations;

namespace SlowlyChangingDimensions
{
    public class ScdExampleData
    {
        public int Id { get; set; }
        public int CorrelationId { get; set; }
        [MaxLength(100)]
        public string Data1 { get; set; }
        [MaxLength(200)]
        public string Data2 { get; set; }
        [MaxLength(200)]
        public string Data3 { get; set; }
    }
}

