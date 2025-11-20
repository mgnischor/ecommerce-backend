# Contributing Guide

Welcome to the **E-Commerce Backend API** project! This document describes how you can contribute to the development of this open-source project. All contributions are welcome, whether in code, documentation, bug reports, feature suggestions, or financial support.

## 📋 Code of Conduct

This project adopts a code of conduct to ensure a collaborative and respectful environment. By participating, you agree to:

-   Be respectful to all participants
-   Accept constructive feedback
-   Focus on collaborative solutions
-   Maintain privacy and data security
-   Respect the project's GPLv3 license

## 🤝 How to Contribute

### Reporting Bugs

Found an issue? Help us improve!

1. **Check if already reported**: Search in [Issues](https://github.com/mgnischor/ecommerce-backend/issues) if the issue has already been reported
2. **Create a new issue**: If not found, [open a new issue](https://github.com/mgnischor/ecommerce-backend/issues/new)
3. **Provide complete details**:
    - Clear description of the problem
    - Steps to reproduce
    - Expected vs. actual behavior
    - Environment (OS, .NET version, database)
    - Error logs (if applicable)

### Suggesting Features

Have an idea to improve the project?

1. **Check if already suggested**: Search in [Issues](https://github.com/mgnischor/ecommerce-backend/issues) with the `enhancement` tag
2. **Open a discussion**: [Create a new issue](https://github.com/mgnischor/ecommerce-backend/issues/new) with the `enhancement` tag
3. **Describe in detail**:
    - The problem the feature solves
    - How it would work
    - Benefits for users
    - Possible impacts on existing code

### Contributing Code

Want to contribute directly with code?

1. **Set up development environment** (see section below)
2. **Choose a task**: Look for issues with `good first issue` or `help wanted` tags
3. **Create a branch**: `git checkout -b feature/feature-name`
4. **Develop**: Follow the code guidelines below
5. **Test**: Run tests and verify everything works
6. **Commit**: Use descriptive messages following [Conventional Commits](https://conventionalcommits.org/)
7. **Push and Pull Request**: Push your branch and open a PR

### Contributing Documentation

Documentation is crucial for open-source projects!

-   **README.md**: Improve descriptions, installation instructions, examples
-   **Code**: Add XML comments to public methods
-   **Wiki**: Help expand repository documentation
-   **Translation**: Translate documentation to other languages

### Financial Support

This project is maintained by volunteers and any financial support is welcome:

-   **GitHub Sponsors**: [Support via GitHub](https://github.com/sponsors/mgnischor)

Your support helps us:

-   Maintain development servers
-   Acquire tools and resources
-   Dedicate more time to the project
-   Improve infrastructure

## 🛠 Development Setup

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   [PostgreSQL 15+](https://www.postgresql.org/download/)
-   [Git](https://git-scm.com/)
-   [Docker](https://www.docker.com/) (optional)

### Installation Steps

1. **Clone the repository**:

    ```bash
    git clone https://github.com/mgnischor/ecommerce-backend.git
    cd ecommerce-backend
    ```

2. **Install dependencies**:

    ```bash
    dotnet restore
    ```

3. **Configure the database**:

    - Create a PostgreSQL database named `ecommerce`
    - Configure the connection string in `appsettings.Development.json`

4. **Configure JWT**:

    - Add your JWT keys in `appsettings.Development.json`

5. **Run migrations**:

    ```bash
    dotnet ef database update
    ```

6. **Run the project**:
    ```bash
    dotnet run
    ```

### Useful Scripts

-   `scripts/build-local.ps1`: Complete build with migrations
-   `scripts/build-docker.cmd`: Docker build
-   `scripts/update-version.ps1`: Version update

## 🔄 Pull Request Process

1. **Fork the repository** and clone your fork
2. **Create a descriptive branch**: `feature/feature-name` or `fix/issue-123`
3. **Develop** following the guidelines below
4. **Test** your changes:
    - Run `dotnet test`
    - Verify builds pass
    - Manually test functionalities
5. **Commit** with clear messages:
    ```
    feat: add user authentication endpoint
    fix: resolve null reference in product service
    docs: update API documentation
    ```
6. **Push** your branch
7. **Open a Pull Request**:
    - Describe the changes
    - Reference related issues
    - Add screenshots if applicable
8. **Wait for review** and make adjustments if necessary

## 📝 Code Guidelines

### C# Style

-   Use C# 12 features when appropriate
-   Follow [Naming Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
-   Use `var` when the type is obvious
-   Prefer LINQ for collection operations
-   Use async/await for I/O operations

### Project Structure

-   **Domain**: Entities, Value Objects, Domain Interfaces
-   **Application**: Commands, Queries, Services, DTOs
-   **Infrastructure**: Persistence, Repositories, Configurations
-   **API**: Controllers, Middlewares, Configuration

### Testing

-   Write unit tests for business logic
-   Use integration tests for APIs
-   Maintain test coverage above 80%
-   Name tests descriptively: `Should_Return_Product_When_Valid_Id`

**Running Tests:**

```bash
# Run all tests
dotnet test

# Run with coverage report
cd tests/scripts
./run-tests-with-coverage.ps1
```

### Commits

Follow [Conventional Commits](https://conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## 📄 License

By contributing, you agree that your contributions will be licensed under the **GNU General Public License v3.0**. This ensures the software remains free and open source.

## 🙏 Acknowledgments

Thank you for contributing! Your help makes this project better for the entire community. Every contribution, no matter how small, is valuable.

For questions, open an [issue](https://github.com/mgnischor/ecommerce-backend/issues) or contact [miguel@datatower.tech](mailto:miguel@datatower.tech).

---

**Repository**: [https://github.com/mgnischor/ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)
