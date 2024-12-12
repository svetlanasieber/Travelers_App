using Newtonsoft.Json.Linq;
using RestSharp;
using Travelers;

namespace Travelers_CRUD
{
    [TestFixture]
    public class CRUD_Tests
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

        [Test, Order(1)]
        public void Test_GetAllDestinations()
        {
            var getRequest = new RestRequest("/destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            var destinations = JArray.Parse(getResponse.Content);

            Assert.That(getResponse.IsSuccessful, Is.True);
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty);
            Assert.That(destinations.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(destinations.Count, Is.GreaterThanOrEqualTo(1));

            //assert the type of single property from object in the array =>
            var categoryType = destinations.FirstOrDefault()?["category"]?.Type;
            Assert.That(categoryType, Is.EqualTo(JTokenType.Object));


            foreach (var d in destinations)
            {
                Assert.That(d["_id"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["name"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["location"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["description"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["category"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["attractions"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(d["bestTimeToVisit"].ToString(), Is.Not.Null.Or.Empty);
            }

            var listOfDestinations = destinations.Select(d => d["name"].ToString()).ToList();
            foreach (var item in listOfDestinations)
            {
                Assert.That(listOfDestinations.Contains("Machu Picchu"));
                Assert.That(listOfDestinations.Contains("Rocky Mountains"));
                Assert.That(listOfDestinations.Contains("Maui Beach"));
                Assert.That(listOfDestinations.Contains("New York City"));
                Assert.That(listOfDestinations.Contains("Yellowstone National Park"));
            }

            category = destinations[0]["category"]["_id"]?.ToString();
        }

        [Test, Order(2)]
        public void Test_GetDestinationByName()
        {
            var getRequest = new RestRequest("/destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.IsSuccessStatusCode, Is.True);
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty);

            var destinations = JArray.Parse(getResponse.Content);
            Console.WriteLine(destinations);
            var destination = destinations.FirstOrDefault(d => d["name"].ToString() == "Rocky Mountains");
            var descr = destination["description"].ToString();

            Assert.AreEqual(descr, "A vast mountain range with stunning scenery and hiking trails.");
        }

        [Test, Order(3)]
        public void Test_CreateDestination()
        {
            var randomTitle = $"title_{randomNum}";
            var postRequest = new RestRequest("/destination", Method.Post)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = randomTitle,
                    location = "testLocation",
                    description = "testDesc",
                    attractions = new string[]
                    {
                        "I have",
                        "no idea"
                    },
                    category = category,
                    bestTimeToVisit = "after salary lol"
                });

            var postResponse = client.Execute(postRequest);

            Assert.Multiple(() =>
            {
                Assert.That(postResponse.IsSuccessful, Is.True, $"Request failed with content: {postResponse.Content}");
                Assert.That(postResponse.Content, Is.Not.Null.Or.Empty, "Content shoudnt be null or empty");
                Assert.That(JObject.Parse(postResponse.Content).Type, Is.EqualTo(JTokenType.Object), "response content isn`t object");
            });

            var responseData = JObject.Parse(postResponse.Content);

            string destinationID;
            Assert.Multiple(() =>
            {
                Assert.That(responseData.ContainsKey("_id"), Is.True);
                destinationID = responseData["_id"].ToString();
            });

            Assert.Multiple(() =>
            {
                Assert.That(responseData["name"].ToString(), Is.EqualTo(randomTitle));
                Assert.That(responseData["location"].ToString(), Is.EqualTo("testLocation"));
                Assert.That(responseData["description"].ToString(), Is.EqualTo("testDesc"));
                Assert.That(responseData["attractions"].ToObject<string[]>(), Is.EqualTo(new string[] { "I have", "no idea" }));  // 
                Assert.That(responseData["bestTimeToVisit"].ToString(), Is.EqualTo("after salary lol"));
                Assert.That(responseData["category"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(responseData["category"].Type, Is.EqualTo(JTokenType.Object), "category must be an object in the response"); // assure it returns as object this time
            });

            lastRandom = randomTitle;
        }

        [Test, Order(4)]
        public void Test_UpdateDestination()
        {
            var getRequest = new RestRequest("/destination", Method.Get);
            var getResponse = client.Execute(getRequest);
            var destinations = JArray.Parse(getResponse.Content);

            Assert.True(getResponse.IsSuccessful);

            var destinationToUpdate = destinations.FirstOrDefault(d => d["name"].ToString() == lastRandom);
            Assert.That(destinationToUpdate, Is.Not.Null);

            var destId = destinationToUpdate["_id"].ToString();

            var randomName = $"updated_{randomNum}";

            var putRequest = new RestRequest("/destination/{id}", Method.Put)
                .AddUrlSegment("id", destId)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = randomName,
                    location = "testLocation",
                    description = "testDesc",
                    attractions = new string[]
                    {
                        "I have",
                        "no idea"
                    },
                    category = category,
                    bestTimeToVisit = "after salary for sure"
                });

            var putResponse = client.Execute(putRequest);
            Assert.True(putResponse.IsSuccessful);

            var responseContent = JObject.Parse(putResponse.Content);
            Assert.That(responseContent, Is.Not.Null);

            Assert.That(responseContent["name"].ToString(), Is.EqualTo(randomName));  
            Assert.That(responseContent["bestTimeToVisit"].ToString(), Is.EqualTo("after salary for sure"));

            // Trying to search for the old name anywhere
            // firstly get all elements again
            
            getResponse = client.Execute(getRequest);
            var updatedDestinations = JArray.Parse(getResponse.Content);

            var result = updatedDestinations.FirstOrDefault(dest => dest["name"]?.ToString() == lastRandom);
            // assert that the element is NOT found
            Assert.Null(result);

        }

        [Test, Order(5)]
        public void Test_DeleteDestination()
        {
            var getRequest = new RestRequest("/destination", Method.Get);
            var getResponse = client.Execute(getRequest);
            var destinations = JArray.Parse(getResponse.Content);
            var lastDest = destinations[0];

            var lastDestID = lastDest["_id"].ToString();

            var delRequest = new RestRequest("/destination/{id}", Method.Delete)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddUrlSegment("id", lastDestID);

            var delResponse = client.Execute(delRequest);
            Assert.True(delResponse.IsSuccessful);


            // try to reach the destination now
            var verifyGetRequest = new RestRequest("/destination/{id}", Method.Get)
                .AddUrlSegment("id", lastDestID);
            var verifyResponse = client.Execute(verifyGetRequest);

            Assert.True(verifyResponse.IsSuccessful);
            Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"));
        }
    }
}
