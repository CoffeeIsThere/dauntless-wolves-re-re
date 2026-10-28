# Student profile and professor password changes

Branch: `feature/student-profile-professor-password`, created from the latest `origin/main` on 29 October 2026. No database migration or new database fields are required.

## Student profile

Student → My Profile now opens a resizable green-and-white read-only dialog containing name, ID, email, program, year, section and account status. Cards show enrolled classes and open clarification requests (Pending + Under Review). A table lists each enrolled offering, its current weighted running grade and standing.

Each offering includes course, section, semester and academic year. Grades are recalculated from saved raw scores through the existing GradeCalculator. No cross-course average is introduced. Blank assessments are excluded; zero scores are included; a class without grades shows a dash and Not Yet. Opening or refreshing the dialog reads a fresh, student-scoped snapshot without updating academic records. Read failures provide Retry and preserve previously loaded results.

Windows checks:

1. Log in as a student and open My Profile. Compare personal details, class count, grades and open requests with the existing student screens.
2. Test a student without classes, a class without assessments, all-ungraded assessments, and a real zero. Ensure no other student's information is visible.
3. Change a score or review a request in another session, then click Refresh or reopen the profile. Verify updated figures.
4. Resize the window and use 100% and 125% scaling. Verify long names, email addresses and class labels, table scrolling, footer controls and Escape/Close.
5. Disconnect the test database, open or refresh the profile, then reconnect and Retry. The dialog should remain usable without invented zero counts.

## Professor password

Professor dashboard → Change Password (beside Log out) opens a styled dialog with masked current, new and confirmation fields. Enter submits; Escape or Cancel dismisses without saving. A successful change uses the existing AuthService transaction and audit entry. Password whitespace is preserved. The current policy requires at least four characters; this feature does not change that policy.

Windows checks:

1. Open the dialog and cancel. Verify no password change or audit entry.
2. Try missing current password, fewer than four characters, confirmation mismatch, the same old/new password, and incorrect current password. The dialog should stay open with feedback.
3. Save a valid new password. Log out, verify the old password fails and the new password works. Confirm one Changed password audit entry without password contents.
4. Deactivate the test professor after login, then try changing their password. The write must be rejected.
5. Check the dashboard button and dialog at 100% and 125% scaling, including narrower windows and long feedback messages.

## Executed validation

The application and portable regression project build with zero warnings and errors. The targeted regression executable passed 21 checks against a disposable database restored from the supplied SQL dump. Checks cover session isolation, empty summaries, weighted zero/missing scores, standing, offering identity, refreshed counts/details, no exposed password hash, wrong-password rejection, successful teacher login after changing password, old-password rejection, audit recording and inactive-teacher rejection.

Windows Forms runtime, visual scaling, keyboard interaction and final screen appearance still require the Windows checks above.

To run the backend checks, build `validation/profile-password.vbproj`, place a local `dbconfig.json` beside its output DLL and run that DLL. Use only a freshly restored disposable database: the test deliberately creates fixtures, changes a professor password/status, and edits requests and profile data. Never run it against actual school records.
