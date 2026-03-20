using Ardalis.Specification;

namespace HobomSpace.Application.Ports;

public interface IRepository<T> : IRepositoryBase<T> where T : class;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class;
