using ApiAutomationSession2._2.Models;
using Microsoft.VisualBasic;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiAutomationSession2._2.Tests
{
    [TestClass]
    public class PetClientTest
    {
        private static RestClient restClient;
        
        private static readonly string BaseUrl = "https://petstore.swagger.io/v2/";
        
        private static readonly string PetEndpoint = "pet";
        private static string GetUrl(string endpoint) => $"{BaseUrl}{endpoint}";
        private static Uri GetURI(string endpoint) => new Uri(GetUrl(endpoint));

        private readonly List<Pet> CleanUpList = new List<Pet>();

        [TestInitialize]
        public void TestInitialize() {
            restClient = new RestClient();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            foreach (var data in CleanUpList)
            {
                var restDeleteRequest = new RestRequest(GetURI($"{PetEndpoint}/{data.Id}"));
                var restDeleteResponse = await restClient.DeleteAsync(restDeleteRequest);
            }
        }

        [TestMethod]
        public async Task PostMethod()
        {
            #region CREATE PET
            List<string> photoUrls = new List<string>();
            photoUrls.Add("www.sample.url.1");

            List<Tag> tags = new List<Tag>();
            tags.Add(new Tag(1, "Cute"));

            Random rnd = new Random();

            Pet newPet = new Pet()
            {
                Id = 50009 + rnd.Next(),
                Category = new Category(1, "Dog"),
                Name = "Rocky",
                PhotoUrls = photoUrls,
                Tags = tags,
                Status = "Available"
            };

            var postRequest = new RestRequest(GetURI(PetEndpoint)).AddJsonBody(newPet);
            var postResponse = await restClient.ExecutePostAsync(postRequest);

            #endregion

            #region CLEAN UP
            CleanUpList.Add(newPet);
            #endregion

            #region GET PET
            var getPetRequest = new RestRequest(GetURI($"{PetEndpoint}/{newPet.Id}"));
            var getPetResponse = await restClient.ExecuteGetAsync<Pet>(getPetRequest);
            #endregion

            #region ASSERTIONS
            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode, "Status Code is not equal to 200");
            Assert.IsTrue(newPet.Name.Equals(getPetResponse.Data?.Name), "Name is not reflected");
            Assert.IsTrue(newPet.Category.Name.Equals(getPetResponse.Data?.Category.Name), "Category Name is not reflected");
            Assert.AreEqual(newPet.PhotoUrls.ToList().Count(), getPetResponse.Data?.PhotoUrls.ToList().Count(), "PhotoUrls are not added successfully");
            Assert.AreEqual(newPet.Tags.ToList().Count(), getPetResponse.Data?.Tags.ToList().Count(), "Tags are not addeed successfully");
            Assert.IsTrue(newPet.Status.Equals(getPetResponse.Data?.Status), "Status is not added successfully");

            for (int ctr = 0; ctr < getPetResponse.Data?.PhotoUrls.Count; ctr++)
                Assert.IsTrue(newPet.PhotoUrls[ctr].Equals(getPetResponse.Data?.PhotoUrls[ctr]), "PhotoUrl value is not added successfully");

            for (int ctr = 0; ctr < newPet.Tags.Count; ctr++)
                Assert.IsTrue(newPet.Tags[ctr].Name.Equals(getPetResponse.Data?.Tags[ctr].Name), "Tag name is not added successfully");

            #endregion
        }

    }
}
