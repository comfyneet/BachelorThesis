using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public interface ISerializable
    {
        [NotNull]
        string Serialize();
    }
}