# TUnit Sample Project

This repository contains a sample .NET application demonstrating integration testing with **TUnit**, a modern testing framework for .NET that leverages source generation, ahead-of-time compilation, and built-in parallelism to deliver high-performance test execution.

## Purpose

This project accompanies a Devware article exploring TUnit as a next-generation testing framework for .NET. The article examines whether TUnit represents merely another framework or a promising alternative that addresses the evolving needs of modern .NET development.

### Key Topics Covered

- Introduction to TUnit and its core architecture
- Source generation and AOT compilation benefits
- Parallel test execution capabilities
- Integration testing for ASP.NET Core Web APIs
- Aspire integration with pooled DbContext scenarios
- Per-test database isolation using PostgreSQL schemas
- Performance benchmarks comparing TUnit with established frameworks
- Developer experience and test writability assessment

## Project Structure

The solution demonstrates a typical .NET application architecture with comprehensive integration testing:

- **TUnit.Sample.Domain** - Core domain models and entities
- **TUnit.Sample.Infrastructure** - Data access layer with Entity Framework Core
- **TUnit.Sample.ApiService** - ASP.NET Core Web API with .NET Aspire integration
- **TUnit.Sample.ApiService.IntegrationTests** - TUnit-based integration tests
- **TUnit.Sample.Common** - Shared constants and utilities

## Technology Stack

- .NET 10.0
- TUnit testing framework
- ASP.NET Core Web API
- .NET Aspire for cloud-native orchestration
- Entity Framework Core 10.0
- PostgreSQL with Testcontainers
- Npgsql provider

## Key Features Demonstrated

### Integration Testing with TUnit

The project showcases advanced integration testing patterns:

- **WebApplicationFactory** override for test-specific service configuration
- **Testcontainers** for PostgreSQL database provisioning
- **Per-test schema isolation** ensuring clean test execution without database resets
- **Pooled DbContext** replacement for integration test scenarios
- **Parallel test execution** with shared test session resources

### Database Testing Strategy

Each test receives its own PostgreSQL schema, enabling:

- True test isolation without full database cleanup
- Parallel test execution without conflicts
- Realistic integration testing with actual database operations
- Schema-based model cache key factories for EF Core

### API Endpoint Coverage

Integration tests cover CRUD operations for domain entities:

- GET collection and single entity endpoints
- POST with entity creation validation
- PUT for entity updates
- DELETE with subsequent verification
- HTTP status code assertions
- JSON response validation

## Running the Tests

### Prerequisites

- .NET 10.0 SDK
- Docker (for Testcontainers PostgreSQL)

### Execute Tests

```shell script
dotnet test
```


TUnit will execute tests in parallel by default, leveraging its source-generated test infrastructure for optimal performance.

## Configuration

The project uses .NET Aspire for service orchestration and configuration. Integration tests override the production DbContext registration to use Testcontainers-provided PostgreSQL instances.

### Test Configuration Highlights

- Connection string override via in-memory configuration
- DbContext pooling removed in favor of scoped instances for tests
- Schema-specific model caching with `SchemaModelCacheKeyFactory`
- Shared test session resources via `SharedType.PerTestSession`

## Article Context

This sample project demonstrates practical TUnit implementation patterns discussed in the accompanying Devware article. The article explores whether TUnit's architectural advantages—source generation, AOT compilation, and native parallelism—position it as a compelling alternative to established frameworks like xUnit, NUnit, and MSTest.

### Questions Addressed

- How does TUnit's performance compare in real-world scenarios?
- What developer experience improvements does source generation enable?
- Can TUnit handle complex integration testing requirements?
- Is the framework mature enough for production adoption?

## License

This project is provided as a reference implementation for educational purposes accompanying the Devware article on TUnit.

## Further Reading

- [TUnit Official Documentation](https://github.com/thomhurst/TUnit)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)