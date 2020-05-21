namespace CallOfCthulhu
{
    /// <summary>
    /// 角色属性的模型 (Characteristic)
    /// </summary>
    public class Trait
    {
        private string name;
        private string formula = "3D6";
        private bool derived = false;
        private int upper = 0;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// 生成公式
        /// </summary>
        public string Formula { get => formula; set => formula = value; }

        /// <summary>
        /// 是否为派生属性
        /// </summary>
        public bool Derived { get => derived; set => derived = value; }

        /// <summary>
        /// 上限
        /// </summary>
        public int Upper { get => upper; set => upper = value; }

        /// <summary>
        /// 属性数值的段落
        /// </summary>
        public enum Segment
        {
            /// <summary>
            /// 初始值/基础值
            /// </summary>
            INITIAL,
            /// <summary>
            /// 调整值
            /// </summary>
            ADJUSTMENT,
            /// <summary>
            /// 成长值
            /// </summary>
            GROWTH,
        }
    }
}
