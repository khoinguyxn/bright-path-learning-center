using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

using BrightPathLearningCenter.Api.Contracts;

namespace BrightPathLearningCenter.Api.Tests.Endpoints;

public sealed class CreateLessonEndpointTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly SchedulingApiFactory _factory = new();

    public CreateLessonEndpointTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_WhenSlotIsFree_ReturnsCreatedWithGeneratedIdAndCentreOffset()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request("New Kid", "T2", "R6", "2026-03-11T09:00:00+07:00"),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        LessonResponse? lesson = await response.Content.ReadFromJsonAsync<LessonResponse>(cancellationToken);
        Assert.NotNull(lesson);
        Assert.Equal("L035", lesson!.Id);
        Assert.Equal("booked", lesson.Status);
        Assert.Equal(TimeSpan.FromHours(7), lesson.StartsAt.Offset);
        Assert.Equal(TimeSpan.FromHours(7), lesson.EndsAt.Offset);
    }

    [Theory]
    [InlineData("New Kid", "T1", "R6", "2026-03-04T11:00:00+07:00", "tutor")]
    [InlineData("Le Minh Chau", "T1", "R1", "2026-03-04T09:00:00+07:00", "student")]
    [InlineData("New Kid", "T3", "R1", "2026-03-07T15:00:00+07:00", "room")]
    public async Task Create_WhenResourceClashes_ReturnsConflictOnThatResource(
        string student,
        string tutorId,
        string roomId,
        string startsAt,
        string expectedType)
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request(student, tutorId, roomId, startsAt),
            cancellationToken);

        await AssertConflictAsync(response, expectedType, cancellationToken);
    }

    [Fact]
    public async Task Create_WhenBackToBack_ReturnsCreated()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request("New Kid", "T2", "R2", "2026-03-03T10:00:00+07:00"),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenExistingSlotIsCancelled_ReturnsCreated()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request("New Kid", "T2", "R2", "2026-03-03T14:00:00+07:00"),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenExistingSlotIsNoShow_ReturnsConflict()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request("New Kid", "T2", "R2", "2026-03-05T13:00:00+07:00"),
            cancellationToken);

        await AssertConflictAsync(response, "tutor", cancellationToken);
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsValidationProblem()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/lessons",
            Request(string.Empty, "T2", "R2", "2026-03-11T09:00:00+07:00", 0),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateLessonRequest Request(
        string student,
        string tutorId,
        string roomId,
        string startsAt,
        int durationMinutes = 60)
    {
        return new CreateLessonRequest
        {
            Student = student,
            TutorId = tutorId,
            RoomId = roomId,
            StartsAt = DateTimeOffset.Parse(startsAt, CultureInfo.InvariantCulture),
            DurationMinutes = durationMinutes
        };
    }

    private static async Task AssertConflictAsync(
        HttpResponseMessage response,
        string expectedType,
        CancellationToken cancellationToken)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        Assert.NotNull(body);

        JsonNode? conflicts = body!["conflicts"];
        Assert.NotNull(conflicts);

        Assert.Contains(
            conflicts!.AsArray(),
            conflict => conflict!["types"]!.AsArray().Any(type => type!.GetValue<string>() == expectedType));
    }
}
