using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Excel;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 详细使用规则见：使用规范.docx
/// </summary>

public class ExcelParserTool
{
    class TableInfo
    {
        public DataTable dataTable;
        public int colCount;
        public int rowCount;
        public string fileName;
        public string dir;
    }
    const int StartRow = 4;
    private static string LuaDir = Application.dataPath + "/Lua/logic";
    //常规配置表
    private static string ExcelDirPath = System.Environment.CurrentDirectory + "/配置表";
    private static string TxtDir = Application.dataPath + "/Localization/{0}/Data/Excel";
    private static string LocalFile = "Localization.txt";
    static List<TableInfo> tableList = new List<TableInfo>();
    static Dictionary<string, Dictionary<string, string>> localIdDic = new Dictionary<string, Dictionary<string, string>>();
    static Dictionary<string, Dictionary<string, string>> localValueDic = new Dictionary<string, Dictionary<string, string>>();

    [MenuItem("工具/生成Excel数据")]
    public static void GenerateDataStructure()
    {
        tableList.Clear();

        Directory.CreateDirectory(LuaDir);

        //SVNFileSync.ExcelSync();
        //AssetDatabase.Refresh();

        //获得所有的excel文件
        List<string> files = new List<string>();
        GetFilesRecursion(files, ExcelDirPath);
        for (int i = 0; i < files.Count; i++)
        {
            string singleFile = files[i];
            if(!singleFile.EndsWith(".xlsx")) continue;
            EditorUtility.DisplayProgressBar("解析Excel文件", Path.GetFullPath(singleFile), (i + 1) * 1f / files.Count);
            //获取有效的路径
            string dir = Path.GetDirectoryName(singleFile).Replace(ExcelDirPath,"");
            if (!string.IsNullOrEmpty(dir)) dir = dir.TrimStart('/', '\\');
            //解析Excel并保存
            ParserSingleFile(singleFile, dir);
        }
        //创建txt文件
        CreateTxt();
        //创建解析文件
        CreateParserFile();
        AssetDatabase.Refresh();
        
        EditorUtility.ClearProgressBar();
        Debug.Log("所有Excel文件对应的解析文件创建完成");
    }

    static void ParserSingleFile(string filePath, string dir)
    {
        FileStream stream = null;
        try
        {
            stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (System.Exception)
        {
            string err = string.Format("读取配置表[{0}]失败, 检查是否用Excel打开了这个配置表", Path.GetFileNameWithoutExtension(filePath));
            EditorUtility.DisplayDialog("", err, "失败");
            Debug.LogError(err);
            return;
        }
        using (stream)
        {
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            DataSet result = excelReader.AsDataSet();

            for (int i = 0; i < result.Tables.Count; i++)
            {
                System.Data.DataTable wookSheet = result.Tables[i];
                if (wookSheet == null)
                {
                    Debug.LogError("Excel中没有Sheet");
                    continue;
                }

                //如果表格名称是#ResUse#ResUse_skill这种结构，表示哟共同的通用结构
                string tableName;
                string sheetName = wookSheet.TableName;
                if (!sheetName.Contains("|")) return;

                string[] nameSplit = sheetName.Split('|');
                tableName = nameSplit[1].Substring(0, 1).ToUpper() + nameSplit[1].Substring(1);

                //获取内容数量
                int columnCount;
                int rowCount;
                GetValidCount(wookSheet, out columnCount, out rowCount);
                if (columnCount == 0) return;
                TableInfo tableInfo = new TableInfo()
                {
                    dataTable = wookSheet,
                    colCount = columnCount,
                    rowCount = rowCount,
                    fileName = tableName,
                    dir = dir
                };
                if (!CheckRepeat(tableName,dir))
                {
                    tableList.Add(tableInfo);
                }
            }
        }
    }

    private static void CreateTxt()
    {
        List<string> addList = new List<string>();

        //清理旧的txt文件:保留文件名称首字母小写的数据文件
        foreach (string dir in AutoResMap.LocalTypeList)
        {
            string dirPath = string.Format(TxtDir, dir);
            if(Directory.Exists(dirPath))
                ClearOldTxt(dirPath);
            else
                Directory.CreateDirectory(dirPath);
        }

        localIdDic.Clear();
        localValueDic.Clear();

        //生成新的txt文件
        for (int k = 0; k < tableList.Count; k++)
        {
            TableInfo tableInfo = tableList[k];
            EditorUtility.DisplayProgressBar("正在生成Txt文件", tableInfo.fileName+".txt", (k + 1) * 1f / tableList.Count);
            StringBuilder sb = new StringBuilder();
            DataRow exportRow = tableInfo.dataTable.Rows[0];
            DataRow typeRow = tableInfo.dataTable.Rows[1];
            //记录字段名称
            DataRow fieldRow = tableInfo.dataTable.Rows[2];
            for (int i = 0; i < tableInfo.colCount; i++)
            {
                if(!exportRow[i].ToString().Trim().ToLower().Contains("c")) continue;//注释用字段，不导入txt文件中

                string value = fieldRow[i].ToString().Trim();
                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogError(tableInfo.fileName+"表格中字段名不能为空！");
                    return;
                }
                sb.Append(value);
                if (i != tableInfo.colCount - 1)
                {
                    sb.Append("\t");
                }
            }
            sb.Append("\n");
            //主键：表格中第一列含有c的为主键
            int mainKey = 0;
            for (int j = 0; j < tableInfo.colCount; j++)
            {
                string value = exportRow[j].ToString().ToLower();
                string type = typeRow[j].ToString().ToLower();
                if (value.Contains("c") && !type.Contains("[]") && !type.Contains("bool"))
                {
                    mainKey = j;
                    break;
                }
            }
            //多语言文本结构
            string[] dirSplits = tableInfo.dir.Split(new[] { '/' }, 1);
            string localModuleName = dirSplits[0];
            if (!localIdDic.ContainsKey(localModuleName)) {
                localIdDic.Add(localModuleName, new Dictionary<string, string>());
                localValueDic.Add(localModuleName, new Dictionary<string, string>());
            }

            //记录内容
            for (int i = StartRow; i < tableInfo.rowCount; i++)
            {
                DataRow rowValue = tableInfo.dataTable.Rows[i];
                string mainKeyStr = rowValue[mainKey].ToString().Trim();
                //记录每行的值
                for (int j = 0; j < tableInfo.colCount; j++)
                {
                    string format = exportRow[j].ToString().Trim().ToLower();
                    if (!format.Contains("c")) continue;//注释用字段，不导入txt文件中

                    string type = typeRow[j].ToString().Trim().ToLower();
                    string value = rowValue[j].ToString().Trim();
                    //内容空的时候，设置默认值
                    if (string.IsNullOrEmpty(value))
                    {
                        if (format.Contains("t") && !format.Contains("e"))
                            Debug.LogErrorFormat("表 {0} 中第 {1} 行的 {2} 值不能为空，请检查！", tableInfo.fileName, i+1, fieldRow[j].ToString().Trim());
                        else
                        {
                            if (type.Contains("[]"))
                                value = "";
                            else
                            {
                                if (type.Contains("string"))
                                    value = "";
                                else
                                    value = "0";
                            }
                        }
                    }
                    else
                    {
                        //检查数据是否合法
                        if (type.Contains("int") || type.Contains("float") || type.Contains("double"))
                        {
                            if (Regex.IsMatch(value, @"[^ 0123456789.|-]"))
                            {
                                Debug.LogErrorFormat("表 {0} 中第 {1} 行的 {2} 值不合法，请检查！", tableInfo.fileName,i+1, fieldRow[j].ToString().Trim());
                            }
                        }
                        else if (type.Contains("bool"))
                        {
                            if (type.Contains("[]"))
                                value = TransBoolArray(value);
                            else
                                value = TransBool(value);
                        }
                        else if(type.Contains("string") && format.Contains("t"))//含有多语言文本
                        {
                            if (type.Contains("[]"))
                            {
                                string[] splits = value.Split('|');
                                value = "";
                                for(int m = 0; m < splits.Length; m++)
                                {
                                    string str = splits[m];
                                    string localId;
                                    if(!localValueDic[localModuleName].TryGetValue(str, out localId)) {//判断字符串是否已经存在
                                        localId = tableInfo.fileName + "_" + fieldRow[j] + "_" + mainKeyStr + "_" + (m + 1);
                                        localIdDic[localModuleName].Add(localId, str);
                                        localValueDic[localModuleName].Add(str, localId);
                                    }
                                    if (m == splits.Length - 1)
                                        value += localId;
                                    else
                                        value += localId + "|";
                                }
                            }
                            else
                            {
                                string localId;
                                if (!localValueDic[localModuleName].TryGetValue(value, out localId))
                                {//判断字符串是否已经存在
                                    localId = tableInfo.fileName + "_" + fieldRow[j] + "_" + mainKeyStr;
                                    localIdDic[localModuleName].Add(localId, value);
                                    localValueDic[localModuleName].Add(value, localId);
                                }
                                value = localId;
                            }
                        }
                    }
                    //保存内容
                    sb.Append(value);
                    if (j != tableInfo.colCount - 1)
                        sb.Append("\t");
                }
                //换行
                if (i != tableInfo.rowCount - 1)
                    sb.Append("\n");
            }
            //根据多语言，分割txt文件
            string subDirPath = dirSplits.Length > 1 ? dirSplits[1] : "";
            if (string.IsNullOrEmpty(subDirPath))
            {
                string filePath = string.Format(TxtDir, dirSplits[0]) + "/"+tableInfo.fileName+".txt";
                File.WriteAllText(filePath,sb.ToString());
                addList.Add(filePath);
            }
            else
            {
                string dirPath = string.Format(TxtDir, dirSplits[0]) + "/" + subDirPath;
                Directory.CreateDirectory(dirPath);
                string filePath = dirPath + "/"+tableInfo.fileName+".txt";
                File.WriteAllText(filePath,sb.ToString());
                addList.Add(filePath);
            }
        }
        CreateLocalFile(addList);//创建多语言文件
        AssetDatabase.Refresh();

        if (addList.Count>0)
        {
            AutoResMap.ToResmapExternal(addList);
        }
    }

    private static void CreateLocalFile(List<string> addList)
    {
        EditorUtility.DisplayProgressBar("正在生成Localization文件", LocalFile, 1);
        foreach(var pair in localIdDic)
        {
            string moduleName = pair.Key;
            StringBuilder sb = new StringBuilder();
            sb.Append("id\tname\n");
            foreach(var pair1 in pair.Value) {
                sb.Append(pair1.Key).Append("\t").Append(pair1.Value).Append("\n");
            }
            string filePath = string.Format(TxtDir, moduleName) + "/" + LocalFile;
            File.WriteAllText(filePath, sb.ToString().TrimEnd());
            addList.Add(filePath);
        }
    }

    private static void CreateParserFile()
    {
        string destFilePath = LuaDir + "/" + "Data.lua";
        string cutLine = "----------CutLine----------";
        string customLine = "-------Do Not Delete-------";
        string permanentLine = "-------Permanent-------";

        //固定文本内容，主要是解析方法
        StringBuilder fixedSB = new StringBuilder();
        //自定义内容，主要是获取数据的方法，支持使用者自己修改
        StringBuilder customSB = new StringBuilder();
        //永久的内容：手动添加进去的解析方法等等
        string permanentStr = "";

        //先获取目标文件中自定义的内容
        Dictionary<string,string> oldCTDic = new Dictionary<string, string>();
        if (File.Exists(destFilePath))
        {
            string fileText = File.ReadAllText(destFilePath);
            string[] split = fileText.Split(new []{ cutLine , permanentLine }, StringSplitOptions.None);
            string oldCustomText = split[1].Trim();
            if (split.Length > 2)
            {
                permanentStr = split[2].Trim();
            }
            if (!string.IsNullOrEmpty(oldCustomText))
            {
                string[] textArray = oldCustomText.Split(new []{ customLine }, StringSplitOptions.RemoveEmptyEntries);
                //按照方法名生成字典，方便查询
                for (int i = 0; i < textArray.Length; i++)
                {
                    string getFuncName = Regex.Match(textArray[i], "Data\\..+\\(.*\\)").Value;
                    oldCTDic.Add(getFuncName,textArray[i].TrimStart());
                }
            }
        }

        //获取模板文件中的内容
        string templateText = File.ReadAllText(Application.dataPath + "/Editor/ExceTool/Template/DataTemplate.lua");
        string[] structTexts = templateText.Split(new [] { "***********" }, StringSplitOptions.None);
        //去掉模板内容中的首尾换行
        structTexts[1] = structTexts[1].TrimStart();
        structTexts[2] = structTexts[2].TrimStart();
        for (int i = 3; i < 7; i++)
        {
            structTexts[i] = structTexts[i].Trim();
        }
        structTexts[7] = structTexts[7].TrimStart();
        structTexts[8] = structTexts[8].TrimStart();
        structTexts[9] = structTexts[9].TrimStart();

        //保存转换成table的方法
        fixedSB.Append(structTexts[0]);

        //开始生成表格解析内容
        for (int i = 0; i < tableList.Count; i++)
        {
            TableInfo tableInfo = tableList[i];
            //以CN中的表格结构为模板
            if(!tableInfo.dir.StartsWith("CN")) continue;

            //使用单独的解析体
            string parseFuncText = structTexts[1].Replace("#1#", tableInfo.fileName)
                    .Replace("#2#", tableInfo.fileName);
            string funcName = "Data." + tableInfo.fileName + "()";
            string localFieldName = "_" + tableInfo.fileName.Substring(0, 1).ToLower() + tableInfo.fileName.Substring(1);
            string dataFuncText = structTexts[2].Replace("#0#", localFieldName).Replace("#1#", tableInfo.fileName);

            DataTable table = tableInfo.dataTable;
            DataRow exprotRow = table.Rows[0];
            DataRow typeRow = table.Rows[1];
            //主键：表格中第一列含有c的为主键
            string type = "";
            for (int j = 0; j < tableInfo.colCount; j++)
            {
                if (exprotRow[j].ToString().ToLower().Contains("c"))
                {
                    type = typeRow[j].ToString().ToLower();
                    break;
                }
            }
            string key;
            if (type.Contains("[]"))
            {
                Debug.LogErrorFormat("{0}表中的主键不能为数组！请检查",tableInfo.fileName);
                continue;
            }
            else if (type.Contains("bool"))
            {
                Debug.LogErrorFormat("{0}表中的主键不能为布尔值！请检查", tableInfo.fileName);
                continue;
            }
            else
            {
                if (type.Contains("string"))
                    //tableHandler:GetValue( records, 0 )
                    key = structTexts[4].Replace("#0#", "0");
                else
                    //tonumber( tableHandler:GetValue( records, 0 ) )
                    key = structTexts[3].Replace("#0#",structTexts[4].Replace("#0#","0"));
            }
            parseFuncText = parseFuncText.Replace("#4#", key);

            //字段赋值
            StringBuilder fieldValueSb = new StringBuilder();
            int index = 0;
            for (int j = 0; j < tableInfo.colCount; j++)
            {
                if (!exprotRow[j].ToString().ToLower().Contains("c"))
                    continue;

                string fieldName = table.Rows[2][j].ToString().Substring(0, 1).ToLower() +
                                   table.Rows[2][j].ToString().Substring(1);

                string fieldValue;
                type = typeRow[j].ToString().ToLower();
                //解析List
                if (type.Contains("[]"))
                {
                    string boolValue;
                    if (type.Contains("string"))
                        boolValue = "false";
                    else
                        boolValue = "true";
                    //样式：levels = ToArray(tableHandler:GetValue( records, 2 ), false),
                    fieldValue = structTexts[6].Replace("#0#", fieldName).Replace("#1#", structTexts[4].Replace("#0#", index.ToString())).Replace("#2#", boolValue);
                }
                //解析常规
                else
                {
                    if (type.Contains("string"))
                        //样式：desc = tableHandler:GetValue( records, 1 ),
                        fieldValue = structTexts[5].Replace("#0#",fieldName).Replace("#1#", structTexts[4].Replace("#0#", index.ToString()));
                    else if(type.Contains("bool"))
                        //records == "1" and true or false,
                        fieldValue = structTexts[5].Replace( "#0#", fieldName ).Replace( "#1#", structTexts[9].Replace( "#0#", structTexts[4].Replace( "#0#", index.ToString() ) ) );
                    else
                        //样式：descID = tonumber( tableHandler:GetValue( records, 1 ) ),
                        fieldValue = structTexts[5].Replace("#0#",fieldName).Replace("#1#", structTexts[3].Replace("#0#", structTexts[4].Replace("#0#", index.ToString())));
                }
                if (index != 0) fieldValueSb.Append("\t\t\t\t");
                fieldValueSb.Append(fieldValue);
                if (j != tableInfo.colCount-1) fieldValueSb.Append("\n");
                index++;
            }
            parseFuncText = parseFuncText.Replace("#5#", fieldValueSb.ToString());

            fixedSB.Append(parseFuncText);

            //获取数据方法体
            if (oldCTDic.ContainsKey(funcName))
            {
                //直接使用文件中已经存在的获取数据的方法
                customSB.Append(customLine).Append("\n").Append(oldCTDic[funcName]);
            }
            else
            {
                //生成默认的获取数据的方法
                customSB.Append(customLine).Append("\n").Append(dataFuncText);
            }
        }
        EditorUtility.DisplayProgressBar("正在生成Lua文件", "Data.lua", 1);
        //存储内容
        fixedSB.Append(cutLine).Append("\n\n").Append(customSB).Append("\n\n").Append(permanentLine).Append("\n").Append(permanentStr);
        File.WriteAllText(destFilePath, fixedSB.ToString());
    }

    //检查是否存在重名的表格
    static bool CheckRepeat(string fileName, string dir)
    {
        foreach (TableInfo info in tableList)
        {
            if (info.fileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)&& info.dir.Equals(dir, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("存在重名表格：" + fileName);
                return true;
            }
        }
        return false;
    }

    static string TransBool(string value)
    {
        value = value.Trim().ToLower();
        if (value.Equals("true") || value.Equals("1"))
            return "1";
        else
            return "0";
    }
    static string TransBoolArray(string value)
    {
        string result = "";
        string[] splits = value.Trim().Split('|');
        for (int i = 0; i < splits.Length; i++)
        {
            if (i == splits.Length - 1)
                result = result + TransBool(splits[i]);
            else
                result = result + TransBool(splits[i]) + "|";
        }
        return result;
    }

    //获取表格中有效的行列数
    public static void GetValidCount(DataTable wookSheet, out int colCount, out int rowCount)
    {
        colCount = wookSheet.Columns.Count;
        rowCount = wookSheet.Rows.Count;
        for (int i = 0; i < colCount; i++)
        {
            var value = wookSheet.Rows[1][i].ToString();
            if (string.IsNullOrEmpty(value))
            {
                colCount = i;
                break;
            }

        }
        for (int i = StartRow; i < rowCount; i++)
        {
            var value = wookSheet.Rows[i][0].ToString();
            if (string.IsNullOrEmpty(value))
            {
                rowCount = i;
                break;
            }
        }
    }

    /// 递归获取dirPath文件夹下所有文件，包括子文件夹中的文件
    static void GetFilesRecursion(List<string> resultList, string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("请检查路径是不是文件夹。" + dirPath);
            return;
        }

        string[] files = GetFiles(dirPath);
        resultList.AddRange(files);
        string[] childDirPaths = Directory.GetDirectories(dirPath);
        for (int i = 0; i < childDirPaths.Length; i++)
        {
            string childDirPath = childDirPaths[i];
            if (childDirPath.Contains("工具生成服务器用表，勿改！！"))
            {
                continue;
            }
            GetFilesRecursion(resultList, childDirPath);
        }
    }

    static List<string> tmpList = new List<string>();
    static string[] GetFiles(string dirPath)
    {
        string[] files = Directory.GetFiles(dirPath);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            if (filePath.EndsWith(".meta") || filePath.Contains(".svn"))
                continue;
            tmpList.Add(filePath);
        }
        files = tmpList.ToArray();
        tmpList.Clear();
        return files;
    }

    //清除指定路径中的txt文件，但是保留小写字母开头的文件
    private static void ClearOldTxt(string dirPath)
    {
        //获得所有的excel文件
        List<string> files = new List<string>();
        GetFilesRecursion(files, dirPath);
        for (int i = files.Count - 1; i >= 0; i--)
        {
            string filePath = files[i];
            if(!filePath.EndsWith(".txt")) continue;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            char firstC = fileName[0];
            if (firstC >= 'a' && firstC <= 'z')
            {
                //小写字母
                continue;
            }
            else
            {
                File.Delete(filePath);
            }
        }
    }
}