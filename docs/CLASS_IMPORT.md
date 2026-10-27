# Import a class into the gradebook

Click **Add Class**, then **Import → From file (CSV)**. Select your roster CSV, enter the course code, course name, section, semester and school year, then click **Add**. Choose Midterm or Finals for the imported assessments and confirm the import.

CSV format:

```csv
student_id,full_name,Quiz 1 /20,Quiz 2 /20,Quiz 3 /30
25-00001,Student Name,18,,25
```

Replace the example ID and name with an existing active student from Admin. Names must match their registered names, ignoring case. Quote names containing commas: `"Surname, Firstname"`. Do not include multiline fields. Student accounts must be registered in Admin first; the import does not create accounts or passwords.

Each column after `full_name` creates an assessment. `/20` means a maximum score of 20. Blank cells stay ungraded. Scores must be between zero and that column's maximum. Duplicate student IDs and assessment titles are rejected.

Quiz, Exam and Project titles are classified accordingly; other assessment titles become Activity. Assessments receive equal relative weight (10 each), and all belong to the term selected at import time. Use the separate Add Assessment workflow when you need different weights.

The class is assigned to the logged-in professor. A new section requires its own unique combination of course code, section, semester and academic year. This workflow creates a new class; it does not overwrite an existing class. The database stores a section, not a timetable, so the dialog asks for Section rather than Schedule.

The complete import is transactional: a validation or database failure rolls back the course, class, enrollments, assessments, scores and audit changes. Cancelling the dialog does not save the staged CSV.

Use **Import → Refresh assigned classes from MySQL** to reload existing class assignments instead of creating a new one.
