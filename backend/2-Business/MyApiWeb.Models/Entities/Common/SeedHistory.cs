using SqlSugar;

namespace MyApiWeb.Models.Entities.Common
{
    /// <summary>
    /// 种子数据执行历史记录表
    /// </summary>
    [SugarTable("Sys_SeedHistory")]
    public class SeedHistory
    {
        /// <summary>
        /// 种子名称（主键）
        /// </summary>
        [SugarColumn(ColumnName = "F_SeedName", IsPrimaryKey = true, Length = 100)]
        public string SeedName { get; set; } = null!;

        /// <summary>
        /// 执行时间
        /// </summary>
        [SugarColumn(ColumnName = "F_ExecutedAt")]
        public DateTimeOffset ExecutedAt { get; set; }

        /// <summary>
        /// 执行者
        /// </summary>
        [SugarColumn(ColumnName = "F_ExecutedBy", Length = 50)]
        public string ExecutedBy { get; set; } = "System";

        /// <summary>
        /// 备注信息
        /// </summary>
        [SugarColumn(ColumnName = "F_Remarks", Length = 500, IsNullable = true)]
        public string? Remarks { get; set; }
    }
}
