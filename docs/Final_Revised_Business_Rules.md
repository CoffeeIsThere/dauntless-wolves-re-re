# 2.1 Narrative Business Rules

Revised 29 October 2026

1. Every student, teacher and admin must have their own account with a unique ID. Two accounts of the same role must not share an email, and passwords must never be stored in plain form.

2. A student or teacher account that has protected related records, such as enrollments, assigned classes, reviewer references or audit entries, must not be deleted. These accounts may be deactivated to restrict access while preserving their records. Only an account with no blocking related records may be deleted, and only by an admin.

3. A course may be offered in many classes. A course that has classes must not be deleted.

4. Every class is one offering of a course for a section, semester and academic year, and must have exactly one teacher. The same offering must not be created twice. A teacher may handle many classes.

5. A student may enroll in many classes, and a class may have many students. A student must not be enrolled in the same class twice.

6. Every assessment must belong to one class and one term (Midterm or Finals), and must have a type, a maximum score above zero and a weight.

7. A student must have no more than one grade for each assessment.

8. A score stays blank until the assessment is graded. A score must not be negative, must not exceed the assessment’s maximum, must have at most two decimal places, and may be recorded only for an assessment in the student’s own class.

9. A student’s running grade must count only the assessments already graded, weighted in proportion to each other. Weights need not total 100 and terms need not be split 50/50.

10. A student is Passed at a running grade of 75% or higher and At risk below 75%. Until something is graded, the student is Not Yet.

11. A student may ask for a re-check or a point increase only on their own graded assessment. A grade may have only one open request at a time.

12. A new request starts as Pending. Only the class’s assigned active teacher may mark it Under Review, approve it or close it. Under Review is optional. The reviewer and review time must be recorded when the request is Approved or Closed. Marking a request Under Review must be recorded in the audit log but does not set the reviewer and review-time fields. A request that is Approved or Closed must not be processed again.

13. Only an active teacher assigned to a class may record scores and decide revision requests for it. Authorized admins may also record scores, but request decisions stay with the assigned teacher.

14. Every important action must be logged with who did it (exactly one person: a student, teacher or admin), and with the old and new values when a score changes.

15. The history of an action must be kept even after the record it refers to is deleted.

16. A teacher or course with classes, a student or class with enrollments, or an account referenced as an audit actor must not be deleted while those protected references remain. Where deletion is permitted, related records must follow the database’s defined deletion rules. Deleting an enrollment or assessment must also remove its related grades and the revision requests associated with those grades.

17. Each user may see and do only what their role allows: students see their own grades and requests, teachers see their assigned classes, and admins manage accounts. Role, ownership and account status must be checked before any protected action.
