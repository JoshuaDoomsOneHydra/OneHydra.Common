using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static T ExecuteScalar<T>(this SqlConnection theConnection, string queryString)
        {
            var returnValue = default(T);
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var uncastValue = command.ExecuteScalar();
            if (uncastValue != null && uncastValue != DBNull.Value)
            {
                returnValue = (T)uncastValue;
            }
            theConnection.Close();
            return returnValue;
        }

        public static T ExecuteProcedureWithReturn<T>(this SqlConnection theConnection, string procedureName, SqlParameter[] parameters)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);
            var returnValueParam = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
            returnValueParam.Direction = ParameterDirection.ReturnValue;
            command.ExecuteNonQuery();
            var returnValue = default(T);
            if (returnValueParam.Value != DBNull.Value)
            {
                returnValue = returnValueParam.Value.CastTo<T>();
            }
            theConnection.Close();
            return returnValue;
        }

        public static int ExecuteNonQuery(this SqlConnection theConnection, string procedureName, SqlParameter[] parameters, int commandTimout = 0)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = commandTimout;
            command.Parameters.AddRange(parameters);
            var returnValue = command.ExecuteNonQuery();
            theConnection.Close();
            return returnValue;
        }

        public static int ExecuteNonQuery(this SqlConnection theConnection, string commandText, int commandTimeout = 0)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = commandTimeout;
            int value = command.ExecuteNonQuery();
            theConnection.Close();
            return value;
        }

        public static int ExecuteNonQueryAsTransaction(this SqlConnection theConnection, string commandText)
        {
            var rowsAffected = -1;
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = WrapSqlWithinTransactionAndTryCatch(commandText);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                // It was successful and we need to get the rows affected
                if (reader.VisibleFieldCount == 1 && reader.Read())
                {
                    rowsAffected = reader.GetInt32(0);
                    theConnection.Close();
                }
                else  // We have an error in the transaction and it was rolled back.
                {
                    if (reader.Read())
                    {

                        var errorStringBuilder = new StringBuilder();
                        var procedure = reader.GetValue(3);//ERROR_PROCEDURE() AS ErrorProcedure,
                        var sqlErrorMessage = reader.GetValue(5);//ERROR_MESSAGE() AS ErrorMessage;
                        errorStringBuilder.AppendLine("An error occured while executing the sql.");
                        errorStringBuilder.AppendLine("ErrorNumber = " + reader.GetInt32(0)); //ERROR_NUMBER() AS ErrorNumber,
                        errorStringBuilder.AppendLine("ErrorSeverity = " + reader.GetInt32(1));//ERROR_SEVERITY() AS ErrorSeverity,
                        errorStringBuilder.AppendLine("ErrorState = " + reader.GetInt32(2));//ERROR_STATE() AS ErrorState,
                        errorStringBuilder.AppendLine("ErrorProcedure = " + (procedure != DBNull.Value ? procedure.ToString() : string.Empty));
                        errorStringBuilder.AppendLine("ErrorLine = " + reader.GetInt32(4));//ERROR_LINE() AS ErrorLine,
                        errorStringBuilder.AppendLine("ErrorMessage = " + (sqlErrorMessage != DBNull.Value ? sqlErrorMessage.ToString() : string.Empty));
                        errorStringBuilder.AppendLine("CommandText = " + commandText);
                        theConnection.Close();
                        throw new Exception(errorStringBuilder.ToString());

                    }
                }
            }
            return rowsAffected;
        }

        public static string GetExcelXml(this SqlConnection theConnection, string queryString)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var reader = command.ExecuteReader();
            var baseExcel = GetBaseExcelDocument();
            var dataStringBuilder = new StringBuilder();
            var columnCount = 1;
            var rowCount = 0;
            if (reader.HasRows)
            {
                var schema = reader.GetSchemaTable();
                var typeArray = new List<String>();
                columnCount = reader.FieldCount;
                if (schema != null)
                {
                    dataStringBuilder.Append("<Row>");
                    for (var x = 0; x < columnCount; x++)
                    {
                        var headerValue = schema.Rows[x][0].ToString();
                        dataStringBuilder.Append(GetExcelCell(headerValue, "String", true));
                        var typeCode = Type.GetTypeCode((Type)schema.Rows[x]["DataType"]);
                        switch (typeCode)
                        {
                            case TypeCode.Boolean:
                                typeArray.Add("Boolean");
                                break;
                            case TypeCode.Decimal:
                            case TypeCode.Double:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                                typeArray.Add("Number");
                                break;
                            case TypeCode.DateTime:
                                typeArray.Add("DateTime");
                                break;
                            default:
                                typeArray.Add("String");
                                break;
                        }
                    }
                    dataStringBuilder.Append("</Row>");
                    rowCount++;
                }
                while (reader.Read())
                {
                    dataStringBuilder.Append("<Row>");
                    for (var x = 0; x < columnCount; x++)
                    {
                        var cellValue = reader.GetValue(x).ToString();
                        var dataType = (typeArray.Count > 0 ? typeArray[x] : "String");
                        dataStringBuilder.Append(GetExcelCell(cellValue, dataType));
                    }
                    dataStringBuilder.Append("</Row>");
                    rowCount++;
                }
                reader.Close();
            }
            if (rowCount == 0)
            {
                columnCount = 1;
                rowCount = 1;
                dataStringBuilder.Append("<Row>" + GetExcelCell("No rows found!", "String", true) + "</Row>");
            }
            var tableXml = GetExcelTable(columnCount, rowCount, dataStringBuilder.ToString().Replace("{", "{{").Replace("}", "}}"));
            baseExcel = string.Format(baseExcel, tableXml);
            theConnection.Close();
            return baseExcel;
        }

        public static T GetObject<T>(this SqlConnection theConnection, string queryString, Func<IDataReader, T> functionToPopulateObject, bool functionHandlesRead = false)
        {
            var returnObject = default(T);
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                if (!functionHandlesRead)
                {
                    reader.Read();
                }
                returnObject = functionToPopulateObject(reader);
                reader.Close();
            }
            theConnection.Close();
            return returnObject;
        }

        public static SqlDataReader GetReader(this SqlConnection theConnection, string queryString, int timeout = 0)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            command.CommandTimeout = timeout;
            var reader = command.ExecuteReader();
            return reader;
        }

        public static DataTable GetTable(this SqlConnection theConnection, string queryString, int timeout = 0)
        {
            var returnTable = new DataTable();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            command.CommandTimeout = timeout;
            var adapter = new SqlDataAdapter(queryString, theConnection) { SelectCommand = { CommandTimeout = timeout } };
            adapter.Fill(returnTable);
            return returnTable;
        }

        public static List<T> GetObjects<T>(this SqlConnection theConnection, string queryString, Func<IDataReader, T> functionToPopulateObject, int timeout = 0)
        {
            var returnList = new List<T>();
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            command.CommandTimeout = timeout;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    returnList.Add(functionToPopulateObject(reader));
                }
                reader.Close();
            }
            theConnection.Close();
            return returnList;
        }

        public static List<T> GetObjects<T>(this SqlConnection theConnection, string queryString, Func<IDataReader, List<T>> functionToPopulateList)
        {
            var returnList = new List<T>();
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                returnList = functionToPopulateList(reader);
            }
            if (!reader.IsClosed)
            {
                reader.Close();
            }

            theConnection.Close();
            return returnList;
        }

        public static List<T> GetObjects<T>(this SqlConnection theConnection, string procedureName, SqlParameter[] parameters, Func<IDataReader, T> functionToPopulateObject)
        {
            var returnList = new List<T>();
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);
            var returnValueParam = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
            returnValueParam.Direction = ParameterDirection.ReturnValue;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    returnList.Add(functionToPopulateObject(reader));
                }
                reader.Close();
            }
            theConnection.Close();
            return returnList;
        }

        public static Dictionary<TK, TV> GetDictionary<TK, TV>(this SqlConnection theConnection, string queryString, Func<IDataReader, KeyValuePair<TK, TV>> functionToPopulateObject)
        {
            var returnList = new Dictionary<TK, TV>();
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var keyValuePair = functionToPopulateObject(reader);
                    returnList.Add(keyValuePair.Key, keyValuePair.Value);
                }
                reader.Close();
            }
            theConnection.Close();
            return returnList;
        }

        public static Dictionary<TK, IList<TV>> GetDictionaryList<TK, TV>(this SqlConnection theConnection, string queryString, Func<IDataReader, KeyValuePair<TK, TV>> functionToPopulateObject)
        {
            var returnList = new Dictionary<TK, IList<TV>>();
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = queryString;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var keyValuePair = functionToPopulateObject(reader);
                    if (returnList.ContainsKey(keyValuePair.Key))
                    {
                        returnList[keyValuePair.Key].Add(keyValuePair.Value);
                    }
                    else
                    {
                        returnList.Add(keyValuePair.Key, new List<TV> { keyValuePair.Value });
                    }
                }
                reader.Close();
            }
            theConnection.Close();
            return returnList;
        }

        private static string WrapSqlWithinTransactionAndTryCatch(string sqlStatement, string transactionName)
        {
            return
                string.Format(
                    @"
            DECLARE @TransactionName varchar(20);
            SET @TransactionName = '{0}'
            BEGIN TRAN @TransactionName;
            BEGIN TRY

            {1}

            SELECT @@RowCount AS RowsAffected
            COMMIT TRAN @TransactionName;
            END TRY
            BEGIN CATCH 
               ROLLBACK TRAN @TransactionName;
               SELECT
               ERROR_NUMBER() AS ErrorNumber,
               ERROR_SEVERITY() AS ErrorSeverity,
               ERROR_STATE() AS ErrorState,
               ERROR_PROCEDURE() AS ErrorProcedure,
               ERROR_LINE() AS ErrorLine,
               ERROR_MESSAGE() AS ErrorMessage;
            END CATCH", transactionName, sqlStatement);
        }

        private static string WrapSqlWithinTransactionAndTryCatch(string sqlStatement)
        {
            var rand = new Random(36);
            var randomTransationName = "transaction" + rand.Next(5000);
            return WrapSqlWithinTransactionAndTryCatch(sqlStatement, randomTransationName);
        }

        private static string GetExcelTable(int columnCount, int rowCount, string rowData)
        {
            return
            string.Format(@"<Table ss:ExpandedColumnCount=""{0}"" ss:ExpandedRowCount=""{1}"" x:FullColumns=""1"" x:FullRows=""1"" ss:DefaultRowHeight=""15"">
                {2}
            </Table>", columnCount, rowCount, rowData);
        }

        private static string GetExcelCell(string value, string dataType, bool isHeader = false)
        {
            value = value.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("'", "&apos;").Replace("\"", "&quot;");
            var typeString = @"ss:Type=""String""";
            if (!isHeader)
            {
                typeString = string.Format(@"ss:Type=""{0}""", dataType);
                if (dataType == "Boolean")
                {
                    value = (value == "True" ? "1" : "0");
                }
                else if (dataType == "Number")
                {
                    value = (string.IsNullOrEmpty(value) ? "0" : value);
                }
                else if (dataType == "DateTime" && string.IsNullOrEmpty(value))
                {
                    typeString = @"ss:Type=""String""";
                }
            }
            return string.Format(@"<Cell><Data {1}>{0}</Data></Cell>", value, typeString);
        }

        private static string GetBaseExcelDocument()
        {
            return @"<?xml version=""1.0""?>
<?mso-application progid=""Excel.Sheet""?>
<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:o=""urn:schemas-microsoft-com:office:office""
 xmlns:x=""urn:schemas-microsoft-com:office:excel""
 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:html=""http://www.w3.org/TR/REC-html40"">
 <DocumentProperties xmlns=""urn:schemas-microsoft-com:office:office"">
  <Author>Josh Dooms</Author>
  <LastAuthor>Josh Dooms</LastAuthor>
  <Created>2012-09-25T19:00:30Z</Created>
  <LastSaved>2012-09-26T08:21:24Z</LastSaved>
  <Version>14.00</Version>
 </DocumentProperties>
 <OfficeDocumentSettings xmlns=""urn:schemas-microsoft-com:office:office"">
  <AllowPNG/>
 </OfficeDocumentSettings>
 <ExcelWorkbook xmlns=""urn:schemas-microsoft-com:office:excel"">
  <WindowHeight>12585</WindowHeight>
  <WindowWidth>27795</WindowWidth>
  <WindowTopX>480</WindowTopX>
  <WindowTopY>120</WindowTopY>
  <ProtectStructure>False</ProtectStructure>
  <ProtectWindows>False</ProtectWindows>
 </ExcelWorkbook>
 <Styles>
  <Style ss:ID=""Default"" ss:Name=""Normal"">
   <Alignment ss:Vertical=""Bottom""/>
   <Borders/>
   <Font ss:FontName=""Calibri"" x:Family=""Swiss"" ss:Size=""11"" ss:Color=""#000000""/>
   <Interior/>
   <NumberFormat/>
   <Protection/>
  </Style>
 </Styles>
 <Worksheet ss:Name=""Sheet1"">
{0}
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <Unsynced/>
   <Selected/>
   <Panes>
    <Pane>
     <Number>3</Number>
     <ActiveCol>2</ActiveCol>
    </Pane>
   </Panes>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
 <Worksheet ss:Name=""Sheet2"">
  <Table ss:ExpandedColumnCount=""1"" ss:ExpandedRowCount=""1"" x:FullColumns=""1""
   x:FullRows=""1"" ss:DefaultRowHeight=""15"">
   <Row ss:AutoFitHeight=""0""/>
  </Table>
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <Unsynced/>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
 <Worksheet ss:Name=""Sheet3"">
  <Table ss:ExpandedColumnCount=""1"" ss:ExpandedRowCount=""1"" x:FullColumns=""1""
   x:FullRows=""1"" ss:DefaultRowHeight=""15"">
   <Row ss:AutoFitHeight=""0""/>
  </Table>
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <Unsynced/>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
</Workbook>";
        }

        #region TestMethods

        public static List<T> GetRandomObjects<T>(this SqlConnection theConnection, string tableName, int numberOfObjects, Func<IDataReader, List<T>> functionToPopulateList)
        {
            var query = string.Format(@"SELECT TOP {0} * FROM {1} ORDER BY NEWID()", numberOfObjects, tableName);
            return GetObjects(theConnection, query, functionToPopulateList);
        }

        public static T ExecuteRandomScalar<T>(this SqlConnection theConnection, string columnName, string tableName)
        {
            theConnection.Open();
            var command = theConnection.CreateCommand();
            command.CommandText = string.Format(@"SELECT TOP 1 {0} FROM {1} ORDER BY NEWID()", columnName, tableName);
            var value = (T)command.ExecuteScalar();
            theConnection.Close();
            return value;
        }

        #endregion TestMethods
    }
}
