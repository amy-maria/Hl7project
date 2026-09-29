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