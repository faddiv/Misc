// See https://aka.ms/new-console-template for more information
using NBomber.CSharp;
using NBomber.Http.CSharp;

Console.WriteLine("Hello, World!");

using var httpClient = new HttpClient();
var duration = TimeSpan.FromMinutes(10);

var scenario1 = Scenario.Create("sub-classing/validated", async context =>
{
    var request =
        Http.CreateRequest("POST", "https://localhost:5001/api/sub-classing/validated")
            .WithHeader("Accept", "application/json")
            .WithBody(new StringContent("{ \"id\": \"string\" }", System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json")));

    var response = await Http.Send(httpClient, request);

    return response;
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5))
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 10,
                      during: duration)
);

var scenario2 = Scenario.Create("mediatr/validated", async context =>
{
    var request =
        Http.CreateRequest("POST", "https://localhost:5001/api/mediatr/validated")
            .WithHeader("Accept", "application/json")
            .WithBody(new StringContent("{ \"id\": \"string\" }", System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json")));

    var response = await Http.Send(httpClient, request);

    return response;
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5))
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 10,
                      during: duration)
);

NBomberRunner
    .RegisterScenarios(scenario1)
    .Run();

NBomberRunner
    .RegisterScenarios(scenario2)
    .Run();
