namespace InsightApp.Entities
{
    public class MemberListReport
    {
        public int? MemberId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public bool? RecievesEmails { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AccountId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string RecievesEmailsText => RecievesEmails.HasValue? (RecievesEmails.Value ? "Yes" : "No") : "Unknown";


    }
}

