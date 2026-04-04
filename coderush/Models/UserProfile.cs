namespace coderush.Models
{
    public class UserProfile
    {
        public const string DefaultProfilePicture = "/upload/blank-person.svg";

        public int UserProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string OldPassword { get; set; }
        public string ProfilePicture { get; set; } = DefaultProfilePicture;

        public string ApplicationUserId { get; set; }
    }
}
