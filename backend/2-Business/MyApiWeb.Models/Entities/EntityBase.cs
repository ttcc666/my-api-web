using SqlSugar;

namespace MyApiWeb.Models.Entities
{
    public abstract class EntityBase
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "F_CreationTime")]
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 创建者ID
        /// </summary>
        [SugarColumn(ColumnName = "F_CreatorId")]
        public Guid CreatorId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SugarColumn(ColumnName = "F_LastModificationTime", IsNullable = true)]
        public DateTimeOffset? LastModificationTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 最后修改者ID
        /// </summary>
        [SugarColumn(ColumnName = "F_LastModifierId", IsNullable = true)]
        public Guid? LastModifierId { get; set; }

        #region 扩展字段

        /// <summary>
        /// 扩展字段1 - 通用字符串字段
        /// </summary>
        [SugarColumn(ColumnName = "F_Extend1", Length = 500, IsNullable = true)]
        public string? Extend1 { get; set; }

        /// <summary>
        /// 扩展字段2 - 通用字符串字段
        /// </summary>
        [SugarColumn(ColumnName = "F_Extend2", Length = 500, IsNullable = true)]
        public string? Extend2 { get; set; }

        /// <summary>
        /// 扩展字段3 - 通用字符串字段
        /// </summary>
        [SugarColumn(ColumnName = "F_Extend3", Length = 500, IsNullable = true)]
        public string? Extend3 { get; set; }

        /// <summary>
        /// 扩展字段4 - 通用字符串字段
        /// </summary>
        [SugarColumn(ColumnName = "F_Extend4", Length = 500, IsNullable = true)]
        public string? Extend4 { get; set; }

        /// <summary>
        /// 扩展字段5 - 通用字符串字段
        /// </summary>
        [SugarColumn(ColumnName = "F_Extend5", Length = 500, IsNullable = true)]
        public string? Extend5 { get; set; }

        /// <summary>
        /// 扩展整数字段1 - 通用数值字段
        /// </summary>
        [SugarColumn(ColumnName = "F_ExtendInt1", IsNullable = true)]
        public int? ExtendInt1 { get; set; }

        /// <summary>
        /// 扩展整数字段2 - 通用数值字段
        /// </summary>
        [SugarColumn(ColumnName = "F_ExtendInt2", IsNullable = true)]
        public int? ExtendInt2 { get; set; }

        /// <summary>
        /// 扩展整数字段3 - 通用数值字段
        /// </summary>
        [SugarColumn(ColumnName = "F_ExtendInt3", IsNullable = true)]
        public int? ExtendInt3 { get; set; }

        /// <summary>
        /// 扩展日期字段1 - 通用时间戳字段
        /// </summary>
        [SugarColumn(ColumnName = "F_ExtendDate1", IsNullable = true)]
        public DateTimeOffset? ExtendDate1 { get; set; }

        /// <summary>
        /// 扩展日期字段2 - 通用时间戳字段
        /// </summary>
        [SugarColumn(ColumnName = "F_ExtendDate2", IsNullable = true)]
        public DateTimeOffset? ExtendDate2 { get; set; }

        #endregion
    }
}
