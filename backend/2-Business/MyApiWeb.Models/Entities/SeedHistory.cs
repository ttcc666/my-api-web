using SqlSugar;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 种子数据执行历史记录表
    /// </summary>
    [SugarTable("SeedHistory")]
    public class SeedHistory
    {
        /// <summary>
        /// 种子名称（主键）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string SeedName { get; set; } = null!;

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTimeOffset ExecutedAt { get; set; }

        /// <summary>
        /// 执行者
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ExecutedBy { get; set; } = "System";

        /// <summary>
        /// 备注信息
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Remarks { get; set; }
    }
}