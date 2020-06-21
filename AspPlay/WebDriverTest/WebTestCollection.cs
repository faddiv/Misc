using Xunit;

namespace WebDriverTest
{
    [CollectionDefinition("WebTest")]
    public class WebTestCollection : ICollectionFixture<WebTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
