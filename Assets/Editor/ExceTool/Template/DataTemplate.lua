-- lua的数据文件，不可手动修改，引入项目后，需要手动Require一下
Data = {};

local function ToArray(content, isNum)
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

***********
local #0# = nil;
function Data.#1#()
    if #0# ~= nil then
        return #0#;
    end
    #0# = {};

    --解析数据
    local tableHandler = TableHandler.Open("#2#","#3#");
    local tableRecordsNum = tableHandler:GetRecordsNum() - 1;
    for records = 0, tableRecordsNum, 1 do
        local key = #4#;
        #0#[key] = {
            #5#
        };
    end
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