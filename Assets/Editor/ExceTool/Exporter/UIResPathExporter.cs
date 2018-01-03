using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Excel;
using System.IO;
using System.Data;

public class UIResPathExporter
{

	public static UIResPathData ReadExcel()
	{
		string excelName = System.Environment.CurrentDirectory + "/配置表/UI资源配置表.xlsx";
		FileStream stream = File.Open(excelName, FileMode.Open, FileAccess.Read, FileShare.Read);
		IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
		DataSet result = excelReader.AsDataSet();

		System.Data.DataTable wookSheet = result.Tables["UIResPath"];
		int startRow = 5;
		int endRow = wookSheet.Rows.Count;
		int columnCount = wookSheet.Columns.Count;

		List<string> keys = new List<string>();
		for (int index = 0; index < columnCount; index++)
		{
			string keyText = wookSheet.Rows[2][index].ToString();
			if (!keyText.Equals(string.Empty))
			keys.Add(keyText);
		}
		var headerColumns = ExcelHelper.GetColumnsHeader(wookSheet, keys);
		UIResPathData Data = ScriptableObject.CreateInstance<UIResPathData>();
		for (int row = startRow; row < endRow; row++)
		{
			string checkStr = ExcelHelper.GetSheetValue(wookSheet, row, headerColumns[keys[0]]);
			if(checkStr.Equals(string.Empty))
				continue;
			UIResPathProperty property = new UIResPathProperty();
			property._resName = StrParser.ParseStr(ExcelHelper.GetSheetValue(wookSheet, row, headerColumns["resName"]), string.Empty);
			property._tagName = StrParser.ParseStr(ExcelHelper.GetSheetValue(wookSheet, row, headerColumns["tagName"]), string.Empty);
			property._res_develop = StrParser.ParseStr(ExcelHelper.GetSheetValue(wookSheet, row, headerColumns["res_develop"]), string.Empty);
			property._res_public = StrParser.ParseStr(ExcelHelper.GetSheetValue(wookSheet, row, headerColumns["res_public"]), string.Empty);
			property._abName = StrParser.ParseStr(ExcelHelper.GetSheetValue(wookSheet, row, headerColumns["abName"]), string.Empty);
			Data._properties.Add(property);
		}
		return Data;
	}
}
