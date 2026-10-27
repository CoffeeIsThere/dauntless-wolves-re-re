Public Class DashboardService
    Public Shared Function GetStatistics() As DataTable
        Return DatabaseHelper.ExecuteQuery("SELECT (SELECT COUNT(*) FROM student) AS students, (SELECT COUNT(*) FROM teacher) AS teachers, (SELECT COUNT(*) FROM class) AS classes, (SELECT COUNT(*) FROM revisionrequest WHERE status IN ('Pending','Under Review')) AS pending")
    End Function
End Class
