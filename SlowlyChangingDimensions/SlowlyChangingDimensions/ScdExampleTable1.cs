using System.ComponentModel.DataAnnotations;

namespace SlowlyChangingDimensions
{
    public class ScdExampleTable1
    {
        public int Id { get; set; }
        public int CorrelationId { get; set; }
        [MaxLength(100)]
        public string Data1 { get; set; }
        [MaxLength(200)]
        public string Data2 { get; set; }
        [MaxLength(200)]
        public string Data3 { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public int? PreviousVersionId { get; set; }
        public ScdExampleTable1 PreviousVersion { get; set; }
        public ScdExampleTable1 NextVersion { get; set; }

        public ScdExampleTable1 NewVersion(DateTime now)
        {
            var nextVersion = new ScdExampleTable1
            {
                CreatedTimestamp = now,
                Data1 = Data1,
                Data2 = Data2,
                Data3 = Data3,
                PreviousVersion = this,
                PreviousVersionId = Id,
                CorrelationId = CorrelationId
            };
            //NextVersion = nextVersion;
            return nextVersion;
        }
    }
}
