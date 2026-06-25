using System;
using System.Linq.Expressions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.Common.Specifications;

/// <summary>
/// Example specification that filters active products.
/// Assumes ProductReadModel has a `Status` or `IsActive` property.
/// </summary>
public sealed class ActiveProductSpecification : Specification<ProductReadModel>
{
    public override Expression<Func<ProductReadModel, bool>> ToExpression()
        => product => EF.Property<string>(product, "Status") == "Active"; // replace with actual property
}
