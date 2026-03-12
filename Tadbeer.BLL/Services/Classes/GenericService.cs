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

    public async Task<TResponse?> GetByIdAsync(params object[] ids)
    {
        var entity = await _repository.GetByIdAsync(ids);
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
        await _unitOfWork.CompleteAsync();
        return entity.Adapt<TResponse>();
    }

    public async Task<IEnumerable<TResponse>> AddRangeAsync(IEnumerable<TRequest> dtos)
    {
        var entities = dtos.Adapt<IEnumerable<TEntity>>();
        await _repository.AddRangeAsync(entities);
        await _unitOfWork.CompleteAsync();
        return entities.Adapt<IEnumerable<TResponse>>();
    }

    public async Task UpdateAsync(TRequest dto, params object[] ids)
    {
        var targetEntity = await _repository.GetByIdAsync(ids);
        if (targetEntity != null)
        {
            dto.Adapt(targetEntity);
            _repository.Update(targetEntity);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task RemoveAsync(params object[] ids)
    {
        var targetEntity = await _repository.GetByIdAsync(ids);
        if (targetEntity != null)
        {
            _repository.Remove(targetEntity);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task RemoveRangeAsync(IEnumerable<object[]> idsCollection)
    {
        foreach (var ids in idsCollection)
        {
            var targetEntity = await _repository.GetByIdAsync(ids);
            if (targetEntity != null)
            {
                _repository.Remove(targetEntity);
            }
        }

        await _unitOfWork.CompleteAsync();
    }
}
