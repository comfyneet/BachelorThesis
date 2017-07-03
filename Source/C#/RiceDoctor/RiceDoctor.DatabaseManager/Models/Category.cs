namespace RiceDoctor.DatabaseManager
{
    public class Category
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string ArticleXPath { get; set; }
        public string TitleXPath { get; set; }
        public string ContentXPath { get; set; }

        public int WebsiteId { get; set; }
        public Website Website { get; set; }
    }
}