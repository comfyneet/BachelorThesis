namespace RiceDoctor.RuleManager
{
    public abstract class Rule
    {
        public abstract override int GetHashCode();

        public abstract override bool Equals(object obj);

        public static bool operator ==(Rule rule1, Rule rule2)
        {
            if (ReferenceEquals(rule1, rule2)) return true;
            if (ReferenceEquals(null, rule1)) return false;
            if (ReferenceEquals(null, rule2)) return false;
            return rule1.Equals(rule2);
        }

        public static bool operator !=(Rule rule1, Rule rule2)
        {
            return !(rule1 == rule2);
        }
    }
}