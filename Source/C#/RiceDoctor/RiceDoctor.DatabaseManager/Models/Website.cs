using System.Collections.Generic;

namespace RiceDoctor.DatabaseManager
{
    public class Website
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public List<Category> Categories { get; set; }

        public List<Article> Articles { get; set; }
    }
}