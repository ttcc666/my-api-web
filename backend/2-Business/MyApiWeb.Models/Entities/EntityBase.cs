using SqlSugar;

namespace MyApiWeb.Models.Entities
{
    public abstract class EntityBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        public Guid CreatorId { get; set; }

        public DateTimeOffset? LastModificationTime { get; set; } = DateTimeOffset.Now;

        [SugarColumn(IsNullable = true)]
        public Guid? LastModifierId { get; set; }
    }
}