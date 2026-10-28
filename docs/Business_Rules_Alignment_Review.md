# The Dauntless Grading System
## Business Rules Alignment Review and Corrected Rules

Prepared 29 October 2026. Reviewed document: 2.1 Narrative Business Rules (17 rules). Comparison basis: the current application implementation and the supplied database schema. This review distinguishes implemented application behavior from database constraints.

## 1. Overall assessment

The business rules mostly align with the system, but several statements require correction before final documentation. Rules 2, 9, 12, and 13 are the highest priorities because they describe behavior that differs directly from the application.

## 2. Review of all 17 rules

| Rule | Assessment | Findings and required clarification |
| --- | --- | --- |
| 1. Accounts and identifiers | Mostly accurate | Email uniqueness is enforced within each account table, not globally across students, teachers, and administrators. The same email can exist in different role tables. Password hashing is enforced by the application; NOT NULL alone does not guarantee a hashed value. |
| 2. Deactivation only | Incorrect | The admin interface supports both deactivation and permanent deletion. Deletion is allowed for eligible accounts without blocking related records. |
| 3. Courses and classes | Accurate | Matches the course-to-class relationship. |
| 4. Class identity and teacher assignment | Accurate | Matches the unique combination of course code, section, semester, and academic year, with one assigned teacher per class. |
| 5. Enrollment | Accurate | Students can join multiple classes, and duplicate enrollment in the same class is prevented. |
| 6. Assessments | Mostly accurate | The database permits weights from 0 to 100, but the current assessment-creation workflow requires greater than 0 and at most 100. |
| 7. One grade per assessment | Accurate | More precisely, one grade record per enrollment-assessment pair. |
| 8. Score validation | Accurate, with an omission | Add that recorded scores must be nonnegative and support at most two decimal places. |
| 9. Fixed 50/50 weights | Incorrect | The application does not enforce a total of 100 or a Midterm/Finals split of 50/50. It uses relative weights normalized over graded assessments. |
| 10. Running grade and statuses | Partly accurate | Running grade is cached on enrollment and overall standing is derived. However, assessment remarks are also saved in StudentGrade.remarks. Pending describes an ungraded assessment; overall standing uses Not Yet when there is no running grade. |
| 11. Revision requests | Mostly accurate | The student must own the graded record, and only one open request per grade is allowed. Structured requested points must be positive; a plain re-check uses NULL. |
| 12. Request lifecycle | Needs clarification | The four statuses are not mandatory sequential stages. Pending can go directly to Approved or Closed. Reviewer and review time are populated on approval or closure, not merely when marked Under Review. |
| 13. Teacher-only score changes | Incorrect as an absolute statement | Assigned active teachers can grade and review requests, but authorized administrators can also record scores through Admin Quick Grade Entry. Request decisions remain with the assigned teacher. |
| 14. Audit logging | Mostly accurate | Exactly one actor matches the schema. Old and new values are optional depending on the action; not every entry has both. |
| 15. Audit entity references | Accurate | Entity type and entity ID are not foreign keys, allowing an audit entry to remain after its target record is deleted. |
| 16. Deletion protection | Accurate for the listed relationships | Keep the examples, but do not imply every foreign key restricts deletion. Some relationships deliberately use cascading deletion or SET NULL. |
| 17. Role-based access | Broadly accurate | Replace role check on every request with a more precise statement about protected operations validating role, ownership, and account status where applicable. |

## 3. Corrected business rules for documentation

The following is a consolidated, copyable version of all 17 rules. Unaffected rules are retained in wording consistent with the reviewed relationships.

### Rule 1. Separate accounts and unique identifiers

Students, teachers, and administrators have separate account records with unique primary keys. Email addresses are unique within each role's account table. The application hashes passwords before storing them.

### Rule 2. Account activation, deactivation, and deletion

Student and teacher accounts may be activated or deactivated. Administrators may permanently delete eligible accounts only when related records do not prevent deletion. Accounts with protected relationships should be deactivated to preserve their history.

### Rule 3. Courses and class offerings

A course represents a subject and is identified by its course code. A course may have multiple class offerings. Each class references an existing course, and a course with related classes cannot be deleted while those references remain.

### Rule 4. Class identity and assigned teacher

A class represents a particular offering of a course for a section, semester, and academic year. Each class is assigned to exactly one teacher, and a teacher may handle multiple classes. The combination of course code, section, semester, and academic year must be unique.

### Rule 5. Enrollment

A student may enroll in multiple classes, and a class may contain multiple students. Enrollment links students to classes. A student may be enrolled in a particular class only once.

### Rule 6. Assessment ownership and definition

Each assessment belongs to one class and one grading period, either Midterm or Finals. Assessments have a type, positive maximum score, and relative weight. The application requires a weight greater than zero and not exceeding 100, with at most two decimal places.

Supported assessment types are Quiz, Activity, Exam, Project, and Attendance. The database weight constraint permits zero, but the current assessment-creation workflow applies the stricter positive-weight rule.

### Rule 7. Unique assessment grade

At most one StudentGrade record may exist for each enrollment-assessment pair. Each grade references an existing enrollment and an existing assessment.

### Rule 8. Valid score and matching class

A score remains NULL until graded. Recorded scores must be between zero and the assessment's maximum score, with at most two decimal places. The enrollment and assessment must belong to the same class.

### Rule 9. Relative weights and running-grade calculation

Running grades use relative assessment weights. The application normalizes these weights over graded assessments and does not require their total to equal 100 or enforce a fixed 50/50 division between Midterm and Finals. Ungraded assessments are excluded, while recorded zero scores are included.

Running Grade = [sum((Score / Maximum Score) x Weight) / sum(Weights of Graded Assessments)] x 100

Only graded assessments with valid positive maximum scores and weights contribute. If no assessments qualify, the running grade is NULL rather than zero.

### Rule 10. Cached running grade and displayed standing

The running grade is calculated from recorded scores, assessment maxima, and relative weights, and is cached on the enrollment record. Overall standing is derived from this value: Passed or At risk, with Not Yet when no running grade exists. Individual assessment remarks are also saved in StudentGrade.

The current passing threshold is 75%. Pending describes an ungraded assessment, while Not Yet describes an overall standing without a running grade.

### Rule 11. Student revision requests

A student may submit a revision request for their own graded assessment, providing a reason and optionally requesting a positive point adjustment. NULL requested points indicates a plain re-check. Only one Pending or Under Review request may exist for the same grade at a time.

### Rule 12. Revision-request lifecycle

A revision request starts as Pending. The assigned teacher may mark it Under Review, approve it, or close it. Under Review is optional. Approval or closure records the reviewing teacher and review time. Approved and Closed requests cannot be processed again through the normal review workflow.

### Rule 13. Score-entry and review authorization

Active teachers may record scores and review revision requests only for their assigned classes. Authorized administrators may also record scores through the administrative grade-entry workflow. Revision-request decisions remain restricted to the assigned active teacher.

### Rule 14. Audit logging

Important supported actions are recorded in the audit log with an action, timestamp, affected entity, and exactly one actor: a student, teacher, or administrator. Old values, new values, and additional details are recorded where applicable.

### Rule 15. Audit target references and retained history

The type and identifier of the record affected by an audit entry are stored in entity_type and entity_id. These target-reference fields are not foreign keys, allowing audit entries to remain if the target record is later deleted. Actor references are separate and retain their foreign-key protections.

### Rule 16. Deletion restrictions and relationship protection

Deletion is restricted where protected references remain, including a teacher with assigned classes, a course with classes, or a student or class with enrollments. Audit actor references may also prevent account deletion. These restrictions protect related records and history. Other relationships may use CASCADE or SET NULL as defined in the database schema.

### Rule 17. Role-based access and protected operations

Students access their own grades and requests, teachers access their assigned classes, and administrators manage accounts and administrative workflows. Protected operations validate the applicable role, ownership, and account status before allowing changes.

## 4. Defense reminders

- Correct Rule 2: the system supports both deactivation and protected permanent deletion.
- Correct Rule 9: weights are relative; a fixed 50/50 split is not enforced.
- Correct Rule 12: Under Review is optional; approval and closure populate reviewer fields.
- Correct Rule 13: authorized administrators have a score-entry workflow, while request decisions remain teacher-controlled.
- Distinguish database constraints from application validation. NOT NULL does not itself enforce password hashing, and a row-level weight CHECK does not enforce a class total.
- Do not describe all status information as unstored: assessment remarks are saved, while overall standing is derived.
- A student's free-text reason does not automatically execute a point adjustment. A teacher must approve an appropriate correction.

This document reviews alignment and provides documentation wording. It does not change application behavior or the database schema.
