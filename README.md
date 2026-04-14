# TUnit Sample Project

[![Build](https://github.com/tavanuka/TUnit.Sample/actions/workflows/build.yml/badge.svg)](https://github.com/tavanuka/TUnit.Sample/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/tavanuka/TUnit.Sample/graph/badge.svg?token=S2NDOG3IEJ)](https://codecov.io/gh/tavanuka/TUnit.Sample)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=tavanuka_TUnit.Sample&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=tavanuka_TUnit.Sample)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=tavanuka_TUnit.Sample&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=tavanuka_TUnit.Sample)

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
- CI/CD pipeline design for TUnit with Microsoft.Testing.Platform

## Project Structure

The solution demonstrates a typical .NET application architecture with comprehensive testing:

**Application**
- **TUnit.Sample.Domain** - Core domain models and entities
- **TUnit.Sample.Infrastructure** - Data access layer with Entity Framework Core
- **TUnit.Sample.ApiService** - ASP.NET Core Web API with .NET Aspire integration
- **TUnit.Sample.Web** - Blazor frontend
- **TUnit.Sample.AppHost** - .NET Aspire orchestration host
- **TUnit.Sample.ServiceDefaults** - Shared Aspire service defaults
- **TUnit.Sample.Common** - Shared constants and utilities

**Tests**
- **TUnit.Sample.ApiService.Tests** - Unit tests `[Category=UnitTest]`
- **TUnit.Sample.Web.Tests** - Blazor component tests `[Category=Components]`
- **TUnit.Sample.ApiService.IntegrationTests** - API integration tests `[Category=Integration]`
- **TUnit.Sample.AppHost.IntegrationTests** - Aspire end-to-end tests `[Category=AppHost]`

## Technology Stack

- .NET 10.0
- TUnit testing framework (Microsoft.Testing.Platform)
- ASP.NET Core Web API
- Blazor
- .NET Aspire for cloud-native orchestration
- Entity Framework Core 10.0
- PostgreSQL with Testcontainers
- Npgsql provider

## CI/CD Pipeline

The project includes a GitHub Actions pipeline that demonstrates running TUnit tests via **Microsoft.Testing.Platform** in CI, with parallel test jobs by category, code coverage, and test result reporting.

### Build & Test (`build.yml`)

```
build ──┬── test (UnitTest)
        ├── component-test (Components)
        ├── integration-test (Integration)
        └── aspire-test (AppHost)
              └── report (Codecov coverage + test results)
```

Each test job filters tests by category using TUnit's `--treenode-filter`:

```shell
dotnet test -- --coverage --report-trx \
  --treenode-filter "/*/*/*/*[Category=UnitTest]" \
  --coverage-output-format cobertura \
  --ignore-exit-code 8
```

`--ignore-exit-code 8` handles Microsoft.Testing.Platform's "zero tests ran" exit code when a category filter matches no tests in a given project.

Coverage reports (Cobertura XML) and TRX test results are uploaded as artifacts and consolidated in the `report` job, which sends them to [Codecov](https://codecov.io).

### Code Inspection (`code-inspection.yml`)

*Note: this step is currently disabled due to community edition not supporting the project frameworks present in this repository.*
*SonarQube Cloud is available as an alternative.*

Runs [Qodana Community for .NET](https://www.jetbrains.com/qodana/) static analysis on every PR and push to `master`.

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
- Docker (for Testcontainers PostgreSQL and Aspire tests)

### Execute Tests

```shell
dotnet test
```

To run a specific category locally:

```shell
dotnet test -- --treenode-filter "/*/*/*/*[Category=UnitTest]"
```

TUnit executes tests in parallel by default, leveraging its source-generated test infrastructure for optimal performance.

## Configuration

The project uses .NET Aspire for service orchestration and configuration. Integration tests override the production DbContext registration to use Testcontainers-provided PostgreSQL instances.

### Test Configuration Highlights

- Connection string override via in-memory configuration
- DbContext pooling removed in favour of scoped instances for tests
- Schema-specific model caching with `SchemaModelCacheKeyFactory`
- Shared test session resources via `SharedType.PerTestSession`

## Article Context

This sample project demonstrates practical TUnit implementation patterns discussed in the accompanying Devware article. The article explores whether TUnit's architectural advantages—source generation, AOT compilation, and native parallelism—position it as a compelling alternative to established frameworks like xUnit, NUnit, and MSTest.

### Questions Addressed

- How does TUnit's performance compare in real-world scenarios?
- What developer experience improvements does source generation enable?
- Can TUnit handle complex integration testing requirements?
- Is the framework mature enough for production adoption?
- How do you build a CI pipeline around Microsoft.Testing.Platform?

## License

This project is provided as a reference implementation for educational purposes accompanying the Devware article on TUnit.

## Further Reading

- [TUnit Official Documentation](https://github.com/thomhurst/TUnit)
- [Microsoft.Testing.Platform Documentation](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)