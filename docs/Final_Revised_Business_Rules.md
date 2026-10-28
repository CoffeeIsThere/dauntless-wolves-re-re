# 2.1 Narrative Business Rules

Revised 29 October 2026. Backend identifies application validation.

| Rule No. | Business Rule | Database/Relationship Impact | Validation/Constraint |
| --- | --- | --- | --- |
| 1 | Each student, teacher and admin must have a unique account ID. Emails must be unique within each role, and passwords must be stored as hashes. | Separate Student, Teacher and Admin tables. | PRIMARY KEY; per-table UNIQUE email; NOT NULL password_hash. Backend: hashing. |
| 2 | Only admins may delete accounts without protected related records. Linked accounts may be deactivated instead. | Student.status; Teacher.status; linked records. | Active/Inactive status; restrictive foreign keys. Backend: admin and linked-record checks. |
| 3 | A course may have many classes and must not be deleted while classes reference it. | Course 1:M Class. | FOREIGN KEY course_code; ON DELETE RESTRICT. |
| 4 | Each class must have one teacher and a unique course, section, semester and academic year combination. | Teacher 1:M Class. | NOT NULL teacher_id foreign key; UNIQUE(course_code, section, semester, academic_year). |
| 5 | A student may join many classes but must not enroll in the same class twice. | Student M:N Class through Enrollment. | Foreign keys; UNIQUE(student_id, class_id). |
| 6 | Each assessment must belong to one class and term, with a valid type, positive maximum score and positive weight not exceeding 100. | Class 1:M Assessment. | Class foreign key; term/type ENUMs; CHECK maximum > 0, weight 0–100. Backend: weight > 0; two-decimal limits. |
| 7 | Each enrollment may have only one grade per assessment. | Enrollment and Assessment linked by StudentGrade. | Foreign keys; UNIQUE(enrollment_id, assessment_id). |
| 8 | Scores must remain blank until graded, fall between zero and the maximum, use at most two decimal places and match the enrollment’s class. | StudentGrade, Enrollment and Assessment. | Nullable DECIMAL(6,2). Backend: range, precision and same-class checks. |
| 9 | Running grades must use relative weights of graded assessments only. Recorded zeros must count; ungraded items must not. Fixed totals or term splits are not required. | Assessment and StudentGrade → Enrollment.running_grade. | Backend: sum(score/max × weight) ÷ sum(graded weights) × 100; NULL if no qualifying grades. |
| 10 | A running grade of at least 75% is Passed; below 75% is At risk. No running grade is Not Yet. | Enrollment.running_grade. | Nullable DECIMAL(5,2). Backend: shared calculator and 75% threshold. |
| 11 | Students may request a re-check or point increase for their own graded assessments, with only one open request per grade. | StudentGrade 1:M RevisionRequest. | Grade foreign key; reason required; NULL points = re-check. Backend: ownership, positive points and one Pending/Under Review request. |
| 12 | Requests must start Pending. The assigned active teacher may mark them Under Review, Approved or Closed. Approval or closure must record reviewer and time. Under Review must be audited. Final decisions must not be processed again. | RevisionRequest; Teacher; AuditLog. | Status ENUM. Backend: transition checks; reviewer/time on approval or closure; audit Under Review. |
| 13 | Assigned active teachers and authorized admins may record scores. Only the assigned active teacher may decide revision requests. | Class.teacher_id; Teacher.status; Admin. | Backend: actor authorization, active teacher and class ownership. |
| 14 | Important actions must be audited with one actor and a timestamp. Score changes must record available old and new values. | AuditLog references one account actor. | Actor foreign keys; CHECK exactly one actor; default timestamp; optional old/new values. |
| 15 | Audit history must remain when its target record is deleted. | AuditLog.entity_type and entity_id. | Entity fields are not foreign keys; INDEX(entity_type, entity_id). |
| 16 | Protected references must block deletion. Permitted deletions must follow defined cascade or null-setting rules. | Class, Enrollment, StudentGrade, RevisionRequest and audit actors. | RESTRICT protected links; CASCADE enrollment/assessment → grades → requests; SET NULL reviewed_by. |
| 17 | Users must access only role-authorized records and actions. Protected operations must validate applicable role, ownership and account status. | Role-based screens and service operations. | Backend: applicable access and account-status checks. |
