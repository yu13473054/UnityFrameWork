using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Excel;
using UnityEditor;
using UnityEngine;

public class ExcelParserTool
{
    const int StartRow = 4;
    private static string ExcelDirPath = System.Environment.CurrentDirectory + "/配置表";
    private static string TxtDir = Application.dataPath + "/Data";
    private static string LuaDir = Application.dataPath + "/Lua/logic";

    class TableInfo
    {
        public DataTable dataTable;
        public int colCount;
        public int rowCount;
        public string fileName;
    }

    static List<TableInfo> tableList = new List<TableInfo>();

    [MenuItem("Tools/生成数据")]
    public static void GenerateDataStructure()
    {
        tableList.Clear();

        Directory.CreateDirectory(ExcelDirPath);
        Directory.CreateDirectory(TxtDir);
        Directory.CreateDirectory(LuaDir);
        AssetDatabase.Refresh();
        //获得所有的excel文件
        string[] files = Directory.GetFiles(ExcelDirPath, "*.xlsx");
        for (int i = 0; i < files.Length; i++)
        {
            string singleFile = files[i];
            string fileName = Path.GetFileNameWithoutExtension(singleFile);
            EditorUtility.DisplayProgressBar("正在生成数据结构", Path.GetFullPath(singleFile), (i + 1) * 1f / files.Length);
            //解析Excel并保存
            TableInfo tableInfo = ParserSingleFile(singleFile, fileName);
            if (tableInfo != null) tableList.Add(tableInfo);
        }
        //创建txt文件
        CreateTxt();
        AssetDatabase.Refresh();
        //创建lua文件
        CreateLuaFile();
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();
        Debug.Log("所有Excel文件对应的解析文件创建完成");
    }

    static TableInfo ParserSingleFile(string filePath, string fileName)
    {
        FileStream stream = null;
        try
        {
            stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (System.Exception)
        {
            string err = string.Format("读取配置表[{0}]失败, 检查是否用Excel打开了这个配置表", fileName);
            EditorUtility.DisplayDialog("", err, "失败");
            Debug.LogError(err);
            return null;
        }
        using (stream)
        {

            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            DataSet result = excelReader.AsDataSet();

            //一张表格中只使用第一个sheet
            System.Data.DataTable wookSheet = result.Tables[0];
            if (wookSheet == null)
            {
                Debug.LogError("Excel中没有Sheet");
                return null;
            }

            string tableName = wookSheet.TableName.Substring(0, 1).ToUpper() + wookSheet.TableName.Substring(1);

            //获取内容数量
            int columnCount;
            int rowCount;
            GetValidCount(wookSheet, out columnCount, out rowCount);
            if (columnCount == 0) return null;
            return new TableInfo()
            {
                dataTable = wookSheet,
                colCount = columnCount,
                rowCount = rowCount,
                fileName = tableName
            };
        }
    }

    //获取表格中有效的行列数
    static void GetValidCount(DataTable wookSheet, out int colCount, out int rowCount)
    {
        colCount = wookSheet.Columns.Count;
        rowCount = wookSheet.Rows.Count;
        for (int i = 0; i < colCount; i++)
        {
            var value = wookSheet.Rows[0][i].ToString();
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

    private static void CreateTxt()
    {
        for (int k = 0; k < tableList.Count; k++)
        {
            TableInfo tableInfo = tableList[k];
            EditorUtility.DisplayProgressBar("正在生成Txt文件", tableInfo.fileName+".txt", (k + 1) * 1f / tableList.Count);
            StringBuilder sb = new StringBuilder();
            DataRow typeRow = tableInfo.dataTable.Rows[0];
            //记录字段名称
            DataRow fieldRow = tableInfo.dataTable.Rows[1];
            for (int i = 0; i < tableInfo.colCount; i++)
            {
                string type = typeRow[i].ToString().Trim().ToLower();
                if(type.Contains("none")) continue;//注释用字段，不导入txt文件中
                string value = fieldRow[i].ToString().Trim();
                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogError(tableInfo.fileName+"表格中字段名不能为空！");
                    return;
                }
                sb.Append(value);
                sb.Append(i == tableInfo.colCount - 1 ? "\n" : "\t");
            }
            //记录内容
            for (int i = 4; i < tableInfo.rowCount; i++)
            {
                DataRow rowValue = tableInfo.dataTable.Rows[i];
                //记录每行的值
                for (int j = 0; j < tableInfo.colCount; j++)
                {
                    string type = typeRow[j].ToString().Trim().ToLower();
                    if (type.Contains("none")) continue;//注释用字段，不导入txt文件中
                    string value = rowValue[j].ToString().Trim();
                    //内容空的时候，设置默认值
                    if (string.IsNullOrEmpty(value))
                    {
                        if (type.Contains("list"))
                            value = "";
                        else
                        {
                            if (type.Contains("int") || type.Contains("float"))
                                value = "0";
                            else
                                value = "";
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

            File.WriteAllText(TxtDir+ tableInfo.fileName+".txt",sb.ToString());
        }
    }

    private static void CreateLuaFile()
    {
        StringBuilder sb = new StringBuilder();
        //获取模板文件中的内容
        string templateText = File.ReadAllText(Application.dataPath + "/Editor/ExceTool/Template/DataTemplate.lua");
        string[] structTexts = templateText.Split(new [] { "***********" }, StringSplitOptions.None);
        //去掉模板内容中的首尾换行
        structTexts[1] = structTexts[1].TrimStart();
        for (int i = 2; i < structTexts.Length; i++)
        {
            structTexts[i] = structTexts[i].Trim();
        }

        //保存文件头
        sb.Append(structTexts[0]);
        for (int i = 0; i < tableList.Count; i++)
        {
            TableInfo tableInfo = tableList[i];
            //字段名，首字母小写
            string localFieldName = "_"+tableInfo.fileName.Substring(0, 1).ToLower() + tableInfo.fileName.Substring(1);
            string fileName = tableInfo.fileName + ".txt";
            string abName = "data";
            //方法体
            string funcText = structTexts[1].Replace("#0#", localFieldName)
                        .Replace("#1#", tableInfo.fileName)
                        .Replace("#2#", fileName)
                        .Replace("#3#", abName);

            DataTable table = tableInfo.dataTable;

            DataRow typeRow = table.Rows[0];

            //主键：表格中第一列不为"none"的为主键
            string type = "";
            for (int j = 0; j < tableInfo.colCount; j++)
            {
                type = typeRow[j].ToString().ToLower();
                if(!type.Contains("none")) break;
            }
            string key;
            if (type.Contains("list"))
            {
                Debug.LogErrorFormat("{0}表中的主键不能为数组！请检查",tableInfo.fileName);
                continue;
            }
            else
            {
                if (type.Contains("int") || type.Contains("float"))
                    //tonumber( tableHandler:GetValue( records, 0 ) )
                    key = structTexts[2].Replace("#0#",structTexts[3].Replace("#0#","0"));
                else
                    //tableHandler:GetValue( records, 0 )
                    key = structTexts[3].Replace("#0#", "0");
            }
            funcText = funcText.Replace("#4#", key);

            //字段赋值
            StringBuilder fieldValueSb = new StringBuilder();
            for (int j = 0; j < tableInfo.colCount; j++)
            {
                string fieldValue;
                type = table.Rows[0][j].ToString().ToLower();
                string fieldName = table.Rows[1][j].ToString().Substring(0, 1).ToLower() +
                                   table.Rows[1][j].ToString().Substring(1);
                //解析List
                if (type.Contains("list"))
                {
                    string boolValue;
                    if (type.Contains("int") || type.Contains("float"))
                        boolValue = "true";
                    else
                        boolValue = "false";
                    //样式：levels = ToArray(tableHandler:GetValue( records, 2 ), false),
                    fieldValue = structTexts[5].Replace("#0#", fieldName).Replace("#1#", structTexts[3].Replace("#0#", j.ToString())).Replace("#2#", boolValue);
                }
                //解析常规
                else
                {
                    if (type.Contains("int") || type.Contains("float"))
                        //样式：descID = tonumber( tableHandler:GetValue( records, 1 ) ),
                        fieldValue = structTexts[4].Replace("#0#",fieldName).Replace("#1#", structTexts[2].Replace("#0#", structTexts[3].Replace("#0#", j.ToString())));
                    else
                        //样式：desc = tableHandler:GetValue( records, 1 ),
                        fieldValue = structTexts[4].Replace("#0#",fieldName).Replace("#1#", structTexts[3].Replace("#0#", j.ToString()));
                }
                if (j != 0) fieldValueSb.Append("\t\t\t");
                fieldValueSb.Append(fieldValue);
                if (j != tableInfo.colCount-1) fieldValueSb.Append("\n");
            }
            funcText = funcText.Replace("#5#", fieldValueSb.ToString());

            sb.Append(funcText);
        }
        EditorUtility.DisplayProgressBar("正在生成Lua文件", "Data.Lua", 1);
        //存储内容
        File.WriteAllText(LuaDir+"Data.Lua", sb.ToString());
    }
}