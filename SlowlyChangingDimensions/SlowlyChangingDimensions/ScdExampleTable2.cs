using System.ComponentModel.DataAnnotations;

namespace SlowlyChangingDimensions
{
    public class ScdExampleTable2
    {
        public int Id { get; set; }
        public int CorrelationId { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; } = DateTime.MaxValue;
        public bool IsCurrentVersion { get; set; } = true;

        [MaxLength(100)]
        public string Data1 { get; set; }
        [MaxLength(200)]
        public string Data2 { get; set; }
        [MaxLength(200)]
        public string Data3 { get; set; }

        public ScdExampleTable2 NewVersion(DateTime now)
        {
            var nextVersion = new ScdExampleTable2
            {
                CreatedTimestamp = now,
                EndTimestamp = DateTime.MaxValue,
                Data1 = Data1,
                Data2 = Data2,
                Data3 = Data3,
                CorrelationId = CorrelationId,
                IsCurrentVersion = true
            };
            EndTimestamp = now;
            IsCurrentVersion = false;
            return nextVersion;
        }
    }
}
