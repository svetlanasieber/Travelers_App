using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category

            // Step 2: Get all categories

            // Step 3: Get category by ID

            // Step 4: Edit the category

            // Step 5: Verify Edit

            // Step 6: Delete the category

            // Step 7: Verify that the deleted category cannot be found
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
