# TimberFrame2D (CE 4011)

A custom, high-performance 2D structural analysis and design engine built from scratch in C#. This project demonstrates advanced software engineering principles, design patterns, and domain-driven design (DDD) applied to civil engineering computational tools.

🚧 **Work in Progress:** This project is actively being developed as a graduate term project at METU. The core matrix solver and loading architecture are currently being finalized.

## Architecture & Features

- **Decoupled 2D Matrix Structural Analysis Solver:** A standalone linear elastic stiffness matrix solver for evaluating timber frames, trusses, and beam elements.

- **Domain-Driven Material-Agnostic Loading Layer:** An independent loading subsystem that automatically handles code-standard load combinations (ASCE 7, EN 1990, TBDY) using generic generators, completely isolated from specific material design rules.

- **Extensible Member Designer Module:** A specialized timber design module (supporting US, EC5, and Turkish codes) that maps seamlessly onto the core analysis engine outputs, architected to allow future expansion into steel and concrete modules.

- **Modern Desktop Interface:** A highly responsive user interface built using WPF and .NET 10 for model visualization and section data binding.

- **CAD/BIM Integration:** Planned integration endpoints designed to consume architectural layout data directly from Revit API suites.

## Technologies & Environment

- **Language & Runtime:** C# 14, .NET 10
- **UI Framework:** WPF (Windows Presentation Foundation)
- **IDE & Tooling:** Visual Studio 2026, GitHub Copilot / OpenAI Codex integration for optimized development workflows

## Design Patterns & Principles

- **SOLID Principles:** High cohesion and loose coupling across separate projects (FrameAnalysisProgram, MemberDesigner, UI.Core)

- **Factory Pattern:** Implemented in the loading layer (LoadingCaseFactory) for standardized, error-free structural load generation

- **Interface Segregation:** Ensuring analysis solvers interact only with core mathematical abstractions rather than heavy, material-specific UI elements

## Getting Started

Clone the repository and explore the codebase to understand the implementation details and design decisions.

## License

This project is open source and available for educational and professional purposes.
