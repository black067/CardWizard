-- 点击 Debug 按钮时执行的脚本
print('hello debug!')

--- 
function TestGenerateDebugDatas()
    local weapons = {
        '随身短刀', 'P1911', '20号霰弹枪', 'M16', '乌兹', '马克沁', '迫击炮', '手雷', '森林猎弓'
    }
    for k, v in ipairs(weapons) do
        local w = Weapon()
        w.Name = v
        w:SetType(k)
        DataBus:CacheData(w)
    end

    local occupations = {
        '医生', '记者', '警察', '侦探', '教授', '画家'
    }
    for k, v in ipairs(occupations) do
        local o = Occupation()
        o.Name = v
        DataBus:CacheData(o)
    end
    MainManager:ExportDataBus()
end

--- 
function TestDataBusLoad()
    DataBus:LoadAll(Config.PathData)
end

--- 
function CreateIcon()
    local Resources = CS.CardWizard.Properties.Resources
    local image = Resources.Image_Avatar_Empty
    local icon = CS.CardWizard.View.UIExtension.ToIcon(image)
    local stream = CS.System.IO.File.Create("./Resources/CardWizard.ico")
    icon:Save(stream)
    stream:Dispose()
    stream:Close()
    print('icon saved.')
end

--- 
function CreateThumnail()
    local Resources = CS.CardWizard.Properties.Resources
    local image = Resources.Image_Avatar_Empty
    local thumbnail = CS.CardWizard.View.UIExtension.ZoomIn(image, 128, 128)
    thumbnail:Save('./Resources/temp.png')
end

--- 
function OpenListWindow( ... )
	-- body
    print('Open List Window')
    local wd = CS.CardWizard.View.ListWindow(Config.baseModel, MainManager.LuaHub, MainManager.Localization)
    wd:Show()
end

function TestForAssets()
    local eras = {'1890s', '1920s', 'Modern'}
    local values = {1,2,3,4,5,6,7,8,9,10}
    for i=1,#eras do
        local msg = ''
        for j=1,#values do
            msg = msg..', '..AssetOrignal(eras[i], values[j])
        end
        print(msg)
    end
end

TestForAssets()

-- GeneratePropDict(Config.baseModel)
-- OpenListWindow()
-- TestGenerateDebugDatas()
-- TestDataBusLoad()
-- CreateIcon()
-- CreateThumnail()