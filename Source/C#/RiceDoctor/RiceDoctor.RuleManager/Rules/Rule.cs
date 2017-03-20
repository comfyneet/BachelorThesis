namespace RiceDoctor.RuleManager
{
    public abstract class Rule
    {
        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();
    }
}