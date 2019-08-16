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

***********
local function Parse#1#()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "#2#" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #4#;
            tempTable[key] = {
                #5#
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "#2#"));
    end
    return tempTable;
end

***********
local #0# = nil;
function Data.#1#()
    if #0# ~= nil then
        return #0#;
    end
    #0# = Parse#1#();
    local mt = Clone( mt );
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
#0# = ToArray(#1#,#2#),
***********
local function Parse#1#(name)
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( name ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #4#;
            tempTable[key] = {
                #5#
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", name));
    end
    return tempTable;
end

***********
local #0# = {};
function Data.#1#(name)
    if #0#[name] ~= nil then
        return #0#[name];
    end

    #0#[name] = Parse#1#(name);
    local mt = Clone( mt );
    mt.name = name;
    setmetatable(#0#[name], mt);
    return #0#[name];
end


***********
#0# == "1" and true or false