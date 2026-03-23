using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Auditing;
using ClinicAdmin.Contracts.Auditing;
using ClinicAdmin.Desktop.ViewModels;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class AuditLogViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenQueryReturnsEntries_ShouldPopulateAuditItems()
    {
        var viewModel = new AuditLogViewModel(
            new FakeAuditLogQueryService(new[]
            {
                new AuditLogItemDto(
                    Guid.NewGuid(),
                    new DateTimeOffset(2026, 3, 23, 8, 0, 0, TimeSpan.Zero),
                    "ADMIN",
                    "PatientRegistered",
                    "Patient",
                    Guid.NewGuid(),
                    "Created patient",
                    null,
                    "P-100",
                    null,
                    "WS-1",
                    true)
            }),
            new FakeFacilityContext());

        await viewModel.InitializeAsync();

        Assert.Single(viewModel.AuditItems);
        Assert.Contains("loaded", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_WhenQueryFails_ShouldShowFriendlyMessage()
    {
        var viewModel = new AuditLogViewModel(new ThrowingAuditLogQueryService(), new FakeFacilityContext());

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.AuditItems);
        Assert.Contains("could not be loaded", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeAuditLogQueryService : IAuditLogQueryService
    {
        private readonly IReadOnlyCollection<AuditLogItemDto> _items;

        public FakeAuditLogQueryService(IReadOnlyCollection<AuditLogItemDto> items)
        {
            _items = items;
        }

        public Task<IReadOnlyCollection<AuditLogItemDto>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items);
    }

    private sealed class ThrowingAuditLogQueryService : IAuditLogQueryService
    {
        public Task<IReadOnlyCollection<AuditLogItemDto>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Boom");
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
