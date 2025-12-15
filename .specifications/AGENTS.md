# Measurement Visualizer

## Overview

This is a sample project demonstrating a simulated measurement generator (C#) and visualizer (TypeScript, Angular).

The measurement generator produces an asynchronous stream of measurements (currently simulated, later real measurement devices). The measurements are transferred to the client using SSE (Server-Sent-Events). The visualizer consumes the stream and displays the measurements in real-time.

## Project Structure

* Project uses **Aspire**
  * `AppHost` for hosting
  * `ServiceDefaults` for DI setup incl. logging with Open Telemetry
* `WebApi` project
  * ASP.NET Core Web API
  * Exposes SSE endpoint for measurements
  * For simplicity, also contains generator implementation and interface in `Measurements` folder/namespace
* `WebApiTests` project
  * xUnit tests for WebApi project
* `Frontend` project
  * Angular application
* `Playground` folder
  * Can be ignored, used for prototyping and experiments only

## Technologies

* .NET 10 with C# 14
* Angular 21

## Important Tasks

* Build
  * Backend: `dotnet build` in project's root folder
  * Frontend: `npm run build` in `Frontend` folder
* Web API
  * `dotnet build` automatically creates `Frontend/WebApi.json` from backend's OpenAPI spec. Never modify this file manually.
  * Run `npm run generate-web-api` in `Frontend` folder to regenerate Angular API client after modifying backend API.
  * The api client is generated in `Frontend/src/app/api` folder.
* Run tests:
  * Backend: `dotnet test` in project's root folder
  * Frontend: Currently no automated frontend tests
* Install dependencies:
  * Backend: `dotnet restore` in project's root folder
  * Frontend: `npm install` in `Frontend` folder

Do **not** start the application yourself. If you want to run the full application, ask me (the user) to start it. I will tell you the port it runs on.

## Coding Guidelines

### Angular

- Uses Angular version 21
- Use standalone components by default
- TypeScript for type safety
- Angular CLI for project setup and scaffolding including creation of new components, services, etc.
- Strict mode is enabled in `tsconfig.json` for type safety
- Follow Angular's component lifecycle hooks best practices
- Use `input()` `output()`, `viewChild()`, `viewChildren()`, `contentChild()` and `contentChildren()` functions instead of decorators
- Use `OnPush` for performance
- Keep templates clean and logic in component classes or services
- Use Angular directives and pipes for reusable functionality where appropriate
- Use Angular's component-level CSS encapsulation
- Implement responsive design using CSS Grid and Flexbox
- Use Angular Signals for reactive state management in components and services. See `.guides/angular-signals.md` and `.guides/angular-signals-forms.md` for details.
- Leverage `signal()`, `computed()`, and `effect()` for reactive state updates
- Use writable signals for mutable state and computed signals for derived state
- Handle loading and error states with signals and proper UI feedback
- Use Angular's `inject()` function for dependency injection in standalone components
- Use `firstValueFrom` to convert observables to promises when working with the web API. Prefer async/await over RxJS operators for better readability.

## Quality Control

Whenever you modify code, ensure that the code compiles without warnings. Additionally, make sure that all unit tests pass successfully.

Only build/run tests of those components that you have modified (C# if you changed backend code, Frontend if you changed Angular code).
