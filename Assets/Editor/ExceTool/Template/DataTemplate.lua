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

***********
Data.table#Name# = nil;
function Data.Parse#Name#()
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( "#Name#" ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #desc#;
            tempTable[key] = {
                #value#
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", "#Name#"));
    end
    return tempTable;
end
function Data.#Name#()
    if Data.table#Name# ~= nil then
        return Data.table#Name#;
    end
    Data.table#Name# = Data.Parse#Name#();
    local mt = Clone( mt );
    mt.name = "#Name#";
    setmetatable(Data.table#Name#, mt);
    return Data.table#Name#;
end

***********
tonumber( tableHandler:GetValue( records, #col# ) )
***********
tableHandler:GetValue( records, #col# )
***********
#key# = #desc#,
***********
#key# = ToArray(tableHandler:GetValue( records, #col# ),#isNum#),
***********
tableHandler:GetValue( records, #col# ) == "1" and true or false
***********
Data.table#Name# = {};
function Data.Parse#Name#(name)
    local tempTable = {};

    --解析数据
    local tableHandler = TableHandler.New();
    if tableHandler:OpenFormResmap( name ) then 
        local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
        for records = 0, tableRecordsNum, 1 do
            local key = #desc#;
            tempTable[key] = {
                #value#
            };
        end
    else
        LogErr(string.format("<Data> 加载表格%s失败！！！", name));
    end
    return tempTable;
end
function Data.#Name#(name)
    if Data.table#Name#[name] ~= nil then
        return Data.table#Name#[name];
    end

    Data.table#Name#[name] = Parse#Name#(name);
    local mt = Clone( mt );
    mt.name = name;
    setmetatable(Data.table#Name#[name], mt);
    return Data.table#Name#[name];
end
