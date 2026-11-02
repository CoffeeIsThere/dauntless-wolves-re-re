# Screenshot test pack — 2 November 2026

## Important distinction

Professor_Imports contains actual supported import CSVs. Admin_Manual_Entry_References contains reference sheets for typing test data into admin forms. The admin sheets are NOT import files: the current admin account/class forms do not implement CSV upload. No application or database changes are included in this pack.

## Before testing

Use a test database or make a backup. Imports create real records and audits. Never delete real school accounts for a screenshot. Sample professor-import files require the existing Active student IDs/names from your supplied sample SQL database. Check IDs and names against your current database and adjust the files if needed. CSV import enrolls existing students; it does not create accounts. Unknown-ID rejection tests only apply if that ID does not already exist.

## Recommended order

1. Read Professor_Imports/IMPORT_INSTRUCTIONS.md. Start with P01, P02, P03, P04 and P05 in Test_Cases_To_Complete.csv.
2. Use separate new class offerings for P06/P07 and rejection tests. Use an unused course code/section if a sample offering already exists.
3. Assessment-only imports add another assessment set each time: import successful files once unless deliberately testing repeated creation.
4. Run the manual admin tests with the NOT_FOR_IMPORT reference sheets. Create test professors before assigning them to classes. Choose private test passwords; none are supplied in these reference files.
5. Fill Actual Result, Pass/Fail, Tester, Date and Application Commit only after testing. These are proposed screenshot cases, not claims of successful execution on your computer.

## Screenshot guidance

Capture the relevant file preview, the outcome/error and the saved record after reopening where applicable. Name files P01_preview.png, P01_saved.png, P09_rejected.png, A03_saved.png, etc. A dialog saying success alone does not establish persistence. For rejection tests, confirm no partial records appeared; the message alone does not prove rollback. Do not expose passwords or database credentials.

Place real screenshots in 04_TestEvidence/Screenshots, copied input files in 04_TestEvidence/CSV_Test_Files, completed test sheet in 04_TestEvidence, and exported reports in 04_TestEvidence/Sample_Exported_Reports. The reference CSVs can be stored under Manual_Test_References to distinguish them from imported files.

## Expected import schemas

Class roster: student_id,full_name,Quiz 1 /20,Quiz 2 /20,Quiz 3 /30
Enrollment only: student_id,full_name
Assessment definitions: type,group,max_score,term,weight

Keep headers unchanged. A blank score is ungraded, not zero. Semester/year identify the offering; Midterm/Finals identify assessment periods. The application uses relative weights and does not implement base 50 or a mandatory equal term average.
