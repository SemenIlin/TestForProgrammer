public interface IFactory<T> where T : Player
{
    T Get(SpecializationType specialization);
}
