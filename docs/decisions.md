# Decision Log

## 001 – Built own HL7 parser before adopting NHapi
**Date:** 2026-09-29
**Decision:** Wrote a minimal parser by hand (delimiters, MSH numbering, components).
**Why:** To learn the v2 encoding rules well enough to troubleshoot real interfaces and prepare for the HL7 Control Specialist exam.
**Trade-off:** Not production-grade (no data-type validation). Plan to switch to NHapi in Phase 3.

## 002 – Accept \n and \r\n as segment separators
**Date:** 2026-09-29
**Decision:** The parser normalizes \r\n and \n to \r before splitting segments.
**Why:** Test messages pasted from emails, text files, and Windows tools often have the
wrong line endings. Rejecting them would make the tool frustrating to use for troubleshooting.
**Trade-off:** HL7 v2 requires \r (0x0D). Accepting other separators hides a sender's
non-conformance. In production, the listener should:
  1. still parse the message (don't block patient care over line endings),
  2. log a warning with the sending application (MSH-3) and control ID (MSH-10),
  3. report it to the sending system's team so they fix it at the source.

**ToDo:** Phase 3, when the MLLP listener is built, add the warning log.
## 003 SQL Server in Docker via Componse 
## 004 – Store raw message plus extracted MSH/PID fields
**Decision:** Messages table keeps the full raw text AND copies key fields into indexed columns.
**Why:** Raw = legal evidence and replay; columns = fast search. LIKE on raw text can't use indexes.
**Trade-off:** Duplicated data and more storage (cheap compared with troubleshooting time).

## 005 – Receive timestamps stored in UTC
**Decision:** ReceivedAtUtc uses SYSUTCDATETIME(); MSH-7 is kept as sent.
**Why:** Local time repeats an hour at the fall DST change, making result order ambiguous.
**Trade-off:** Must convert to local time for display.

## 006 – MSH-10 control ID indexed but not unique
**Decision:** Non-unique index on (InterfaceId, ControlId).
**Why:** Senders legitimately resend after timeouts/NAKs, and some reset counters. A unique
constraint could block patient care. Duplicates will be detected in application logic.
**Trade-off:** Duplicates can get in; needs monitoring (see duplicate-detection query).

## 007 – Docker Desktop for local infrastructure
**Decision:** SQL Server (and later IRIS for Health) run in containers via Docker Desktop.
**Why:** Reproducible dev environment; nothing installed directly on the Mac.
**Trade-off:** On Apple Silicon, SQL Server runs as an x86 image through Rosetta emulation
(slower, fine for dev). Containers use 4 GB+ of RAM while running; stop them with
`docker compose stop` when not working.

## 008 – Hl7DateTime ignores year-only and year-month values
**Decision:** Parse accepts YYYYMMDD and longer; YYYY and YYYYMM return null.
**Why:** Only used for MSH-7, where low precision is useless. Converting "1955" to
1955-01-01 would invent precision the sender never gave (dangerous for DOB/age logic).
**Revisit:** Before parsing PID-7 (DOB), store precision alongside the value.