using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Tadbeer.BLL.Services.Interfaces;

public interface IGenericService<TRequest, TResponse, TEntity>
    where TRequest : class
    where TResponse : class
    where TEntity : class
{
    Task<TResponse?> GetByIdAsync(object id);
    Task<IEnumerable<TResponse>> GetAllAsync();
    Task<IEnumerable<TResponse>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TResponse> AddAsync(TRequest dto);
    Task<IEnumerable<TResponse>> AddRangeAsync(IEnumerable<TRequest> dtos);
    Task UpdateAsync(object id, TRequest dto);
    Task RemoveAsync(object id);
    Task RemoveRangeAsync(IEnumerable<object> ids);
}
