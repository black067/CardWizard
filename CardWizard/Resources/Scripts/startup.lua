-- 此脚本中的内容, 会在程序启动后被执行
-- print 方法用于输出信息, 被输出的文本会在界面下方的输出框中显示
print('欢迎使用 COC 角色编辑器 - 规则基于 第 6 版规则书.')

--- <summary>
--- baseModel 是一个数组, 元素类型为 DataModel
--- </summary
function GeneratePropDict(models)
    local dict = CS.CardWizard.Tools.Utilities.NewDict('string', 'object')
    local max = CS.CardWizard.Tools.Utilities.GetCount(models) - 1
    for i = 0, max do
        local m = models[i]
        dict:Add(m.Name, 'int')
    end
    local CodeBuilder = CS.CardWizard.Tools.CodeBuilder
    local builder = CS.CardWizard.Tools.CodeBuilder('PropDict')
    builder.Namespace = 'CardWizard.Data'
    local unit = builder:GenerateUnit(dict)
    local file = CodeBuilder.GenerateCode(unit, './Generated', builder.Name, true)
end
