--- 此文件声明了一些重要的算法, 如果不清楚如何修改/修改了会产生何种后果, 请不要修改.
-- 默认年龄是 24 岁
DEFAULT_AGE = CS.CallOfCthulhu.Character.DEFAULT_AGE

DOC_DamageBonus = [[ DamageBonus 的算法参考以下数据 (来自第七版规则书)
STR+SIZ		伤害加值		体格
2-64		-2			-2
65-84		-1			-1
85-124		0			0
125-164		+1d4		1
165-204		+1d6		2
205-284*	+2d6		3
285-364		+3d6		4
365-444		+4d6		5
445-524		+5d6		6
* 这之上, 每+80, 伤害奖励再增加1D6
]]

--- <summary>
--- 计算伤害奖励的脚本
--- 返回值分别是 伤害加值的计算公式 和 体格值
--- </summary>
function DamageBonus(strength, size)
	local result = 0
	local build = 0
	local sum = strength + size
	-- 结果为 常数 的情况
	if (sum > 84 and sum <= 124) then return '0', build end
	if (sum <= 64) then return '-2', -2 end
	if (sum <= 84) then return '-1', -1 end
	-- 骰子面数
	local d = 6
	if (sum > 124 and sum <= 164) then 
		d = 4
		build = 1
	end
	-- 系数
	local c = 1
	if sum > 164 then
		c = sum / 80 - 1
		build = c + 1
	end
	-- 返回计算结果
	return string.format('%0.0fD%0.0f', c, d), build
end

DOC_GetMOV = [[ GetMOV 算法参考以下数据 (来自第七版规则书)
MOV 与 DEX, STR, SIZ 有关: 
- MOV 7		DEX < SIZ and STR < SIZ
- MOV 8		DEX >= SIZ or STR >= SIZ
- MOV 9		DEX > SIZ and STR > SIZ
MOV 的值还与年龄有关: 
- 年龄在40-49岁之间: MOV -= 1
- 年龄在50-59岁之间: MOV -= 2
- 年龄在60-69岁之间: MOV -= 3
- 年龄在70-79岁之间: MOV -= 4
- 年龄在80-89岁之间: MOV -= 5
]]

--- <summary>
--- 计算 MOV 的值
--- </summary>
function GetMOV(size, dexterity, strength, age)
	if age == nil then age = DEFAULT_AGE end
	local AgePenalty = math.floor((age - 40) / 10 + 1)
	local movement = 7
	if dexterity > size and strength > size then movement = 9
	elseif dexterity >= size or strength >= size then movement = 8 
	end
	if AgePenalty > 0 then movement = movement - AgePenalty end
	return movement
end

DOC_AssetOrignal = [[ AssetOrignal 算法参考以下数据 (来自第六版规则书)
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
--- <summary>
--- 初始资产的计算
--- </summary>
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

--- <summary>
--- 教育增强检定
--- </summary>
function EDUBonus(eduBase, count)
	if count == nil then count = 1 end
	-- EDU shouldn't be larger than 99
	print(string.format('在 %0.0f 教育值的基础上, 进行 %0.0f 次教育增强检定', eduBase, count))
	local r = 0
	for i = 1, count do
		if eduBase + r >= 99 then 
			break 
		end
		local d = Roll(1, 100) -- Equals to 1D100
		if eduBase + r >= d then goto ContinueEDUBonus end
		local v = Roll(1, 10) -- Equa to 1D10
		if eduBase + r + v < 100 then  -- EDU shouldn't be larger than 99
			r = r + v
		else
			r = 99 - eduBase
			break
		end
		::ContinueEDUBonus::
	end
	print(string.format('最终教育值增加了 %0.0f', r))
	return r
end

DOC_EDUAndAges = [[ 角色教育值与年龄的关系如下 (来自第七版规则书)
玩家的调查员应选择年龄在 15 至 90 之间。
若设想的调查员超过这个年龄范围，那么请找您的守秘人进行调整吧。
对照相应年龄，调整调查员的属性，不同年龄段的调整不叠加。 
15-19 岁：力量和体型合计减 5 点。教育减 5 点。 决定幸运值时可以骰 2 次并取较好的一次。
20-39 岁：对教育进行 1 次增强检定。
40-49 岁：对教育进行 2 次增强检定。力量/体质/敏捷合计减 5 点。外貌减 5 点。
50-59 岁：对教育进行 3 次增强检定。力量/体质/敏捷合计减 10 点。外貌减 10 点。
60-69 岁：对教育进行 4 次增强检定。力量/体质/敏捷合计减 20 点。外貌减 15 点。
70-79 岁：对教育进行 4 次增强检定。力量/体质/敏捷合计减 40 点。外貌减 20 点。
80-89 岁：对教育进行 4 次增强检定。力量/体质/敏捷合计减 80 点。外貌减 25 点。
]]

DATA_AGEBONUS = {
	[{0, 19}] = [[
Comment: 教育减 5 点。 力量/体型合计减 5 点。决定幸运值时可以骰 2 次并取较好的一次。
Rule: STR + SIZ == -5
Bonus:
- key: LUCK*
  formula: math.max(5 * (3D6), 5 * (3D6))
- key: EDU
  formula: -5]],

	[{20, 39}] = [[
Comment: 对教育进行 1 次增强检定。
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: EDU
  formula: EDUBonus(EDU, 1)]],

	[{40, 49}] = [[
Comment: 对教育进行 2 次增强检定。力量/体质/敏捷合计减 5 点。外貌减 5 点。
Rule: STR + CON + DEX == -5
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: APP
  formula: -5 
- key: EDU
  formula: EDUBonus(EDU, 2)]],

	[{50, 59}] = [[
Comment: 对教育进行 3 次增强检定。力量/体质/敏捷合计减 10 点。外貌减 10 点。
Rule: STR + CON + DEX == -10
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: APP  
  formula: -10 
- key: EDU  
  formula: EDUBonus(EDU, 3)]],

	[{60, 69}] = [[
Comment: 对教育进行 4 次增强检定。力量/体质/敏捷合计减 20 点。外貌减 15 点。
Rule: STR + CON + DEX == -20
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: APP  
  formula: -15 
- key: EDU  
  formula: EDUBonus(EDU, 4)]],

	[{70, 79}] = [[
Comment: 对教育进行 4 次增强检定。力量/体质/敏捷合计减 40 点。外貌减 20 点。
Rule: STR + CON + DEX == -40
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: APP  
  formula: -20 
- key: EDU  
  formula: EDUBonus(EDU, 4)]],

	[{80, 99}] = [[
Comment: 对教育进行 4 次增强检定。力量/体质/敏捷合计减 80 点。外貌减 25 点。
Rule: STR + CON + DEX == -80
Bonus: 
- key: LUCK*
  formula: 5 * (3D6)
- key: APP  
  formula: -25 
- key: EDU  
  formula: EDUBonus(EDU, 4)]],
}

--- <summary>
--- 年龄对属性的影响
--- </summary>
function AgeBonus(age)
	for k, v in pairs(DATA_AGEBONUS) do
		if age >= k[1] and age <= k[2] then
			return v
		end
	end
	return DATA_AGEBONUS[{80, 99}]
end
