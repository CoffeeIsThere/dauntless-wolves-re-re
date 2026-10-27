# Professor service integration checks

Run only against a disposable database populated with the supplied grading-system SQL dump. These checks create an assessment and change a student's score. Never point them at a live database.

1. Build: `dotnet build validation/professor-services.vbproj`.
2. Place `dbconfig.json` next to the generated `validation/bin/Debug/net8.0/professor-services.dll`, using DatabaseHelper's Host, Port, Database, User and Password fields. Do not commit credentials.
3. Run: `dotnet validation/bin/Debug/net8.0/professor-services.dll`.

Thirty-two checks exercise CSV import validation, request approval/status transitions, re-check score overrides, duplicate approval protection, and assessment ownership, term validation, assessment creation, score saving, range validation, rejected clearing, persisted values, grade-sheet columns and audit queries. The harness compiles the actual grading services for Linux without loading Windows Forms. Its session stub is used only by the test executable.

Windows manual checks: switch between sections of the same course; confirm assessment titles and maximum scores match each section; edit and paste scores, reopen and verify persistence; enroll an existing student; create a weighted Midterm/Finals assessment; approve an Under Review request; inspect audit timestamps and exported class headers. Worksheet summary inputs remain local calculations, as described by the gradebook tooltip. Removing a saved grade or changing an existing assessment maximum is explicitly blocked on this screen rather than silently changing only the display.


Additional Windows review:

- At 1366×768 and 125% display scaling, verify Student/Assessment/Score labels are separated from their inputs, all score rows are accessible, and the gradebook toolbar wraps without clipping Revision or PDF buttons.
- Open Add Assessment from the dashboard; confirm the correct section is selected. Create an assessment and return to the dashboard; verify it is refreshed.
- Import CSV with header `type,group,max_score,term,weight` and example row `Quiz,Quiz,20,Midterm,10`. All rows are validated before confirmation. Each saved assessment is committed individually; a database failure reports the count saved before stopping. The group column is retained for file compatibility; worksheet grouping follows the assessment type.
- Check All/Midterm/Finals grade-sheet filters and column widths. RunningGrade remains the overall cached grade, while the term filter selects assessment columns.
- Click Review for both Pending and Under Review rows; test approve, corrected re-check score, mark under review, close with reason, and cancellation. Confirm the score and audit timestamp persist after reopening.
- Export audit logs and class grades with Microsoft Print to PDF enabled. Check headings, page numbers, long logs across pages, cancellation, and replacing an existing PDF. Export reports failure if no PDF is created within ten seconds and preserves the prior destination until a new PDF is available.
- Verify analytics captions remain visible and separate from their numbers.


Layout follow-up: verify the four analytics cards retain their entire rounded border beneath the captions. Dashboard Add Assessment must open only the assessment dialog, including CSV import, with no gradebook window appearing; cancelling must leave the dashboard active. Check clarification details, action-specific prompts, Cancel/Escape, and required corrected-score validation. At 1200px window width, gradebook buttons should remain compact and the meetings label/value should wrap as a single unit.


Automatic refresh checks on Windows:

- Keep Class Grades open and switch between CS 103 sections A and B. Verify rows and the class heading change immediately, and the current All/Midterm/Finals filter stays selected. Check a class without enrollments clears the previous rows.
- Leave the admin dashboard open and create a score or audit entry from another application instance. The Updates feed should show committed matching entries within roughly two seconds plus query time, with no tab switch.
- Select a feed filter, create both matching and nonmatching entries, and confirm the filter stays applied. Switch filters rapidly to verify an older query cannot overwrite the latest filter results.
- Return from an admin quick action and verify the dashboard refreshes. Interrupt database connectivity temporarily: background refresh should preserve displayed rows without repeated error dialogs, then recover after reconnecting. Close Admin during a refresh to confirm no disposed-control errors.


Student review responses: approve a re-check with a corrected score and a response; close another request with a reason. Log in as that student and select each request in My Request Status. Verify the review timestamp and response appear in the green-tinted panel, long responses scroll, and an older decision with no response displays a clear fallback. Verify another student cannot see these requests. Approval responses are stored in the existing transactional request audit entry; existing closure notes are read from those entries without a schema migration.

Performance bars: check 0%, 1%, 50%, and 100% at normal and 125% scaling. Each percentage should remain fully visible in its own space at the right of the track, independently of the fill width; all four tracks use the same modest height.


Class roster imports: follow docs/CLASS_IMPORT.md. Verify Add Class opens the styled dialog, CSV import stages data without writing anything until Add, and cancellation writes nothing. Check all three quiz columns appear with the imported roster and values, and reopening preserves them. Test unknown student IDs, name mismatches, duplicate sections and out-of-range scores. Verify the parent professor class list includes a newly created class.

Window lifecycle: open the gradebook from the professor form, close the professor form using X, and verify the gradebook closes too and the application exits when there are no other forms. Also check Logout closes the gradebook but leaves the login window usable. The owned gradebook does not add a second taskbar button. Verify the professor title is The Dauntless Grading System and the green navigation colors stay consistent when switching tabs.
