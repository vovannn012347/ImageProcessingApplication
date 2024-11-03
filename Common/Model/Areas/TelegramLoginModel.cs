
namespace Common.Model.Areas
{
    public class TelegramLoginModel
    {
        public long Id { get; set; }
        //public string AuthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string PhotoUrl { get; set; } //no for now
        public string UserName { get; set; }
        //public string Hash { get; set; }
    }
}