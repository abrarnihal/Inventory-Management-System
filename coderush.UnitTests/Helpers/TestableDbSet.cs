using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace coderush.Controllers.Api.UnitTests;

/// <summary>
/// A testable DbSet implementation that wraps a List and properly
/// implements IQueryable for use in unit tests. This avoids the
/// Moq/Castle.DynamicProxy issue where explicit IQueryable interface
/// members on DbSet cannot be intercepted in .NET 10 / EF Core 10.
/// </summary>
internal class TestableDbSet<T> : DbSet<T>, IQueryable<T>, IEnumerable<T> where T : class
{
    private readonly List<T> _data;
    private readonly IQueryable<T> _queryable;

    public TestableDbSet(List<T> data)
    {
        _data = data;
        _queryable = data.AsQueryable();
    }

    public TestableDbSet() : this([]) { }

    IQueryProvider IQueryable.Provider => _queryable.Provider;
    Expression IQueryable.Expression => _queryable.Expression;
    Type IQueryable.ElementType => _queryable.ElementType;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    public List<T> AddedEntities { get; } = [];
    public List<T> RemovedEntities { get; } = [];
    public List<T> UpdatedEntities { get; } = [];

    public override EntityEntry<T> Add(T entity)
    {
        AddedEntities.Add(entity);
        _data.Add(entity);
        return null!;
    }

    public override EntityEntry<T> Remove(T entity)
    {
        RemovedEntities.Add(entity);
        _data.Remove(entity);
        return null!;
    }

    public override EntityEntry<T> Update(T entity)
    {
        UpdatedEntities.Add(entity);
        return null!;
    }

    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => null!;
}
