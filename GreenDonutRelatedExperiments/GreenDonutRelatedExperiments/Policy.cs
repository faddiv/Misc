using Microsoft.Extensions.ObjectPool;

namespace GreenDonutRelatedExperiments;

internal class Policy
    : IPooledObjectPolicy<HelperContainer>
{
    public HelperContainer Create()
    {
        return new HelperContainer();
    }

    public bool Return(HelperContainer obj)
    {
        obj.Values = null;
        obj.Payload = null;
        return true;
    }
}
