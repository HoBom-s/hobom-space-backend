using Ardalis.Specification.EntityFrameworkCore;
using HobomSpace.Application.Ports;
using HobomSpace.Infrastructure.Persistence;

namespace HobomSpace.Infrastructure.Persistence.Repositories;

public sealed class EfRepository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext), IRepository<T>, IReadRepository<T> where T : class;
