namespace Customers.Api.Tests.Integration.CustomerController;

//[Collection("CustomerApi Collection")]
public class GetCustomerControllerTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly HttpClient _httpClient;

    public GetCustomerControllerTests(WebApplicationFactory<IApiMarker> appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Act
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");

        // response as json
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        problem!.Title.Should().Be("Not Found");
        problem.Status.Should().Be(404);
    }
}
