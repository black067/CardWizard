DOC_DamageBonus = [[ 本程序的 DB 算法参考以下数据 (来自第六版规则书)
STR+SIZ  DB
2-12     -1D6
13-16    -1D4
17-24    0
25-32    +1D4
33-40    +1D6
41-56    +2D6
57-72    +3D6
73-88    +4D6
89-104   +5D6
105-120  +6D6
121-136  +7D6
137-152  +8D6
153-168  +9D6
169-184  +10D6
* 这之上, 每+16, 伤害奖励再增加1D6
]]

--- 计算伤害奖励的脚本
function DamageBonus(strength, size)
	local result = 0
	local sum = strength + size
	-- 结果为 0 的情况
	if (sum > 16 and sum <= 24) then return '0' end
	-- 骰子面数
	local d = 6
	if (sum > 12 and sum <= 16)
	or (sum > 24 and sum <= 32) then d = 4 end
	-- 系数
	local c = 1
	if sum <= 16 then c = -1
	elseif sum > 32 then
		c = sum / 16 - 1
	end
	-- 返回计算结果
	return string.format('%0.0fD%0.0f', c, d)
end

--- 计算 MOV 的值
function GetMOV(character)
	return 7
end

DOC_AssetOrignal = [[ 本程序的 初始资金 算法参考以下数据 (来自第六版规则书)
1. 选择对应的游戏年代(1890s, 1920s 和 现代)
  - 1890s: 投1D10, 结果: 1=$500+房子&膳食, 2=$1000, 3=$1500, 4=$2000, 5=$2500, 6=$3000, 7=$4000, 8=$5000, 9=$5000, 10=$10000
  - 1920s: 投1D10, 结果: 1=$1500+房子&膳食, 2=$2500, 3=$3500, 4=$3500, 5=$4500, 6=$5500, 7=$6500, 8=$7500, 9=$10000, 10=$20000
  - 现代: 投1D10, 结果:	1=$15000+房子&膳食, 2=$25000, 3=$35000, 4=$45000, 5=$55000, 6=$75000, 7=$100000, 8=$200000, 9=$300000, 10=$500000
2. 调查员拥有财产和其他价值年收入五倍的资产
  - 一个现代的调查员投出55000美元拥有275000的资产。
  - 这些资产的十分之一存入银行当作现金。
  - 另外十分之一是股份和债券, 可以在30天内转移。
  - 余下的是老书, 房子或者是任何符合角色的东西。
]]

DATA_AssetOriginal = {}
DATA_AssetOriginal['1890s'] = {	'$500+房子&膳食', '$1000', '$1500', '$2000', '$2500', '$3000', '$4000', '$5000', '$5000', '$10000'}
DATA_AssetOriginal['1920s'] = {	'$1500+房子&膳食', '$2500', '$3500', '$3500', '$4500', '$5500', '$6500', '$7500', '$10000', '$20000'}
DATA_AssetOriginal['Modern'] = {'$15000+房子&膳食', '$25000', '$35000', '$45000', '$55000', '$75000', '$100000', '$200000', '$300000', '$500000'}
--- 初始资产的计算
function AssetOrignal(era, value)
	-- 1890s || 1920s
	local result = 0
	local index = 0
	if value >= 1 and value <= #DATA_AssetOriginal['1890s'] then index = value end
	if era == '1890s' or '1920s' then
		result = DATA_AssetOriginal[era][index]
	-- 现代
	else
		result = DATA_AssetOriginal['Modern'][index]
	end
	return result
end

DOC_EDUAndAges = [[ 角色教育值与年龄的关系如下 (来自第六版规则书)
一名调查员的最小年龄为'教育(EDU) + 6'岁. 你每大于这个岁数 10 岁, 获得 1 点教育(EDU) 与 20 点职业点数.
当超过 40 岁时, 每超过 10 年, 从以下属性中选择 1 点减去: STR / CON / DEX / APP.
]]

function GetMinAge(eduBase)
	return eduBase + 6
end

function AgeBonus(eduBase, age, minAge)
	if minAge == nil then minAge = GetMinAge(eduBase) end
	local bonus = {}
	-- 计算年龄带来的教育值与职业技能点奖励
	local delta = age - minAge
	local ageBonus =  math.floor(delta / 10)
	bonus['EDU'] = ageBonus
	bonus['OccupationPoints'] = ageBonus
	-- 计算衰老惩罚
	bonus['Adjustment'] = 0
	local deltaForPenalty = age - 40
	if deltaForPenalty >= 0 then
		bonus['Adjustment'] = math.floor(deltaForPenalty / 10)
	end
	return bonus
end
