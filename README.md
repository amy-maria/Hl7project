# Project Overview
HealthHub is an HL7-to-FHIR Interoperability Engine designed to bridge the gap between legacy medical devices and modern healthcare cloud environments. It features context-aware validation to support clinical emergency workflows.

# From HL7 to FHIR
Problem: "Legacy HL7 data is difficult to parse and doesn't work with modern web applications."
Solution: "HealthHub ingests raw MLLP streams, validates them against clinical rules, and transforms them into standardized FHIR JSON resources."

## Project Demo
<img src="./docs/image/Screenshot 2026-10-22 at 11.38.03 AM.png" width="600" alt="HealthHub Desktop UI">


# Technical Highlights
-Architecture: "Implemented Clean Architecture and Separation of Concerns to decouple core logic from data and UI."
-Design Patterns: "Utilized the Strategy Pattern for a modular validation engine and the Repository Pattern for data persistence."
-Asynchronous Programming: "Leveraged Async/Await and Task-based programming to build a non-blocking, high-performance MLLP listener."
-Modern C# Features: "Utilized Records, Primary Constructors, and LINQ to write concise, maintainable, and modern C# code."
-Dependency Injection: "Implemented Dependency Injection to allow for easy testing and modularity of the validation and data layers."
