using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Colin.Core.IO
{
  public class CsvFile
  {
    public DataTable Table;

    public CsvFile(string path)
    {
      Table = new DataTable();
      using (FileStream fs = new FileStream(Path.ChangeExtension(path, ".csv"), FileMode.Open))
      {
        using (StreamReader reader = new StreamReader(fs))
        {
          string lineData;
          string[] datas;
          DataRow row;
          DataColumn column;

          lineData = reader.ReadLine();
          if (lineData is not null)
          {
            datas = ConvertCsvData(lineData);
            for (int count = 0; count < datas.Length; count++)
            {
              column = new DataColumn();
              column.Caption = datas[count];
              column.ColumnName = datas[count];
              column.AutoIncrement = false;
              column.ReadOnly = true;
              column.Unique = false;
              Table.Columns.Add(column);
            }
            row = Table.NewRow();
            for (int count = 0; count < datas.Length; count++)
            {
              row[count] = datas[count];
            }
            Table.Rows.Add(row);
          }

          while (true)
          {
            lineData = reader.ReadLine();
            if (lineData is null)
              break;
            else
            {
              row = Table.NewRow();
              datas = ConvertCsvData(lineData);
              for (int count = 0; count < datas.Length; count++)
              {
                row[count] = datas[count];
              }
              Table.Rows.Add(row);
            }
          }
        }
      }
    }

    public static string[] ConvertCsvData(string str)
    {
      bool flag = false;
      char temp;
      int next;
      string token = "";
      List<string> result = new List<string>();
      for (int i = 0; i < str.Length; i++)
      {
        temp = str[i];
        next = i + 1;
        if (temp is '"')
        {
          if (flag is false)
            flag = true;
          else if (next < str.Length && str[next] is '"')
          {
            token += '"';
            i += 1;
          }
          else
            flag = false;
        }
        else if (flag && temp is ',')
        {
          token += temp;
        }
        else
        {
          if (str[i] is ',')
          {
            result.Add(token);
            token = "";
          }
          else if (next >= str.Length)
          {
            token += str[i];
            result.Add(token);
            token = "";
          }
          else
            token += str[i];
        }
      }
      return result.ToArray();
    }
  }
}