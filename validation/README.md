# Professor service integration checks

Run only against a disposable database populated with the supplied grading-system SQL dump. These checks create an assessment and change a student's score. Never point them at a live database.

1. Build: `dotnet build validation/professor-services.vbproj`.
2. Place `dbconfig.json` next to the generated `validation/bin/Debug/net8.0/professor-services.dll`, using DatabaseHelper's Host, Port, Database, User and Password fields. Do not commit credentials.
3. Run: `dotnet validation/bin/Debug/net8.0/professor-services.dll`.

211 checks exercise CSV import validation, request approval/status transitions, re-check score overrides, duplicate approval protection, and assessment ownership, term validation, assessment creation, score saving, range validation, rejected clearing, persisted values, grade-sheet columns and audit queries. The harness compiles the actual grading services for Linux without loading Windows Forms. Its session stub is used only by the test executable.

Windows manual checks: switch between sections of the same course; confirm assessment titles and maximum scores match each section; edit and paste scores, reopen and verify persistence; enroll an existing student; create a weighted Midterm/Finals assessment; approve an Under Review request; inspect audit timestamps and exported class headers. Gradebook inputs now represent persisted assessment scores, and summaries use the shared weighted calculator. The old attendance/bonus/project/exam worksheet-only inputs are hidden to avoid conflicting grades. Removing a saved grade or changing an existing assessment maximum is explicitly blocked on this screen rather than silently changing only the display.


Additional Windows review:

- At 1366×768 and 125% display scaling, verify Student/Assessment/Score labels are separated from their inputs, all score rows are accessible, and the gradebook toolbar wraps without clipping Revision or PDF buttons.
- Open Add Assessment from the dashboard; confirm the correct section is selected. Create an assessment and return to the dashboard; verify it is refreshed.
- Import CSV with header `type,group,max_score,term,weight` and example row `Quiz,Quiz,20,Midterm,10`. All rows are validated before confirmation. All assessments, their pending grade rows and audit entries are committed in one transaction; a database failure rolls back the entire file. The group column is retained for file compatibility; worksheet grouping follows the assessment type.
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


Gradebook request chooser: click Revision Request and confirm a list opens, initially filtered to Open requests for the selected class. Test All my classes, student/assessment search, each status filter and Refresh. Select any Pending or Under Review row and click Review selected or double-click it. Verify the same styled decision dialog appears in both the gradebook and professor Clarification tab, including the student reason, corrected-score validation and response field. Invalid input or a service error must leave the dialog open, and Cancel must not change the request. Approved/Closed rows remain browseable but cannot be reviewed again. After a successful decision, the chooser refreshes; after closing it, the gradebook reloads scores. Verify the login title reads The Dauntless Grading System — Log in.


Gradebook navigation regression (Windows only): build `dotnet build validation/gradebook-navigation.vbproj`. Place a `dbconfig.json` pointing to a disposable seeded database beside `validation/bin/Debug/net8.0-windows/gradebook-navigation.dll`, then run `dotnet validation/bin/Debug/net8.0-windows/gradebook-navigation.dll`. This opens the real gradebook, creates a test assessment, edits it through the DataGridView editor, moves CurrentCell to reproduce the original reentrancy path, checks row identity and canonical weighted grade calculation, and requests a structural reload during an active edit. It writes test grades and requires Windows Desktop Runtime; compilation on Linux does not execute these checks.

Also manually verify Enter, Tab, mouse navigation, invalid scores, clearing a saved score, clipboard paste (including a failed cell), and switching classes while an edit is active. Confirm rejected edits restore their previous values without clearing the worksheet, selection/scroll position survive ordinary saves, and the last edit belongs to its original class. Full structural reloads, such as changing class, intentionally rebuild the selected worksheet.


Full audit fixes (Windows checks):

- Compare the same class/student across Gradebook, My Grades standing, Class Grades, and My Performance with unequal assessment weights and a below-passing fractional score. All overall grades must use raw scores and the shared weighted calculator; weights are relative and ungraded items are excluded. Verify a legitimate zero shows 0.0, while no grades shows --/Not Yet.
- Gradebook no longer shows the incompatible fixed-weight attendance, bonus, project and exam worksheet inputs. Real Exam/Project assessments remain editable in their own assessment columns. Test the navigation runner against the new canonical total.
- Switch between offerings of the same course in the student selectors; labels include section, semester and year, and performance filters by class ID.
- Import students through Add Student → Import CSV using `student_id,full_name`. Verify enrollment, grade placeholders and audit entries persist after reopening; unknown IDs/name mismatches must roll back. The MySQL option refreshes actual enrollments from dbconfig.json. No separate hardcoded gradebook database is used.
- Export a large gradebook to PDF and check that all students appear across numbered pages and visible columns remain within the page. Requires Microsoft Print to PDF on Windows.
- Create a student/professor or change pending-request/class counts from another session and verify all four admin dashboard tiles refresh alongside Updates.
- Select a professor class with no students and confirm the right-hand score card clears.
- Test role-specific login with a shared email/password and leading/trailing password spaces. Deactivate an account while it is logged in and verify grade, request, enrollment and password writes are rejected.

The backend suite exercises grade precision/weights, history entity isolation, deactivation, role collision, whitespace-preserving authentication, persistent/idempotent roster imports, and fresh dashboard counts. UI navigation, display scaling and Windows printer behavior still require the manual checks above.


Admin account controls:

- Manage Students/Professors: verify Edit remains available and Delete is always labeled Delete for both active and inactive accounts. Cancel the deletion confirmation and confirm no account changes; confirming deletion of a new unlinked account must remove its database row and add an admin audit entry.
- Try deleting accounts with enrollments, assigned classes, reviewer links, or actor audit history. Deletion must be blocked with an explanation; linked grades/classes/audits must remain untouched. Use the status badge to deactivate these accounts instead.
- Click the status badge to switch Active → Inactive → Active on both pages. Check the pill colors, database state and login behavior. No action should fire from a header, right click or empty cell padding.
- Click Full-time/Part-time in Manage Professors twice and verify both changes persist and appear in the audit log.
- Test these actions on a later pagination page and with filters active. The current page should remain selected when possible and clamp to the last remaining page after deletion.
- Login: focus the username field and press Enter; repeat from the password field. Both must use the same login path as the Log in button, including validation and role selection.

Backend tests cover unauthorized status/employment/deletion calls, bidirectional badge mutations, persistent employment values, linked-account deletion protection, real deletion of unlinked student/professor records, repeat deletion, and retained admin audit entries. Windows interaction checks remain manual.

Fast professor score entry (Windows only): build `dotnet build validation/professor-score-entry.vbproj`. Place a `dbconfig.json` pointing to a disposable seeded database beside `validation/bin/Debug/net8.0-windows/professor-score-entry.dll`, then run `dotnet validation/bin/Debug/net8.0-windows/professor-score-entry.dll`. The runner requires a class with at least two enrolled students, creates an isolated assessment, and checks Enter submission, correct student persistence, success-only advancement, retained class/assessment selection, cleared input focus, repeated Enter on an empty box, last-student completion without wrapping, and same-class refresh preservation. It must run on Windows; Linux cross-building does not execute these interaction checks.

Manually check the Record Scores page:

- Select a class, an assessment, and a student. Enter a valid score and press Enter. Verify the score is saved for that student, the next dropdown entry is selected, and the caret remains in the empty Score box. Repeat using Submit and Recalculate.
- Verify normal successes show inline feedback without a popup. Hold/repeat Enter after a save with the Score box empty: no extra save, popup or advancement should occur.
- Submit nonnumeric, negative, out-of-range and nonfinite values. Dismiss the warning; the same student/class/assessment and entered text must remain, with focus in Score for correction.
- Temporarily disconnect the database or deactivate the professor account and submit. A failed save must not advance or clear the entry. Reconnect/reactivate before further testing.
- Save for the last student. The dropdown must stay on that student and the feedback must read All students completed; the empty Score box remains focused. The professor may manually select another student or assessment afterward.
- Switch away and back to the professor window, and dismiss a validation warning, while positioned midway through the roster. A same-class refresh must preserve the student and assessment rather than reset to the first entries.


Surname sorting: Record Scores students, Class Grades rows, and gradebook rows use the shared surname ordering. Keep original display names. Verify names such as Jasmine Faye Catapang, Kyla Mae Dimaculangan and Elijah Rebolledo are ordered by Catapang/Dimaculangan/Rebolledo rather than their first names. Equal surnames use given names and then student IDs as tie breakers. Verify Enter-to-submit follows this new order while retaining the class/assessment, and clicking the Class Grades Student header restores surname A–Z after another column sort.

The schema has only full_name, so the sorter supports First Last and Last, First formats, common surname particles and suffixes. Ambiguous multiword surnames should use the explicit Last, First format (e.g. Garcia Lopez, Juan) rather than relying on surname inference. No names are rewritten and no database migration is needed.


Final review fixes:

- Add Student now chooses an active registered student (ID plus canonical name) and the exact destination class. Choose a different class than the gradebook behind the dialog and verify the enrollment is created only in the chosen class. The gradebook switches to that class after success. CSV import remains available in the dialog.
- Add Class must allow an existing course code with a different section, semester or year. An exact duplicate offering is still rejected by the database constraint.
- Import assessments named Quiz 1 and Quiz 3, then add another quiz. The generated title must be unused (Quiz 2); duplicate titles must be rejected even if another window created them after the form loaded.
- Scores, assessment maxima, weights and requested points support at most two decimal places. Enter 15.555: it must be rejected and must not advance the student or alter the old score. Enter 15.55 and verify all displays agree after reload.
- Clipboard paste validates every editable target first and commits the entire batch in one transaction. Test a valid first score followed by an out-of-range or unknown assessment/student entry. No score or audit entry from the failed batch may persist. Valid batches update existing grid cells without rebuilding rows.
- Type an unsaved professor score, then manually change student, class or assessment. The draft must clear with an explanatory inline message. A same-class catalog refresh must preserve a draft when the actual student and assessment IDs have not changed; invalid submission retains the draft.
- In another window/session, enroll a student or assign a class. Return to the student/professor window or select another tab and verify the catalog refreshes without logging out. Existing selections stay on the same IDs if they remain available. Reopen an already-open gradebook from the professor window and verify new assignments are available there too.
- Professor class labels and Class Grades headings include section, semester and year. Check both short and long labels in the widened dropdown.

The backend suite now contains 211 checks, including failed-paste score/audit rollback, precision rejection, duplicate-title protection, destination-aware enrollment and refreshed catalog query results. Both Windows interaction harnesses compile, but must be run on Windows.


Gradebook names and Add Student design: gradebook cells and the enrollment picker's display labels show surname first, for example `Puyat, Ron Jacob` and `Cruz, Juan A.`. Complete given names remain intact because the database has no separate middle-name field. Existing initial letters gain a period, already-formatted names are not reordered twice, and compound surnames/suffixes are preserved. Stored full_name and student ID remain unchanged.

On Windows, compare Add Student with Add Class/Add Assessment: it should have no native title bar, a dark green header with Import, white body, gray labels with green underlines, and matching Cancel/Enroll buttons. Check long dropdown values, 125% scaling, inline missing-selection validation, Escape/cancellation, CSV import, and enrollment into a class different from the one behind the popup.

## Admin workspace improvements

The service suite now passes 211 checks, including administrative bulk-status transactions and rollback, duplicate selections, authorization, linked-record counts, safe account detail reads, and audit filtering (dates, actor, action, literal wildcard text, entity and account activity). The audit viewer exports its displayed result set; PDF exports wrap complete values across pages.

Windows interaction harness: `dotnet build validation/admin-workspace.vbproj -p:EnableWindowsTargeting=true`, then run the generated executable against a disposable seeded database with its own local `dbconfig.json`. It checks background refresh, selection across pages, page/current-row preservation, status and employment filters, account detail tabs, and audit search/export availability. This harness requires Windows Forms and cannot run on Linux.

Manual Windows review at 100% and 125% scaling:

- Manage Students / Professors: search name, ID, email, program or department; use Clear, Status, Employment, Reset and Refresh. Filters wrap on narrow windows. Check long values, empty results, and the footer loading/error status.
- Select several accounts on different pages, including accounts hidden by a filter. Confirm the bulk bar counts all selected accounts, cancellation changes nothing, Activate/Deactivate changes only confirmed accounts, and each changed account has an audit entry. There is no bulk delete.
- Double-click an account name for Profile, Enrolled/Assigned classes and Recent activity. Double-click activity to read its full before/after/details fields. Password hashes must never appear.
- Delete an account with links: verify exact enrollment, grade, request, assigned-class and actor-audit counts as applicable, then test cancel and the offered Mark Inactive action. An unlinked account requires a separate permanent-deletion confirmation.
- Click the dashboard totals to open Students, Professors, the Class directory or open revision requests. Click an Updates row for full details. Dashboard updates continue refreshing automatically; Refresh retries failures.
- Audit viewer: combine actor/action/search/date filters, clear optional date checkboxes, use Reset and Refresh, and try reversed dates. Errors retain old results but disable export until a successful reload. CSV/PDF exports use the loaded results and their displayed sort order; PDF includes the applied filter description and full long values across pages. Edit a filter without applying it and verify export still matches the displayed results.
- Edit or toggle an account while on page 2 with filters applied. Refresh preserves the page (clamped if it no longer exists), current account/cell, scroll and bulk selections. Returning from another window refreshes the active account directory.


## Final submission fixes

The latest service run passes 211 checks. Added coverage includes strict consecutive academic years, distinct semester/year labels in Admin Quick Grade Entry, student request enrollment filtering and ownership, and horizontal PDF column groups with repeated student identity columns.

`dotnet build validation/final-submission.vbproj -p:EnableWindowsTargeting=true` builds the new Windows interaction checks. Run its executable against a disposable database with a local dbconfig.json. It checks student decision/response, performance and history refresh; Admin Add Class saves selected dates; inline read-error recovery; and wide/long report pagination using bitmap graphics without sending a printer job. These Windows interaction checks are compiled on Linux but must be executed on Windows.

Before handing off the application, check these flows on Windows:

1. Open a gradebook for class A, open Add Student, select class B, and import a `student_id,full_name` CSV. Verify new enrollment exists only in B, the gradebook selects B after success, and cancellation changes nothing. The Import refresh option must also select the class chosen in the popup.
2. Admin Add Class: select 2nd Semester and a new consecutive academic year, save and verify both values. Try `abcd-efgh`, equal years and skipped years; all must be rejected. Review the two new fields at 100% and 125% scaling.
3. Admin Quick Grade Entry: create the same course/section in another semester/year; both dropdown entries must identify their complete offering.
4. Student Request Clarification: choose different courses and verify requests follow the selection and identify their course/section/semester/year. Approve a request in a separate professor session, return to the student window and verify status and response update. Repeat with an open Performance or Grade History page after changing a score.
5. Export a class with many assessment columns and long student names. Each PDF column group must repeat student identification, preserve every assessment, wrap headings and values, and continue long rows across pages. Also test long professor audit details. Use Microsoft Print to PDF for the final saved-file check.
6. Disconnect the test database while switching professor classes, student courses or loading Admin requests. The form should stay open with an inline error and Retry. Reconnect and retry. Admin Add Class must explain a failed professor lookup and keep Save disabled until a successful load.


## Admin clarification request overview

Admin View Requests and the Pending Requests dashboard tile now open `AdminRequestsForm`. The standard button starts at All; the tile starts at Open (Pending + Under Review). This is read-only oversight; professors keep decision and score authority.

The service suite passes 211 checks, including request date normalization, newest-first ordering, requested-date ranges, exact course filters, student ID/name search, literal wildcard escaping, combined filters, access validation and professor-response retrieval.

Windows interaction checks: build `validation/admin-requests.vbproj -p:EnableWindowsTargeting=true` and run against a disposable seeded database with a local dbconfig.json. Checks cover dates, read-only cells, unchanged-refresh stability, selected request preservation, full reasons, filtered/sorted export snapshots, CSV timestamps, empty results, invalid-date recovery, Open-only navigation, and long text report pagination without sending a print job. The harness is compiled on Linux; execution requires Windows.

Manual Windows review:

- Open Admin → View Requests. Resize/maximize at 100% and 125% scaling. Request dates should be readable, the table should scroll horizontally when needed, and both full-reason/response areas should remain accessible with their own scrollbars.
- Select an Approved or Closed request. Verify its complete reason, response, current score, requested adjustment, assigned professor, reviewing professor, request date and review date. Historical missing review information must say Not recorded rather than inventing a date or reviewer.
- Apply Student, Status, Course/section and requested-date filters individually and together. Enter applies filters; Reset returns to All. Course labels include semester/year. Through includes the entire selected day.
- Click the Pending Requests tile; only Pending and Under Review should appear. Closed is distinct from Under Review by text as well as color.
- Change a request through a separate professor session. Within five seconds, the admin table should update while retaining the selected request, current sort and scroll when the request still matches the filters. Unchanged refreshes should not rebuild the table or interrupt reading notes.
- Disconnect the disposable database and Refresh. Existing results should remain readable, Retry should appear, and exports should remain disabled until a successful load. Reconnect and retry. Close the window while a load is pending; it should not reopen or show a disposed-control error.
- Sort and filter, then export CSV/PDF. Both must contain only the displayed requests in the displayed order, plus full reasons/responses and request/review dates. PDF pages repeat the filter description and include page numbers; long records must continue without ellipses. Canceling Save must create no file. Editing filters without applying them must leave exports tied to the displayed result set.


## Final four reliability fixes

The portable suite now passes 211 checks. It deliberately rejects a later assessment insert and an audit insert using temporary triggers in the disposable database. The tests verify rollback of every assessment, pending grade and audit entry; retry produces exactly the intended batch; repeated types get distinct titles under the class lock. Password-change tests verify that an audit failure leaves the original password hash untouched, the old password still works, and a successful retry commits exactly one audit entry. Test triggers are removed in Finally blocks.

The admin request interaction harness also checks that a failed manual filter remains blocked from automatic refresh until a successful manual load. Rebinding the grid suppresses intermediate detail clearing, and unchanged detail text keeps its native selection and scroll position.

Windows handoff checks:

- Import an assessment-only CSV containing multiple types and repeated quiz rows. Confirm the complete file saves, the intended titles appear, and each student has matching pending grade rows. Cancellation saves nothing.
- Change a student's password, log out, and verify that only the new password works and one Changed password entry appears in Admin Audit Logs.
- Admin Requests: apply a reversed date range, wait more than five seconds, and confirm the validation error/Retry remain visible and exports stay disabled. Correct the dates and apply/retry; normal refresh and exports should resume.
- Scroll/select text in a selected request's reason and response. Update a different request in a separate professor session; when the admin table refreshes, the selected request and unchanged note positions should remain intact. Selecting a different request should start its details at the top.

The database suite has executed on Linux. All five Windows interaction projects compile; their runtime, display scaling and final Microsoft Print to PDF checks require Windows.
