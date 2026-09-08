namespace ZadElealm.Core.ServiceDto;

public sealed record CourseEnrollmentSummaryReadModel(
    int TotalEnrolledStudents,
    bool IsCurrentUserEnrolled);
