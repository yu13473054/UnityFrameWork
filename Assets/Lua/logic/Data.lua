-- lua的数据文件，不可手动修改，引入项目后，需要手动Require一下
Data = {};
local _logStr = "<Data> %s中没有第%s条数据，请检查！！！";
local mt = {
    name = "",
    __index = function(t, key)
        LogErr(string.format(_logStr, getmetatable( t ).name, key));
    end
}

local function ToArray(content, isNum)
    if content == "" then
        return {};
    end
    if content then
        local array = {};
        local splitResult = string.split( content, ',' );
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

local function ParseAI()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "AI" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = tonumber( tableHandler:GetValue( records, 0 ) );
            tempTable[key] = {
                sn = tonumber( tableHandler:GetValue( records, 0 ) ),
				desc = tonumber( tableHandler:GetValue( records, 1 ) ),
				v1 = tonumber( tableHandler:GetValue( records, 2 ) ),
				v2 = tonumber( tableHandler:GetValue( records, 3 ) ),
				v3 = tableHandler:GetValue( records, 4 ),
				v4 = ToArray(tableHandler:GetValue( records, 5 ),true),
				v5 = ToArray(tableHandler:GetValue( records, 6 ),true),
				v6 = ToArray(tableHandler:GetValue( records, 7 ),false),
				v7 = tableHandler:GetValue( records, 8 ),
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "AI"));
    end
    return tempTable;
end

----------CutLine----------

-------Do Not Delete-------
local _aI = nil;
function Data.AI()
    if _aI ~= nil then
        return _aI;
    end
    _aI = ParseAI();
    local mt = Clone( mt );
    mt.name = "AI";
    setmetatable(_aI, mt);
    return _aI;
end

-------Permanent-------
