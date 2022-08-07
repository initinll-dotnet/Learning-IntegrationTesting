using Bogus;

using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;

using FluentAssertions;

using System.Net;
using System.Net.Http.Json;

using Xunit;

namespace Customer.Api.Tests.Integration.CustomerController;

public class GetCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _httpClient;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public GetCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _httpClient.GetAsync($"customers/{createdCustomer!.Id}");
        var retrievedCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        // Assert
        retrievedCustomer.Should().BeEquivalentTo(createdCustomer);
        response.StatusCode.Should().Be(HttpStatusCode.OK);    
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExists()
    {
        // Act
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
