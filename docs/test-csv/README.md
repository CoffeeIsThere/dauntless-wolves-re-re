# CSV test files — The Dauntless Grading System

Use these files with test classes. Imports save real records and audit entries.

## Before starting

- The IDs and names come from the SQL sample database supplied with this project. Confirm that these accounts exist, remain Active, and have the same registered names in Admin. If you changed or removed them, update the CSVs to match your database.
- Student accounts must already exist. These files enroll registered students; they do not create accounts or passwords.
- Log in as a teacher. Imports apply to classes owned by that teacher.
- Keep the headers unchanged. Save edited files as CSV, not XLSX. Blank scores mean ungraded; zero means a recorded zero.

## Recommended order

1. Open Digital Gradebook → Add Class → Import → From file (CSV). Choose `valid/01_class_three_quizzes.csv`.
2. Enter a new test offering, for example course code `CSVTEST01`, name `CSV Import Test`, section `TEST-A`, semester `1st Semester`, academic year `2026-2027`. Click Add, choose Midterm for the imported assessments, and confirm. If that offering already exists, choose another test code or section.
3. Expect six enrolled students and three quiz assessments. Check the maximum scores (20, 20, 30), decimals, a recorded zero, and blank/pending scores. The class-roster import gives assessments equal relative weights of 10.
4. In the same test class, open Add Student, verify the destination class, then Import → From file (CSV) and select `valid/03_enroll_registered_students.csv`. Expect three additional students with pending grades. Re-importing this roster should preserve existing enrollments and add no duplicates.
5. Use Add Assessment → Import → From file (CSV) with files 04 and 05. Expect four assessments per file with the listed grading periods and relative weights. Titles are generated automatically; existing quiz/activity/exam numbers continue. Re-importing a successful assessment-only file adds another set of assessments, so normally import it only once.
6. Try files 02 and 06 through Add Class → Import, using separate NEW test offerings. File 02 exercises different assessment types; file 06 creates only ungraded/pending results.

## Valid files

| File | Import from | Expected result |
|---|---|---|
| 01_class_three_quizzes.csv | Add Class → Import | 6 students; 3 quizzes; decimals, zeros and blanks retained |
| 02_class_mixed_assessments.csv | Add Class → Import | 6 students; Quiz, Activity, Project and Exam columns |
| 03_enroll_registered_students.csv | Add Student → Import | Enrolls 3 additional registered students; existing enrollments are skipped |
| 04_assessments_midterm.csv | Add Assessment → Import | 4 Midterm assessments; relative weights total 100 |
| 05_assessments_finals.csv | Add Assessment → Import | 4 Finals assessments; relative weights total 100 |
| 06_class_blank_scores.csv | Add Class → Import | 3 students; 2 assessments; all scores remain pending |

The Midterm/Finals choice for a class-roster import applies to every assessment in that file. The semester and academic year identify the class offering; they are separate from assessment grading periods.

## Invalid files — expected to fail

Use invalid files 01–06 through **Add Class → Import** with a new test offering. Use files 07–09 through **Add Assessment → Import**. Some invalid class files are rejected when you click Add, after the CSV has been staged.

| File | Expected rejection |
|---|---|
| 01_duplicate_student_id.csv | The same student ID appears twice |
| 02_score_above_maximum.csv | Score 21 exceeds the quiz maximum of 20 |
| 03_name_mismatch.csv | Name does not match the registered student ID |
| 04_unknown_student_id.csv | Student 99-99999 is not registered in the supplied sample database |
| 05_extra_decimal_places.csv | Score 18.555 has more than two decimal places |
| 06_mismatched_columns.csv | A data row has more columns than the header |
| 07_unsupported_assessment_type.csv | Practical is not a supported assessment type |
| 08_wrong_assessment_term.csv | Assessment term must be Midterm or Finals, not 1st Semester |
| 09_zero_assessment_weight.csv | Assessment weight must be greater than zero |

After rejection, verify that no partial class, enrollment, assessment, score or audit changes were saved. Canceling an import should also save nothing.

Supported assessment types: Quiz, Activity, Exam, Project, Attendance. Maximums, scores and weights allow at most two decimal places. The `group` column is retained for import compatibility; the assessment type determines its display grouping.
