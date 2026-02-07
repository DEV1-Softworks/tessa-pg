# Contributing to Tessa Procedural Level Generator for Unity

Thank you for your interest in contributing to Tessa! We welcome contributions and issue reports from the community and you feel like you're playing with it at home. To ensure a smooth contribution process, please follow the guidelines outlined in this document. 

## Standards and Guidelines

### Code Style

Please adhere to the existing code style and conventions used in the project. This includes naming conventions, formatting, and commenting. Consistency in code style helps maintain readability and makes it easier for others to understand your contributions.

Please consider the following guidelines:

- Please use the prefix Tessa for all of your classes. This helps to avoid naming conflicts and makes it clear that the class is part of the Tessa library. Only contracts, enums, algorithms and interfaces can be named without the prefix, and they should be placed in the folder named "Contracts" to further distinguish them from the rest of the codebase.
- If you want to add an algorithm, please add it to the "Algorithms" folder. If this algorithm is for platformer levels, please add it to the `Algorithms/Platformer` folder. If it is for top-down levels, please add it to the `Algorithms/TopDown` folder. This helps to keep the codebase organized and makes it easier for others to find and understand your contributions.
- Use clear and descriptive names for variables, methods, and classes. Avoid abbreviations unless they are widely understood. This helps improve code readability and maintainability for other contributors.
- Use consistent indentation and spacing. Follow the existing formatting style used in the project to maintain a uniform codebase.
- Include comments where necessary to explain complex logic or important decisions. This helps other contributors understand the reasoning behind your code and makes it easier for them to review and maintain it in the future.
- Avoid adding unnecessary comments that state the obvious. Focus on providing meaningful comments that add value to the code and help others understand its purpose and functionality.
- When making changes, ensure that your code is well-structured and organized. This includes breaking down large methods into smaller, more manageable ones, and grouping related code together. This helps improve readability and makes it easier for others to navigate and understand your contributions.

### SOLID Principles

When contributing to the project, please keep in mind the SOLID principles of object-oriented design. These principles help create maintainable and scalable code. Here is a brief overview of the SOLID principles:

- **Single Responsibility Principle**: A class should have only one reason to change, meaning it should have only one responsibility or job. This helps keep classes focused and easier to maintain.
- **Open/Closed Principle**: Software entities (classes, modules, functions, etc.) should be open for extension but closed for modification. This means that you should be able to add new functionality without changing existing code, which helps prevent bugs and maintain stability.
- **Liskov Substitution Principle**: Objects of a superclass should be replaceable with objects of a subclass without affecting the correctness of the program. This promotes the use of inheritance and polymorphism in a way that maintains the integrity of the code.
- **Interface Segregation Principle**: Clients should not be forced to depend on interfaces they do not use. This encourages the creation of smaller, more specific interfaces rather than large, general ones, which can lead to better modularity and flexibility.
- **Dependency Inversion Principle**: High-level modules should not depend on low-level modules. Both should depend on abstractions. This promotes the use of interfaces and dependency injection, which can help decouple code and improve testability.

If a class doesn't comply any of these principles, please consider refactoring it to adhere to them. This will help maintain the overall quality and maintainability of the codebase. However, we understand that in some cases, especially when dealing with Unity's MonoBehaviour classes, it may not be possible to fully adhere to these principles due to the nature of Unity's architecture, so please address in the header of the class as comments the reasons why it violates the principles and any potential refactorings that could be done in the future to address these non-compliances.

### Additional Guidelines

For C# code, please follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions). For Unity-specific code, please also consider Unity's best practices and guidelines.

## AI usage
Tessa is AI friendly, and we encourage contributors to use AI tools to assist in their contributions. However, please ensure that any AI-generated code is compliant with the project's license and does not contain any code that would violate the license. No AI-generated code will be accepted without proper attribution and compliance with the license. 

Also, by contributing to this project, you accept that your contributions will be licensed under the same license as the project (Beerware), and that you have the right to submit the code and that it does not infringe on any third-party rights.

Finally, please remember that while AI tools can be helpful, they should not replace human judgment and creativity. Always review and test your contributions thoroughly to ensure they meet the project's standards and guidelines. The ownership and responsibility of the code you contribute ultimately lies with you, and it is important to ensure that your contributions are of high quality and do not violate any licenses or third-party rights.

If you want to add rules for AGENTS.md, edit the current AGENTS.md file and add the rules there and submit them as part of your contribution. We'll consider them and if they are good, we'll add them to the main file.