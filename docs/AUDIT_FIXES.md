# Full application audit fixes

The application uses GradeCalculator.CalculateRunningGrade over raw scores and relative assessment weights. Ungraded/zero-weight assessments are excluded; the remaining weights are normalized. Gradebook and student standing/performance now use this policy, and professor summaries derive their values from current scores rather than trusting old cached grades.

The older worksheet-only attendance, dropped, bonus, project-percentage and separate exam inputs are hidden. They did not represent saved assessments and used a conflicting fixed-weight/transmuted formula. Actual Attendance, Project and Exam assessments remain available as database-backed columns. The assessment dialog's display grouping is automatic; it does not assign an extra grading contribution.

Other fixes:

- Student grade history reads only StudentGrade audit entities belonging to the student; numeric IDs from unrelated entities cannot produce history rows.
- Login preserves password whitespace and prioritizes the selected student/teacher role. Admin login remains available with either radio selection.
- Mutating grading, review, enrollment and user-account workflows check account availability against the database. Deactivated sessions cannot continue those writes or change their password.
- Add Student CSV imports use `student_id,full_name`, validate registered active identities, create actual enrollments and pending grades, and write audit entries in one transaction. Existing enrollments are preserved. The MySQL option refreshes configured class enrollments.
- Gradebook PDF export uses the same paginated visible-column exporter as other reports.
- All four admin dashboard counts refresh from database totals alongside the Updates feed.
- Student course labels include section, semester and year. Performance selection filters by class ID rather than the course title.
- An empty professor class clears the previous student's score card. Zero grades remain distinct from missing grades in analytics.

No schema migration is required. Existing assessment scores are not rewritten by these fixes. The expected impact is corrected displayed averages, statuses and histories where the old implementations disagreed.

58 backend integration checks passed on a disposable database restored from the supplied SQL dump. The application and Windows navigation harness cross-build with zero warnings/errors. Linux cannot run Windows Forms or Microsoft Print to PDF: perform the Windows checks in validation/README.md before merging.
