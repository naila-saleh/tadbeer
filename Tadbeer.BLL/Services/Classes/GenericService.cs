using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mapster;
using Tadbeer.BLL.Services.Interfaces;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes;

public class GenericService<TRequest, TResponse, TEntity> : IGenericService<TRequest, TResponse, TEntity>
    where TRequest : class
    where TResponse : class
    where TEntity : class
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IGenericRepository<TEntity> _repository;

    public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<TResponse?> GetByIdAsync(object id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.Adapt<TResponse>();
    }

    public async Task<IEnumerable<TResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Adapt<IEnumerable<TResponse>>();
    }

    public async Task<IEnumerable<TResponse>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = await _repository.FindAsync(predicate);
        return entities.Adapt<IEnumerable<TResponse>>();
    }

    public async Task<TResponse> AddAsync(TRequest dto)
    {
        var entity = dto.Adapt<TEntity>();
        await _repository.AddAsync(entity);
        return entity.Adapt<TResponse>();
    }

    public async Task<IEnumerable<TResponse>> AddRangeAsync(IEnumerable<TRequest> dtos)
    {
        var entities = dtos.Adapt<IEnumerable<TEntity>>();
        await _repository.AddRangeAsync(entities);
        return entities.Adapt<IEnumerable<TResponse>>();
    }

    public async Task UpdateAsync(object id, TRequest dto)
    {
        var targetEntity = await _repository.GetByIdAsync(id);
        if (targetEntity != null)
        {
            dto.Adapt(targetEntity);
            _repository.Update(targetEntity);
        }
    }

    public async Task RemoveAsync(object id)
    {
        // Safe fetch before remove (prevents stub tracking errors)
        var targetEntity = await _repository.GetByIdAsync(id);
        if (targetEntity != null)
        {
            _repository.Remove(targetEntity);
        }
    }

    public Task RemoveRangeAsync(IEnumerable<object> ids)
    {
        var stubs = new List<TEntity>();
        var idProperty = typeof(TEntity).GetProperty("Id");

        if (idProperty != null)
        {
            foreach (var id in ids)
            {
                var stub = Activator.CreateInstance<TEntity>();
                idProperty.SetValue(stub, id);
                stubs.Add(stub);
            }
            _repository.RemoveRange(stubs);
        }

        return Task.CompletedTask;
    }
}
