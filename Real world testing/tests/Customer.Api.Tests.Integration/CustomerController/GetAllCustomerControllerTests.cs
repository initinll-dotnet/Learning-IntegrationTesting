
using Bogus;

using Customers.Api.Contracts.Requests;

using System.Net.Http.Json;
using System.Net;

using Xunit;
using Customers.Api.Contracts.Responses;
using FluentAssertions;

namespace Customer.Api.Tests.Integration.CustomerController
{
    public class GetAllCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _httpClient;

        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public GetAllCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _httpClient = apiFactory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsAllCustomers_WhenCustomerExist()
        {
            // Arrange
            var customer = _customerGenerator.Generate();
            var createdResponse = await _httpClient.PostAsJsonAsync("customers", customer);
            var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

            // Act
            var response = await _httpClient.GetAsync("customers");
            var customersResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            customersResponse!.Customers.Single().Should().BeEquivalentTo(createdCustomer);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyResult_WhenNoCustomerExist()
        {
            // Act
            var response = await _httpClient.GetAsync("customers");
            var customersResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            customersResponse!.Customers.Should().BeEmpty();
        }
    }
}
