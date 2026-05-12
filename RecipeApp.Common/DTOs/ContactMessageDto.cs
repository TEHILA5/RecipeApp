namespace RecipeApp.Common.DTOs
{
    public class ContactMessageDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Category { get; set; } = "";
        public string? RecipeName { get; set; }
        public string Message { get; set; } = "";
        public string Urgency { get; set; } = "Normal";
    }

    public class AdminReplyDto
    {
        public string ToEmail { get; set; } = "";
        public string ToName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string ReplyContent { get; set; } = "";
    }
}