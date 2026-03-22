namespace MedicalResultsTracker.Services
{
    internal interface IRepository<T> where T : class, new()
    {
        T Get(int index);
        bool Remove(int index);
        void Add(T item);
    }
}
