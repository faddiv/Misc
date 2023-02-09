namespace ServiceBusBasedDotNet.Web.MessageContracts;

public class LongShotJob
{
    public int Seconds { get; set; }
    public bool ShouldFail { get; set; }
}
