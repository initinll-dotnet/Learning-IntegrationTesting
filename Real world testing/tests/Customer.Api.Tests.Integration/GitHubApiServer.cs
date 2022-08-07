using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Customer.Api.Tests.Integration;

public class GitHubApiServer : IDisposable
{
    private WireMockServer _server;

    public string Url => _server.Url!;

    public void Start()
    {
        _server = WireMockServer.Start();
    }

    public void SetupUser(string username)
    {
        string body = GenerateGithubUserResponseBody(username);

        _server
            .Given(Request
                .Create()
                .WithPath($"/users/{username}")
                .UsingGet())
            .RespondWith(Response
                .Create()
                .WithBody(body)
                .WithHeader("content-type", "application/json; charset=utf-8")
                .WithStatusCode(200));
    }

    public void SetupThrottledUser(string username)
    {
        _server
            .Given(Request
                .Create()
                .WithPath($"/users/{username}")
                .UsingGet())
            .RespondWith(Response
                .Create()
                //.WithBody(body)
                .WithHeader("content-type", "application/json; charset=utf-8")
                .WithStatusCode(403));
    }

    static string GenerateGithubUserResponseBody(string username)
    {
        return $@"{{'login': '{username}',
'id': 37251096,
'node_id': 'MDQ6VXNlcjM3MjUxMDk2',
'avatar_url': 'https://avatars.githubusercontent.com/u/37251096?v=4',
'gravatar_id': '',
'url': 'https://api.github.com/users/{username}',
'html_url': 'https://github.com/{username}',
'followers_url': 'https://api.github.com/users/{username}/followers',
'following_url': 'https://api.github.com/users/{username}/following{{/other_user}}',
'gists_url': 'https://api.github.com/users/{username}/gists{{/gist_id}}',
'starred_url': 'https://api.github.com/users/{username}/starred{{/owner}}{{/repo}}',
'subscriptions_url': 'https://api.github.com/users/{username}/subscriptions',
'organizations_url': 'https://api.github.com/users/{username}/orgs',
'repos_url': 'https://api.github.com/users/{username}/repos',
'events_url': 'https://api.github.com/users/{username}/events{{/privacy}}',
'received_events_url': 'https://api.github.com/users/{username}/received_events',
'type': 'User',
'site_admin': false,
'name': 'Nitin Londhe',
'company': null,
'blog': '',
'location': null,
'email': null,
'hireable': null,
'bio': null,
'twitter_username': '{username}',
'public_repos': 1,
'public_gists': 0,
'followers': 0,
'following': 0,
'created_at': '2018-03-10T19:49:54Z',
'updated_at': '2022-07-27T12:57:16Z'
}}";
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
