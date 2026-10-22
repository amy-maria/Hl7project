# Architecture Decision Records (ADR) - HealthHub Project

This document captures the significant architectural decisions made during the development of the HealthHub Interoperability Engine. These records provide context for *why* certain technical paths were chosen, including the clinical trade-offs.

---

## ADR 001: Custom HL7 Parser Implementation
**Status:** Accepted
**Context:** We need a lightweight way to parse HL7 v2 messages for internal routing and validation.
**Decision:** Implement a custom, minimal parser to handle MSH numbering, delimiters, and component/repetition logic.
**Consequences:** 
- **Pros:** Extremely lightweight; no external dependencies; provides deep understanding of v2 encoding for troubleshooting.
- **Cons:** Does not currently support full HL7 data-type validation (e.    g., checking if a field is a valid Date or XPN).
**Note:** Plan to use NHapi as a reference implementation in the testing suite.

## ADR 002: Delimiter Normalization
**Status:** Accepted
**Context:** Real-world HL7 sources often provide inconsistent line endings (e.g., `\n` or `\r\n` instead of the standard `\r`).
**Decision:** The parser will normalize all incoming line endings to `\r` before splitting segments.
**Consequences:** 
- **Pros:** Increases the "Ingestion Reliability" of the engine, preventing message rejection due to trivial formatting issues.
- **Cons:** Hides non-conformance from the sender. 
**Mit1:** The MLLP Listener must log a warning including the `MSH-3` (Sending Application) and `MSH-10` (Control ID) when non-standard delimiters are detected.

## ADR 003: Relational Data Storage (SQL Server)
**Status:** Accepted
**Context:** We need a reliable, searchable store for message history and interface configurations.
**Decision:** Use SQL Server (running in Docker) to store both the raw message text and extracted metadata.
**Con Consequences:** 
- **Pros:** Allows for fast, indexed searching of specific clinical identifiers (like MRN) while preserving the "Legal Record" (the raw message).
- **Cons:** Requires managing a database lifecycle and handling schema migrations.

## ADR 004: Data Redundancy for Search Performance
**Status:** Accepted
**Context:** Searching for a specific Patient ID within a massive blob of raw text is slow and resource-intensive.
**Decision:** Use a "Dual-Storage" approach. Store the full raw message, but also extract key fields (MSH-10, PID-3, etc.) into dedicated, indexed columns.
**Consequences:** 
- **Pros:** Enables near-instantaneous search for clinicians.
- **Cons:** Slight increase in storage requirements and write-time complexity.

## ADR 005: Timezone Management (UTC)
**Status:** Accepted
Context: Healthcare data must be chronologically accurate across different timezones and Daylight Savings transitions.
**Decision:** All system-generated timestamps (e.g., `ReceivedAtUtc`) must be stored in UTC. 
**Consequences:** 
- **Pros:** Ensures a single, unambiguous timeline for all messages.
- **Cons:** The UI layer must handle the conversion from UTC to the user's local time for display.

## ADR 006: Identifier Logic (MRN Selection)
**Status:** Accepted
Context: A patient may have multiple identifiers (SSN, Medical Record Number, Driver's License).
**Decision:** The system will identify the primary MRN by searching for the identifier type `MR` in the `PID-3` field.
**Consequences:** 
- **Pros:** Highly reliable; avoids the risk of grabbing the wrong ID if the order of identifiers changes.
- **Cons:** Requires the sender to follow the standard identifier type coding.

## ADR 007: Context-Aware Validation (Emergency Mode)
**Status: Accepted**
**Context:** In clinical environments, "Strict Data" can conflict with "Patient Safety." For example, a trauma patient may arrive without a name or ID.
**Decision:** Implement a `ValidationContext` that allows the engine to switch between **Standard Mode** and **Emergency Mode** based on the incoming `MSH-9` (Trigger Event) code.
**Consequences:** 
- **Pros:** Allows for "Permissive Validation" during critical events (e.g., `ADT^A99`), ensuring that life-saving data is ingested even if it is incomplete.
- **Cons:** Requires careful monitoring to ensure "Emergency Mode" is not used to bypass standards during routine operations.

## ADR 008: FHIR Transformation Layer
**Status: Accepted**
**Context:** Modern healthcare is moving toward the FHIR (Fast Healthcare Interoperability Resources) standard. 
**Decision:** Build a dedicated transformation layer that converts legacy HL7 v2 messages into standardized FHIR JSON resources.
**Consequences:** 
- **Pros:** Makes our system "Future-Proof" and allows it to integrate with modern cloud-based EMRs and mobile applications.
- **Cons:** Requires maintaining a mapping logic between the two different data models.
