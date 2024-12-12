using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class DestinationTests : IDisposable
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
        public void Test_GetAllDestinations()
        {
        }

        [Test]
        public void Test_GetDestinationByName()
        {
        }

        [Test]
        public void Test_AddDestination()
        {
        }

        [Test]
        public void Test_UpdateDestination()
        {
        }

        [Test]
        public void Test_DeleteDestination()
        {
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
