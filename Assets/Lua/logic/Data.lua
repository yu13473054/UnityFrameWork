-- lua的数据文件，不可手动修改，引入项目后，需要手动Require一下
Data = {};
local _logStr = "<Data> %s中没有第%s条数据，请检查！！！";
local mt = {
    name = "",
    __index = function(t, key)
        LogErr(debug.traceback(string.format(_logStr, getmetatable( t ).name, key)));
    end
}

local function ToArray(content, isNum)
    if content == "" then
        return {};
    end
    if content then
        local array = {};
        local splitResult = string.split( content, '|' );
        for j = 1, #splitResult, 1 do
            if isNum then
                table.insert( array, tonumber( splitResult[j] ) );
            else
                table.insert( array, splitResult[j] );
            end
        end
        return array;
    end
end

Data.tableAI = nil;
function Data.ParseAI()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "AI" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				v1 = tonumber( tableHandler:GetValue( records, 1 ) ),
				v2 = tonumber( tableHandler:GetValue( records, 2 ) ),
				v3 = tableHandler:GetValue( records, 3 ),
				v4 = ToArray(tableHandler:GetValue( records, 4 ),true),
				v5 = ToArray(tableHandler:GetValue( records, 5 ),true),
				v6 = ToArray(tableHandler:GetValue( records, 6 ),false),
				v7 = tableHandler:GetValue( records, 7 ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "AI"));
    end
    return tempTable;
end
function Data.AI()
    if Data.tableAI ~= nil then
        return Data.tableAI;
    end
    Data.tableAI = Data.ParseAI();
    local mt = Clone( mt );
    mt.name = "AI";
    setmetatable(Data.tableAI, mt);
    return Data.tableAI;
end

Data.tableUnit = nil;
function Data.ParseUnit()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Unit" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				name = tableHandler:GetValue( records, 1 ),
				aiType = tonumber( tableHandler:GetValue( records, 2 ) ),
				skillList = ToArray(tableHandler:GetValue( records, 3 ),true),
				charSn = tonumber( tableHandler:GetValue( records, 4 ) ),
				hp = tonumber( tableHandler:GetValue( records, 5 ) ),
				atk = tonumber( tableHandler:GetValue( records, 6 ) ),
				def = tonumber( tableHandler:GetValue( records, 7 ) ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Unit"));
    end
    return tempTable;
end
function Data.Unit()
    if Data.tableUnit ~= nil then
        return Data.tableUnit;
    end
    Data.tableUnit = Data.ParseUnit();
    local mt = Clone( mt );
    mt.name = "Unit";
    setmetatable(Data.tableUnit, mt);
    return Data.tableUnit;
end

Data.tableSkill = nil;
function Data.ParseSkill()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Skill" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				cd = tonumber( tableHandler:GetValue( records, 1 ) ),
				type = tonumber( tableHandler:GetValue( records, 2 ) ),
				launcherList = ToArray(tableHandler:GetValue( records, 3 ),true),
				ftlName = ToArray(tableHandler:GetValue( records, 4 ),false),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Skill"));
    end
    return tempTable;
end
function Data.Skill()
    if Data.tableSkill ~= nil then
        return Data.tableSkill;
    end
    Data.tableSkill = Data.ParseSkill();
    local mt = Clone( mt );
    mt.name = "Skill";
    setmetatable(Data.tableSkill, mt);
    return Data.tableSkill;
end

Data.tableChar = nil;
function Data.ParseChar()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Char" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				res = tableHandler:GetValue( records, 1 ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Char"));
    end
    return tempTable;
end
function Data.Char()
    if Data.tableChar ~= nil then
        return Data.tableChar;
    end
    Data.tableChar = Data.ParseChar();
    local mt = Clone( mt );
    mt.name = "Char";
    setmetatable(Data.tableChar, mt);
    return Data.tableChar;
end

Data.tableFight_Buff = nil;
function Data.ParseFight_Buff()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Fight_Buff" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				actionType = tonumber( tableHandler:GetValue( records, 1 ) ),
				buffType = tonumber( tableHandler:GetValue( records, 2 ) ),
				valueList = ToArray(tableHandler:GetValue( records, 3 ),true),
				turn = tonumber( tableHandler:GetValue( records, 4 ) ),
				dispelType = tonumber( tableHandler:GetValue( records, 5 ) ),
				fxSn_act = tonumber( tableHandler:GetValue( records, 6 ) ),
				fxSn_con = tonumber( tableHandler:GetValue( records, 7 ) ),
				fxSn_des = tonumber( tableHandler:GetValue( records, 8 ) ),
				sort = ToArray(tableHandler:GetValue( records, 9 ),true),
				viewType = tonumber( tableHandler:GetValue( records, 10 ) ),
				icon = tableHandler:GetValue( records, 11 ),
				desc = tableHandler:GetValue( records, 12 ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Fight_Buff"));
    end
    return tempTable;
end
function Data.Fight_Buff()
    if Data.tableFight_Buff ~= nil then
        return Data.tableFight_Buff;
    end
    Data.tableFight_Buff = Data.ParseFight_Buff();
    local mt = Clone( mt );
    mt.name = "Fight_Buff";
    setmetatable(Data.tableFight_Buff, mt);
    return Data.tableFight_Buff;
end

Data.tableFight_Launcher = nil;
function Data.ParseFight_Launcher()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Fight_Launcher" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				life = tonumber( tableHandler:GetValue( records, 1 ) ),
				interval = tonumber( tableHandler:GetValue( records, 2 ) ),
				num = tonumber( tableHandler:GetValue( records, 3 ) ),
				bulletSn = tonumber( tableHandler:GetValue( records, 4 ) ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Fight_Launcher"));
    end
    return tempTable;
end
function Data.Fight_Launcher()
    if Data.tableFight_Launcher ~= nil then
        return Data.tableFight_Launcher;
    end
    Data.tableFight_Launcher = Data.ParseFight_Launcher();
    local mt = Clone( mt );
    mt.name = "Fight_Launcher";
    setmetatable(Data.tableFight_Launcher, mt);
    return Data.tableFight_Launcher;
end

Data.tableFight_Bullet = nil;
function Data.ParseFight_Bullet()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Fight_Bullet" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				life = tonumber( tableHandler:GetValue( records, 1 ) ),
				speed = tonumber( tableHandler:GetValue( records, 2 ) ),
				checkType = tonumber( tableHandler:GetValue( records, 3 ) ),
				checkValues = ToArray(tableHandler:GetValue( records, 4 ),true),
				trigger = tonumber( tableHandler:GetValue( records, 5 ) ),
				targetType = tonumber( tableHandler:GetValue( records, 6 ) ),
				targetValues = ToArray(tableHandler:GetValue( records, 7 ),true),
				effectList = ToArray(tableHandler:GetValue( records, 8 ),true),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Fight_Bullet"));
    end
    return tempTable;
end
function Data.Fight_Bullet()
    if Data.tableFight_Bullet ~= nil then
        return Data.tableFight_Bullet;
    end
    Data.tableFight_Bullet = Data.ParseFight_Bullet();
    local mt = Clone( mt );
    mt.name = "Fight_Bullet";
    setmetatable(Data.tableFight_Bullet, mt);
    return Data.tableFight_Bullet;
end

Data.tableFight_Effect = nil;
function Data.ParseFight_Effect()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Fight_Effect" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				decType = ToArray(tableHandler:GetValue( records, 1 ),true),
				decValue = ToArray(tableHandler:GetValue( records, 2 ),true),
				actionType = tonumber( tableHandler:GetValue( records, 3 ) ),
				valueList = ToArray(tableHandler:GetValue( records, 4 ),true),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Fight_Effect"));
    end
    return tempTable;
end
function Data.Fight_Effect()
    if Data.tableFight_Effect ~= nil then
        return Data.tableFight_Effect;
    end
    Data.tableFight_Effect = Data.ParseFight_Effect();
    local mt = Clone( mt );
    mt.name = "Fight_Effect";
    setmetatable(Data.tableFight_Effect, mt);
    return Data.tableFight_Effect;
end

Data.tableAudio = nil;
function Data.ParseAudio()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "Audio" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				name = tableHandler:GetValue( records, 1 ),
				type = tonumber( tableHandler:GetValue( records, 2 ) ),
				volume = tonumber( tableHandler:GetValue( records, 3 ) ),
				loop = tonumber( tableHandler:GetValue( records, 4 ) ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "Audio"));
    end
    return tempTable;
end
function Data.Audio()
    if Data.tableAudio ~= nil then
        return Data.tableAudio;
    end
    Data.tableAudio = Data.ParseAudio();
    local mt = Clone( mt );
    mt.name = "Audio";
    setmetatable(Data.tableAudio, mt);
    return Data.tableAudio;
end

-------Permanent-------
function Data.Audio3()
    if Data.tableAudio ~= nil then
        return Data.tableAudio;
    end
    Data.tableAudio = Data.ParseAudio();
    local mt = Clone( mt );
    mt.name = "Audio";
    setmetatable(Data.tableAudio, mt);
    return Data.tableAudio;
end

function Data.Audio2()
    if Data.tableAudio ~= nil then
        return Data.tableAudio;
    end
    Data.tableAudio = Data.ParseAudio();
    local mt = Clone( mt );
    mt.name = "Audio";
    setmetatable(Data.tableAudio, mt);
    return Data.tableAudio;
end