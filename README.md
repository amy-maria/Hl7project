🏥 HealthHub: HL7-to-FHIR Interoperability Engine

HealthHub is a high-performance .NET-based engine designed to automate the ingestion, validation, and transformation of clinical data. It acts as a bridge between legacy HL7 v2 messaging (used by many hospital systems) and modern FHIR-based web services.

🚀 Core Capabilities
Real-time Ingestion: A high-performance MLLP listener that handles concurrent TCP connections.
Clinical Validation: A modular rule engine that performs strict validation for standard data, while allowing for "Emergency Mode" to ensure patient care is never delayed by missing information.
Data Transformation: An automated pipeline that converts raw HL7 v2 segments into standardized, searchable FHIR JSON resources.
Reliable Persistence: A data layer that stores both the raw "legal" record and extracted metadata for rapid clinical searching. 

## Project Demo
<img src="./docs/image/Screenshot 2026-10-22 at 11.38.03 AM.png" width="600" alt="HealthHub Desktop UI">


# Technical Highlights
## 🚀 Technical Highlights

* **Architecture:** Implemented **Clean Architecture** and **Separation of Concerns** to decouple core domain logic from data access and user interface layers.
* **Design Patterns:** Utilized the **Strategy Pattern** for a modular, pluggable validation engine and the **Repository Pattern** for standardized data persistence.
* **Asynchronous Programming:** Leveraged **Async/Await** and **Task-based programming** to build a non-blocking, high-performance MLLP listener capable of handling concurrent connections.
* **Modern C# Features:** Utilized modern language features including **Records**, **Primary Constructors**, and **LINQ** to write concise, highly readable, and maintainable code.
* **Dependency Injection:** Implemented **Dependency Injection (DI)** to ensure the system is highly testable and to allow for easy modularity across the validation and data layers.

