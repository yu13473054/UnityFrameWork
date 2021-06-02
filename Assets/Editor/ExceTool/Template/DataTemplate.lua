-- lua的数据文件，不可手动修改，引入项目后，需要手动Require一下
Data = DefineConst("Data", {});
local _logStr = "<Data> %s中没有第%s条数据，请检查！！！";
local _mt = {
    name = "",
    __index = function(t, key)
        LogErr(debug.traceback(string.format(_logStr, getmetatable( t ).name, key)));
    end
}

local _emptyTable = {};
local _repeatTb = {num = {}, str = {}};
local function ToArray(content, isNum)
    if content == "" then
        return _emptyTable;
    end
    if content then
        local array = nil;
        if isNum then
            array = _repeatTb.num[content];
            if array then return array; end --直接获取缓存中的数据
            array = {};
            _repeatTb.num[content] = array;
            local splitResult = string.split( content, ',' );
            for j = 1, #splitResult, 1 do
                table.insert( array, tonumber( splitResult[j] ) );
            end
        else
            array = _repeatTb.str[content];
            if array then return array; end --直接获取缓存中的数据
            array = {};
            _repeatTb.str[content] = array;
            local splitResult = string.split( content, ',' );
            for j = 1, #splitResult, 1 do
                table.insert( array, splitResult[j] );
            end
        end
        return array;
    end
end

***********
function Data.Parse#1#()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.OpenFromData("#2#");
    if tableHandler then
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #4#;
            ---@class Data_#1#
            local t = {
                #5#
            };
            tempTable[key] = t;
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "#2#"));
    end
    return tempTable;
end

***********
#0# = nil;
function Data.#1#()
    if #0# ~= nil then
        return #0#;
    end
    #0# = Data.Parse#1#();
    local mt = Clone( _mt );
    mt.name = "#1#";
    setmetatable(#0#, mt);
    return #0#;
end

***********
tonumber( #0# )
***********
tableHandler:GetValue( records, #0# )
***********
#0# = #1#,
***********
#0# = ToArray(#1#, #2#),
***********
function Data.Parse#1#(name)
    local tempTable = {};

    --解析数据
	local tableHandler = TableHandler.OpenFromData(name);
    if tableHandler then
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #4#;
            ---@class Data_#1#
            local t = {
                #5#
            };
            tempTable[key] = t;
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", name));
    end
    return tempTable;
end

***********
#0# = {};
function Data.#1#(name)
    if #0#[name] ~= nil then
        return #0#[name];
    end

    #0#[name] = Data.Parse#1#(name);
    local mt = Clone( _mt );
    mt.name = name;
    setmetatable(#0#[name], mt);
    return #0#[name];
end

***********
#0# == "1" and true or false