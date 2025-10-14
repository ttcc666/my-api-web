# Repository Guidelines

## Project Structure & Module Organization
- **Backend API (`backend/1-Presentation/MyApiWeb.Api/`)**: Hosts `Program.cs`, controllers, middleware, and cross-cutting configuration.
- **Business Layer (`backend/2-Business/`)**: `MyApiWeb.Models` stores DTOs; `MyApiWeb.Services` coordinates domain rules and integrates dependencies.
- **Data Access (`backend/3-DataAccess/MyApiWeb.Repository/`)**: Contains EF Core contexts, repositories, and persistence adapters.
- **Frontend (`frontend/`)**: Vue 3 SPA under `src/` with views, Pinia stores, API helpers, and Vitest cases inside `__tests__/`. Static assets live in `public/`.
- **Infrastructure**: Solution file `my-api-web.sln` keeps all projects aligned; CI logic resides under `.github/workflows/`; architecture decisions are documented in `架构设计方案.md`.

## Build, Test, and Development Commands
- `dotnet restore`: Restore backend dependencies before any build.
- `dotnet build backend/1-Presentation/MyApiWeb.Api/MyApiWeb.Api.csproj`: Compile the API to validate server changes.
- `dotnet run --project backend/1-Presentation/MyApiWeb.Api`: Launch the API locally; Serilog emits logs to `logs/`.
- `npm install` (run in `frontend/`): Install locked frontend dependencies.
- `npm run dev`: Start the Vite dev server with HMR for rapid UI feedback.
- `npm run build`: Produce a production bundle; pair with `npm run preview` for smoke tests.
- `npm run lint`, `npm run format`, `npm run type-check`: Enforce ESLint, Prettier, and `vue-tsc` standards.
- `npm run test:unit`: Execute Vitest suites in JSDOM.

## Coding Style & Naming Conventions
- **C#**: Prefer SRP-aligned services with constructor injection; types use PascalCase, parameters camelCase, and asynchronous methods append `Async`.
- **TypeScript/Vue**: Two-space indentation, single quotes, trailing commas, and no semicolons per Prettier. Components follow UpperCamelCase; Pinia stores adopt the `useXxxStore` pattern.
- **Formatting**: Run repository formatters before submitting; avoid inline explanatory comments unless clarifying complex flows.

## Testing Guidelines
- Place frontend tests under `frontend/src/__tests__` or alongside components with `.spec.ts`. Use clear `describe` names matching component titles and Testing Library assertions.
- Backend testing projects should live under `backend/tests/` and join `my-api-web.sln`; execute via `dotnet test`.
- Aim for coverage on controllers, services, and repositories that guard business-critical paths.

## Commit & Pull Request Guidelines
- **Commits**: Follow imperative Chinese phrases (e.g., `优化全局异常处理`), grouping a single logical change per commit.
- **Pull Requests**: Link related issues, describe user impact, and attach UI screenshots or GIFs for frontend changes. Confirm linting, tests, and builds pass before requesting review. Flag breaking changes and document migration guidance when necessary.

## Security & Configuration Tips
- Store environment-specific secrets via User Secrets or CI/CD vaults; never commit production credentials.
- Keep Serilog logs scrubbed of sensitive data and configure rolling policies for deployments.
- Bind strongly typed options in `Program.cs`, and prefer environment variables or `appsettings.Development.json` overrides for runtime differences.
