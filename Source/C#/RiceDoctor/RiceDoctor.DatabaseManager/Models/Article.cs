using System;

namespace RiceDoctor.DatabaseManager
{
    public class Article : IEquatable<Article>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public DateTime RetrievedDate { get; set; }

        public int WebsiteId { get; set; }
        public Website Website { get; set; }

        public bool Equals(Article other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Article) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(Article article1, Article article2)
        {
            if (ReferenceEquals(article1, article2)) return true;
            if (ReferenceEquals(null, article1)) return false;
            if (ReferenceEquals(null, article2)) return false;
            return article1.Equals(article2);
        }

        public static bool operator !=(Article article1, Article article2)
        {
            return !(article1 == article2);
        }
    }
}