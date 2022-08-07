
using Bogus;

using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using System.Net;
using System.Net.Http.Json;

using Xunit;

namespace Customer.Api.Tests.Integration.CustomerController;

public class UpdateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _httpClient;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);


    public UpdateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Update_UpdateUser_WhenDataIsValid()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        customer = _customerGenerator.Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ReturnsValidationError_WhenEmailIsInvalid()
    {
        // Arrange        
        var customer = _customerGenerator.Generate();
        var createdResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        const string invalidEmail = "abc";

        customer = _customerGenerator
            .Clone()
            .RuleFor(x => x.Email, invalidEmail)
            .Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error!.Title.Should().Be("One or more validation errors occurred.");
        error!.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    }

    [Fact]
    public async Task Update_ReturnsValidationError_WhenGithubUserDoesNotExist()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        const string invalidGithubUser = "abc";

        customer = _customerGenerator
            .Clone()
            .RuleFor(x => x.GitHubUsername, invalidGithubUser)
            .Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error!.Title.Should().Be("One or more validation errors occurred.");
        error!.Errors["GitHubUsername"][0].Should().Be($"There is no GitHub user with username {invalidGithubUser}");
    }
}
