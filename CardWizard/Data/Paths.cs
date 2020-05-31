using CallOfCthulhu;
using System.ComponentModel;

namespace CardWizard.Data
{
    /// <summary>
    /// 路径数据库, 缓存了应用的特殊路径
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// 存档的根目录
        /// </summary>
        [Description("存档所在目录")]
        public string PathSave = "./Save";

        /// <summary>
        /// 资源的根目录
        /// </summary>
        [Description("资源的根目录")]
        public string PathResources = "./Resources";

        /// <summary>
        /// 动态链接库的目录
        /// </summary>
        [Config.ProcessIndex(1)]
        [Description("动态链接库的目录")]
        public string PathLibs = "{PathResources}/Libs";

        /// <summary>
        /// 数据存放的根目录
        /// </summary>
        [Config.ProcessIndex(1)]
        [Description("数据存放的根目录")]
        public string PathData = "{PathResources}/Data";

        /// <summary>
        /// 技能数据位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("技能数据的存储位置")]
        public string FileSkills = $"{{PathData}}/{nameof(Skill)}.All.yaml";

        /// <summary>
        /// 职业数据的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("职业数据的存储位置")]
        public string FileOccupations = $"{{PathData}}/{nameof(Occupation)}.All.yaml";

        /// <summary>
        /// 武器数据的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("武器数据的存储位置")]
        public string FileWeapons = $"{{PathData}}/{nameof(Weapon)}.All.yaml";

        /// <summary>
        /// 脚本的目录
        /// </summary>
        [Config.ProcessIndex(1)]
        [Description("脚本的目录")]
        public string PathScripts = "{PathResources}/Scripts";

        /// <summary>
        /// 公式文件存储的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("公式文件存储的位置")]
        public string ScriptFormula = "{PathScripts}/formula.lua";

        /// <summary>
        /// 启动完毕脚本的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("启动脚本存储的位置")]
        public string ScriptStartup = "{PathScripts}/startup.lua";

        /// <summary>
        /// Debug 脚本的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        [Description("Debug 脚本存储的位置")]
        public string ScriptDebug = "{PathScripts}/debug.lua";

        /// <summary>
        /// 图片资源的位置
        /// </summary>
        [Config.ProcessIndex(1)]
        [Description("图片资源存储的目录")]
        public string PathTextures = "{PathResources}/Textures";
    }
}
