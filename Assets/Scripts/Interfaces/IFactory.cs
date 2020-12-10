public interface IFactory<T> where T : class 
{
    T Get(SpecializationType specialization);
}
