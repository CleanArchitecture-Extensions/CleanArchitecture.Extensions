using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantMigrationRunnerTests
{
    [Fact]
    public async Task RunAsync_runs_migrations_for_each_tenant()
    {
        var accessor = new CurrentTenantAccessor();
        await using var factory = new SqliteTenantDbContextFactory(accessor);
        var runner = new TenantMigrationRunner<MigrationDbContext>(accessor, factory);

        var tenants = new[]
        {
            new TenantInfo("alpha"),
            new TenantInfo("beta")
        };

        await runner.RunAsync(tenants);

        Assert.Equal(new[] { "alpha", "beta" }, factory.CreatedTenantIds);

        foreach (var connection in factory.Connections)
        {
            using var historyCommand = connection.CreateCommand();
            historyCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            var historyTable = historyCommand.ExecuteScalar()?.ToString();

            using var widgetCommand = connection.CreateCommand();
            widgetCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='MigrationWidgets';";
            var widgetTable = widgetCommand.ExecuteScalar()?.ToString();

            Assert.Equal("__EFMigrationsHistory", historyTable);
            Assert.Equal("MigrationWidgets", widgetTable);
        }
    }

    private sealed class SqliteTenantDbContextFactory : ITenantDbContextFactory<MigrationDbContext>, IAsyncDisposable
    {
        private readonly CurrentTenantAccessor _accessor;
        private readonly List<SqliteConnection> _connections = new();
        private readonly List<string?> _createdTenantIds = new();

        public SqliteTenantDbContextFactory(CurrentTenantAccessor accessor)
        {
            _accessor = accessor;
        }

        public IReadOnlyList<SqliteConnection> Connections => _connections;

        public IReadOnlyList<string?> CreatedTenantIds => _createdTenantIds;

        public MigrationDbContext CreateDbContext()
        {
            return CreateContext();
        }

        public Task<MigrationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateContext());
        }

        public ValueTask DisposeAsync()
        {
            foreach (var connection in _connections)
            {
                connection.Dispose();
            }

            _connections.Clear();
            return ValueTask.CompletedTask;
        }

        private MigrationDbContext CreateContext()
        {
            _createdTenantIds.Add(_accessor.TenantId);

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            _connections.Add(connection);

            var options = new DbContextOptionsBuilder<MigrationDbContext>()
                .UseSqlite(connection)
                .Options;

            return new MigrationDbContext(options);
        }
    }

    private sealed class MigrationDbContext : DbContext
    {
        public MigrationDbContext(DbContextOptions<MigrationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MigrationWidget> MigrationWidgets => Set<MigrationWidget>();
    }

    private sealed class MigrationWidget
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [DbContext(typeof(MigrationDbContext))]
    [Migration("0001_Initial")]
    private sealed class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigrationWidgets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationWidgets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MigrationWidgets");
        }
    }
}
