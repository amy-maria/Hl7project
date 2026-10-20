# Interface Specification: Registration to HealthHub (ADT^A01)

## 1. Overview
**Purpose:** This interface receives new patient admissions from the Registration System.
**Message Type:** ADT^A01 (Admission)
**Trigger Event:** Patient Admission (A01)
**Sending System:** Registration System

## 2. Mandatory Segments
*List the segments that MUST be present in every message for this to be considered valid.*
- [ x] MSH (Message Header)
- [ x] EVN (Event Type)
- [ x] PID (Patient Identification)
- [ x] PV1 (Patient Visit)

## 3. Field-Level Validation Rules


| Segment | Field | Requirement | Validation Rule (Logic) |
| :--- | :--- | :--- | :--- |
| MSH | 10 | Mandatory | Cannot be empty (Control ID) |
| PID | 3 | Mandatory | Must match pattern: `[0-9]{5,10}` |
| PID | 5 | Mandatory | Must contain at least `^` (Family^Given) |
| PID | 8 | Mandatory | Must be 'M', 'F', or 'U' |
| PV1 | 3 | Mandatory | Must not be empty (Location) |
| ... | ... | ... | ... |

## 4. Clinical Logic & Constraints
- MSH-3 sending application required
- MSH-9 message type required
- PID-3 (Patient ID) must follow the pattern [0-9]{5,10}.
- PID-5 (Patient Name) must have at least a Last^First. Need ability to register a Trauma/Code case with no valid name and update when information is received.
- PID-8 SEX accept M,F,U for unknown 
- PID-11 Patient address - required; needs to have "dummy" address in emergency, ability to handle for homeless, handle for international travelers, ability to validate address??
- PID-13 Phone number - home or mobile required. Ability to handle if no phone or info not available. Ability to handle outside the US phone numbers (ie. country codes)
- PID-18 Patient acct number
- PID-19 SSN - required. Need ability to update for emergency and number for people w/o SSN
- PVI1-2 Patient type (I=inpatient, O=outpatient, E=Emergency, P=preadmission)
- PV1-3 Assigned location- All locations for patient type available including overflow areas
- PV1-7 Attending Dr- required
- PV1-17 Admitting Dr- required
- PV1-44 Admit date/time - 
- Pv1-45 Discharge date/time- 
- EVN-1 Event type code
- EVN-2 Recorded date/time
- EVN-6 Event occurred
- NK1- Next of kin - can be empty


## 5. Sample Payload

MSH|^~\&|REGADT|MERCYHOSP|HUBENGINE|MERCYHOSP|20261001090000||ADT^A01^ADT_A01|MSG10001|P|2.5.1
EVN|A01|20261001090000
PID|1||E200001^^^MERCYEMPI^PI~200001^^^MERCYHOSP^MR||GARCIA^MARIA^L||19720614|F|||45 OAK AVE^^SPRINGFIELD^IL^62702||(217)555-0142
PV1|1|I|3EAST^310^B^MERCYHOSP||||5678^PATEL^ANITA^^^^MD


 