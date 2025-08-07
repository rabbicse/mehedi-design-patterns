# Mehedi.Patterns.ObserverToolkit

A lightweight and reusable implementation of the Observer design pattern for .NET Core, WPF, and WinForms, built with Clean Architecture principles. The `Mehedi.Patterns.ObserverToolkit` provides a flexible and decoupled way to implement event-driven communication in your .NET applications.

## Overview

The `Mehedi.Patterns.ObserverToolkit` is part of the `mehedi-design-patterns` repository, which offers reusable architectural patterns for .NET developers. The Observer pattern enables a one-to-many dependency between objects, allowing multiple observers to be notified of state changes in a subject. This toolkit is designed to be lightweight, easy to integrate, and aligned with Clean Architecture for maintainable and testable code.

## Features

- **Decoupled Communication**: Observers and subjects are loosely coupled, reducing dependencies.
- **Event-Driven Updates**: Supports real-time notifications for UI updates or data changes.
- **Clean Architecture Compliance**: Separates business logic from infrastructure for better maintainability.
- **Cross-Platform Support**: Compatible with .NET Core, WPF, and WinForms applications.
- **Extensible Interface**: Easily extend or customize the toolkit for specific use cases.

## Getting Started

### Prerequisites

- .NET Core SDK (version 6.0 or higher)
- Visual Studio 2022 or any compatible IDE
- Basic understanding of the Observer pattern and Clean Architecture

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/rabbicse/mehedi-design-patterns.git
   ```
2. Navigate to the ObserverToolkit directory:
   ```bash
   cd mehedi-design-patterns/patterns/Mehedi.Patterns.ObserverToolkit
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```

### Usage

The `Mehedi.Patterns.ObserverToolkit` provides interfaces and classes to implement the Observer pattern. Below is a basic example of how to use it:

```csharp
using Mehedi.Patterns.ObserverToolkit;

// Create a subject
var subject = new Subject<string>();

// Create observers
var observer1 = new ConcreteObserver<string>("Observer1");
var observer2 = new ConcreteObserver<string>("Observer2");

// Subscribe observers to the subject
subject.Subscribe(observer1);
subject.Subscribe(observer2);

// Notify observers of a state change
subject.Notify("Data has changed!");

// Unsubscribe an observer
subject.Unsubscribe(observer1);
```

This example demonstrates subscribing observers to a subject and notifying them of changes. Check the `src` directory for detailed examples and additional configuration options.

## Project Structure

- **src/**: Contains the core implementation of the Observer pattern, including `ISubject`, `IObserver`, and concrete classes.
- **tests/**: Unit tests to ensure reliability and correctness of the toolkit.
- **examples/**: Sample applications demonstrating usage in .NET Core, WPF, and WinForms.

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/observer-enhancement`).
3. Make your changes and commit (`git commit -m "Add observer enhancement"`).
4. Push to the branch (`git push origin feature/observer-enhancement`).
5. Open a pull request.

Please ensure your code adheres to Clean Architecture principles and includes unit tests.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/rabbicse/mehedi-design-patterns/blob/master/LICENSE) file for details.

## Contact

For questions or feedback, reach out via [GitHub Issues](https://github.com/rabbicse/mehedi-design-patterns/issues) or contact the maintainer at [your-email@example.com].

---

© 2025 rabbicse
