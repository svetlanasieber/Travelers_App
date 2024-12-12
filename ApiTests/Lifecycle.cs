using Newtonsoft.Json.Linq;
using RestSharp;
using Travelers;

namespace Travelers_CategoryManagement
{
    [TestFixture]
    public class Lifecycle
    {
        private RestClient client;
        private string token;
        private int randomNum;
        private string lastRandom;
        private string category;

        [SetUp]
        public void SetUp()
        {
            client = new RestClient(GlobalConstants.baseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            var random = new Random();
            randomNum = random.Next(10, 50000);
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }


        [Test]
        public void Test_CategoryLifecycle()
        {
            //[1]: create new category
            var randomCategoryName = $"name_{randomNum}";
            var postRequest = new RestRequest("/category", Method.Post)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = randomCategoryName
                });

            var postResponse = client.Execute(postRequest);
            Assert.True(postResponse.IsSuccessful);

            var jsonResponse = JObject.Parse(postResponse.Content);
            Assert.That(jsonResponse["_id"].ToString(), Is.Not.Null.Or.Empty);
            var myLastAddedCategoryID = jsonResponse["_id"].ToString();







            //[2]: get all categories
            var getRequest = new RestRequest("/category", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.True(getResponse.IsSuccessful);
            var responseType = JArray.Parse(getResponse.Content).Type;
            Assert.That(responseType, Is.EqualTo(JTokenType.Array));      
            
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty); //
            Assert.That(JArray.Parse(getResponse.Content).Count, Is.GreaterThanOrEqualTo(1)); // we expect >=1 categories count  







            //[3]: Get category by ID
            var getRequestByID = new RestRequest("category/{id}", Method.Get)
                .AddUrlSegment("id", myLastAddedCategoryID);

            var getResponseByID = client.Execute(getRequestByID);

            Assert.True(getResponseByID.IsSuccessful);
            Assert.That(getResponseByID.Content, Is.Not.Empty);

            var getByIDjsonResponse = JObject.Parse(getResponseByID.Content);
            var IDfromGetByIDResponse = getByIDjsonResponse["_id"].ToString();
            var categoryNamefromGetByIDResponse = getByIDjsonResponse["name"].ToString();

            Assert.AreEqual(IDfromGetByIDResponse, myLastAddedCategoryID);    // we assert there is NO mismatch between the ids
            Assert.AreEqual(categoryNamefromGetByIDResponse, randomCategoryName);  // we verify the name of the category is same aswell







            //[4]: Edit the category
            var updatedName = $"updatedName_{randomNum}";
            var putRequest = new RestRequest("category/{id}", Method.Put)
                .AddUrlSegment("id", myLastAddedCategoryID)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = updatedName
                });

            var putResponse = client.Execute(putRequest);       
            Assert.True(putResponse.IsSuccessful);







            //[5]: Verification (after edit):
            var verifyUpdateRequest = new RestRequest("category/{id}", Method.Get)
                .AddUrlSegment("id", myLastAddedCategoryID);
            var verifyUpdateResponse = client.Execute(verifyUpdateRequest);

            Assert.IsTrue(verifyUpdateResponse.IsSuccessful);
            Assert.That(verifyUpdateResponse.Content, Is.Not.Empty);
            var responseCategoryName = JObject.Parse(verifyUpdateResponse.Content)["name"].ToString();
            Assert.AreEqual(responseCategoryName, updatedName);






            //[6]: Delete the category
            var deleteRequest = new RestRequest("/category/{id}", Method.Delete)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddUrlSegment("id", myLastAddedCategoryID);

            var deleteResponse = client.Execute(deleteRequest);
            Assert.True(deleteResponse.IsSuccessful);






            //[7]: verify the deleted category cannot be found
            var verifyDeletionRequest = new RestRequest("/category/{id}", Method.Get)
                .AddUrlSegment("id", myLastAddedCategoryID);
            var verifyDeletionResponse = client.Execute(verifyDeletionRequest);

            Assert.True(verifyDeletionResponse.IsSuccessful);
            Assert.That(verifyDeletionResponse.Content, Is.EqualTo("null"));
        }
    }
}
