using CallOfCthulhu.Models;

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
        public string PathSave = "./Save";

        /// <summary>
        /// 资源的根目录
        /// </summary>
        public string PathResources = "./Resources";

        /// <summary>
        /// 数据存放的根目录
        /// </summary>
        [Config.ProcessIndex(1)]
        public string PathData = "{PathResources}/Data";

        /// <summary>
        /// 技能数据位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string FileSkills = $"{{PathData}}/{nameof(Skill)}.All.yaml";

        /// <summary>
        /// 职业数据的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string FileOccupations = $"{{PathData}}/{nameof(Occupation)}.All.yaml";

        /// <summary>
        /// 武器数据的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string FileWeapons = $"{{PathData}}/{nameof(Weapon)}.All.yaml";

        /// <summary>
        /// 脚本的目录
        /// </summary>
        [Config.ProcessIndex(1)]
        public string PathScripts = "{PathResources}/Scripts";

        /// <summary>
        /// 启动脚本的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string ScriptFormula = "{PathScripts}/formula.lua";

        /// <summary>
        /// 启动完毕脚本的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string ScriptStartup = "{PathScripts}/startup.lua";

        /// <summary>
        /// Debug 脚本的位置
        /// </summary>
        [Config.ProcessIndex(2)]
        public string ScriptDebug = "{PathScripts}/debug.lua";

        /// <summary>
        /// 图片资源的位置
        /// </summary>
        [Config.ProcessIndex(1)]
        public string PathTextures = "{PathResources}/Textures";
    }
}
