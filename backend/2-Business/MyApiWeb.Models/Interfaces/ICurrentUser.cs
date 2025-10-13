using System;

namespace MyApiWeb.Models.Interfaces
{
    public interface ICurrentUser
    {
        Guid? Id { get; }
    }
}