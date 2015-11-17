/*
Copyright(c) 2015 Terry Aney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
    
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BTR.Core.Linq
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "BTR.Globalization", "MH1000:StringLiteralsShouldBeInResxOrIgnored", Justification = "Code, not text.", Target="*:MH1000:StringLiteralsShouldBeInResxOrIgnored" )]
	public static class LinqToSqlExtensions
	{
		/// <summary>
		/// Creates a *.csv file from an IQueryable query, dumping out the 'simple' properties/fields.
		/// </summary>
		/// <param name="query">Represents a SELECT query to execute.</param>
		/// <param name="fileName">The name of the file to create.</param>
		/// <remarks>
		/// <para>If the file specified by <paramref name="fileName"/> exists, it will be deleted.</para>
		/// <para>If the <paramref name="query"/> contains any properties that are entity sets (i.e. rows from a FK relationship) the values will not be dumped to the file.</para>
		/// <para>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</para>
		/// </remarks>
        public static T DumpCSV<T>( this T query, string fileName ) where T : System.Collections.IEnumerable
		{
			return query.DumpCSV( fileName, true, null );
		}

		/// <summary>
		/// Creates a *.csv file from an IQueryable query, dumping out the 'simple' properties/fields.
		/// </summary>
		/// <param name="query">Represents a SELECT query to execute.</param>
		/// <param name="fileName">The name of the file to create.</param>
        /// <param name="deleteFile">Whether or not to delete the file specified by <paramref name="fileName"/> if it exists.</param>
        /// <param name="translateValue">A call back function used to translate the value if the raw value needs to be massaged.</param>
        /// <remarks>
		/// <para>If the <paramref name="query"/> contains any properties that are entity sets (i.e. rows from a FK relationship) the values will not be dumped to the file.</para>
		/// <para>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</para>
		/// </remarks>
        public static T DumpCSV<T>( this T query, string fileName, bool deleteFile, Func<MemberInfo, object, object> translateValue = null ) where T : System.Collections.IEnumerable
        {
            if ( File.Exists( fileName ) && deleteFile )
            {
                File.Delete( fileName );
            }
            using ( var output = new FileStream( fileName, FileMode.CreateNew ) )
            {
                return query.DumpCSV( output, translateValue );
            }
        }

        public static T DumpCSV<T>( this T query, Stream stream, string delimitter = ",", Encoding encoding = null ) where T : System.Collections.IEnumerable
        {
            return query.DumpCSV( stream, null, delimitter, encoding );
        }

        private static T DumpCSV<T>( this T query, Stream stream, Func<MemberInfo, object, object> translateValue = null, string delimitter = ",", Encoding encoding = null ) where T : System.Collections.IEnumerable
		{
            using ( var writer = new StreamWriter( stream, encoding ?? new UTF8Encoding( false, true ) ) )
			{
				var firstRow = true;

				PropertyInfo[] properties = null;
				FieldInfo[] fields = null;
				Type type = null;
				bool typeIsAnonymous = false;

				foreach ( var r in query )
				{
					if ( type == null )
					{
						type = r.GetType();
						typeIsAnonymous = type.IsAnonymous();
                        properties = type.GetProperties().Where( p => IsCsvExportAllowed( p.PropertyType, (MemberInfo)p ) ).ToArray();
                        fields = type.GetFields().Where( p => IsCsvExportAllowed( p.FieldType, (MemberInfo)p ) ).ToArray();
					}

					var firstCol = true;

					if ( firstRow )
					{

                        foreach ( var p in properties )
                        {
                            if ( !firstCol ) writer.Write( delimitter );
                            else { firstCol = false; }

                            writer.Write( CsvHeaderName( (MemberInfo)p ) );
                        }

                        foreach ( var p in fields )
                        {
                            if ( !firstCol ) writer.Write( delimitter );
                            else { firstCol = false; }

                            writer.Write( CsvHeaderName( (MemberInfo)p ) );
                        }
							
						writer.WriteLine();
					}
					firstRow = false;
					firstCol = true;

                    foreach ( var p in properties )
					{
                        if ( !firstCol ) writer.Write( delimitter );
						else { firstCol = false; }

                        var value = translateValue != null
                            ? translateValue( p, r )
                            : p.GetValue( r, null );

                        DumpValue( value, writer, delimitter );
					}

                    foreach ( var p in fields )
                    {
                        if ( !firstCol ) writer.Write( delimitter );
                        else { firstCol = false; }

                        var value = translateValue != null
                            ? translateValue( p, r )
                            : p.GetValue( r );

                        DumpValue( value, writer, delimitter );
                    }

					writer.WriteLine();
				}
			}

            return query;
		}

        /// <summary>
        /// Returns all fields/properties from <paramref name="source"/> except for the field(s)/property(ies) listed in the selector expression.
        /// </summary>
        public static IQueryable SelectExcept<TSource, TResult>( this IEnumerable<TSource> source, Expression<Func<TSource, TResult>> selector )
        {
            var newExpression = selector.Body as NewExpression;

            var excludeProperties = newExpression != null
                    ? newExpression.Members.Select( m => m.Name )
                    : new[] { ( (MemberExpression)selector.Body ).Member.Name };
            
            var sourceType = typeof( TSource );
            var allowedSelectTypes = new Type[] { typeof( string ), typeof( ValueType ), typeof( XElement ) };
            var sourceProperties = sourceType.GetProperties( BindingFlags.Public | BindingFlags.Instance ).Where( p => allowedSelectTypes.Any( t => t.IsAssignableFrom( ( (PropertyInfo)p ).PropertyType ) ) ).Select( p => ( (MemberInfo)p ).Name );
            var sourceFields = sourceType.GetFields( BindingFlags.Public | BindingFlags.Instance ).Where( f => allowedSelectTypes.Any( t => t.IsAssignableFrom( ( (FieldInfo)f ).FieldType ) ) ).Select( f => ( (MemberInfo)f ).Name );

            var selectFields = sourceProperties.Concat( sourceFields ).Where( p => !excludeProperties.Contains( p ) ).ToArray();

            var dynamicSelect = 
                    string.Format( "new( {0} )",
                            string.Join( ", ", selectFields ) );

            return selectFields.Count() > 0
                ? source.AsQueryable().Select( dynamicSelect )
                : Enumerable.Empty<TSource>().AsQueryable<TSource>();
        }

        private static bool IsCsvExportAllowed( Type memberType, MemberInfo member )
        {
            var allowedCsvTypes = new Type[] { typeof( string ), typeof( ValueType ) };
            if ( !allowedCsvTypes.Any( t => t.IsAssignableFrom( memberType ) ) ) return false;

            var scaffoldColumnAttrib = member.GetCustomAttributes( typeof( ScaffoldColumnAttribute ), true ).Cast<ScaffoldColumnAttribute>().FirstOrDefault(); 
            // BTR Tahiti attribute.
            var xmlPropertyMappingAttr = member.GetCustomAttributes( true ).Cast<Attribute>().Where( a => a.GetType().Name == "XmlPropertyMappingAttribute" ).FirstOrDefault();
            
            return scaffoldColumnAttrib == null || scaffoldColumnAttrib.Scaffold || xmlPropertyMappingAttr != null;
        }

        /// <summary>
        /// Gets the header name.  Didn't directly reference BTR's Tahiti assemblies, but instead did late binding/reflection to determine if there.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static string CsvHeaderName( MemberInfo member )
        {
            var attributes = member.GetCustomAttributes( true ).Cast<Attribute>();
            
            var exportNameAttr = attributes.Where( a => a.GetType().Name == "ExportNameAttribute" ).FirstOrDefault();
            var xmlPropertyMappingAttr = attributes.Where( a => a.GetType().Name == "XmlPropertyMappingAttribute" ).FirstOrDefault();
            var displayAttr = attributes.Where( a => a.GetType().Name == "DisplayAttribute" ).FirstOrDefault();
            var authIdAttr = attributes.Where( a => a.GetType().Name == "AuthIdAttribute" ).FirstOrDefault();
            
            var name = exportNameAttr != null
                ? (string)exportNameAttr.GetType().GetProperty( "Name" ).GetValue( exportNameAttr, null )
                : xmlPropertyMappingAttr != null
                    ? (string)xmlPropertyMappingAttr.GetType().GetProperty( "XmlField" ).GetValue( xmlPropertyMappingAttr, null )
                    : displayAttr != null
                        ? (string)displayAttr.GetType().GetMethod( "GetName" ).Invoke( displayAttr, null ) 
                        : member.Name;

            return authIdAttr != null ? name + "/key" : name;
        }

        private static void DumpValue( object v, StreamWriter writer, string delimitter )
		{
			if ( v != null )
			{
				switch ( Type.GetTypeCode( v.GetType() ) )
				{
					// csv encode the value
                    case TypeCode.String:
                        string value = (string)v;

                        if ( value.Contains( delimitter ) || value.Contains( '"' ) || value.Contains( "\n" ) )
						{
							value = value.Replace( "\"", "\"\"" );

							if ( value.Length > 31735 )
							{
								value = value.Substring( 0, 31732 ) + "...";
							}
							writer.Write( "\"" + value + "\"" );
						}
						else
						{
							writer.Write( value );
						}
						break;
					
					default: writer.Write( v ); break;
				}
			}
		}

		public static bool IsAnonymous( this Type type )
		{
			if ( type == null )
				throw new ArgumentNullException( "type" );

			// HACK: The only way to detect anonymous types right now.
			return Attribute.IsDefined( type, typeof( CompilerGeneratedAttribute ), false )
					   && type.IsGenericType && type.Name.Contains( "AnonymousType" )
					   && ( type.Name.StartsWith( "<>" ) || type.Name.StartsWith( "VB$" ) )
					   && ( type.Attributes & TypeAttributes.NotPublic ) == TypeAttributes.NotPublic;

		}

		/// <summary>
		/// Batches together multiple IQueryable queries into a single DbCommand and returns all data in
		/// a single roundtrip to the database.
		/// </summary>
		/// <param name="context">The DataContext to execute the batch select against.</param>
		/// <param name="queries">Represents a collections of SELECT queries to execute.</param>
		/// <returns>Returns an IMultipleResults object containing all results.</returns>
		public static IMultipleResults SelectMutlipleResults( this DataContext context, IQueryable[] queries )
		{
			var commandList = new List<DbCommand>();
			
			foreach ( IQueryable query in queries )
			{
				var command = context.GetCommand( query );
				commandList.Add( command );
			}

			SqlCommand batchCommand = CombineCommands( commandList );
			batchCommand.Connection = context.Connection as SqlConnection;

			DbDataReader dr = null;

			if ( batchCommand.Connection.State == ConnectionState.Closed )
			{
				batchCommand.Connection.Open();
				dr = batchCommand.ExecuteReader( CommandBehavior.CloseConnection );
			}
			else
			{
				dr = batchCommand.ExecuteReader();
			}

			IMultipleResults mr = context.Translate( dr );
			return mr;
		}

		/// <summary>
		/// Combines multiple SELECT commands into a single SqlCommand so that all statements can be executed in a 
		/// single roundtrip to the database and return multiple result sets.
		/// </summary>
		/// <param name="commandList">Represents a collection of commands to be batched together.</param>
		/// <returns>Returns a single SqlCommand that executes all SELECT statements at once.</returns>
		private static SqlCommand CombineCommands( List<DbCommand> selectCommands )
		{
			SqlCommand batchCommand = new SqlCommand();
			SqlParameterCollection newParamList = batchCommand.Parameters;
		
			int commandCount = 0;
			
			foreach ( DbCommand cmd in selectCommands )
			{
				string commandText = cmd.CommandText;
				DbParameterCollection paramList = cmd.Parameters;
				int paramCount = paramList.Count;
			
				for ( int currentParam = paramCount - 1; currentParam >= 0; currentParam-- )
				{
					DbParameter param = paramList[ currentParam ];
					DbParameter newParam = CloneParameter( param );
					string newParamName = param.ParameterName.Replace( "@", string.Format( "@{0}_", commandCount ) );
					commandText = commandText.Replace( param.ParameterName, newParamName );
					newParam.ParameterName = newParamName;
					newParamList.Add( newParam );
				}
				if ( batchCommand.CommandText.Length > 0 )
				{
					batchCommand.CommandText += ";";
				}
				batchCommand.CommandText += commandText;
				commandCount++;
			}
			
			return batchCommand;
		}

		/// <summary>
		/// Returns a clone (via copying all properties) of an existing DbParameter.
		/// </summary>
		/// <param name="src">The DbParameter to clone.</param>
		/// <returns>Returns a clone (via copying all properties) of an existing DbParameter.</returns>
		private static DbParameter CloneParameter( DbParameter src )
		{
			SqlParameter source = (SqlParameter)src;
			SqlParameter destination = new SqlParameter();

			destination.Value = source.Value;
			destination.Direction = source.Direction;
			destination.Size = source.Size;
			destination.Offset = source.Offset;
			destination.SourceColumn = source.SourceColumn;
			destination.SourceVersion = source.SourceVersion;
			destination.SourceColumnNullMapping = source.SourceColumnNullMapping;
			destination.IsNullable = source.IsNullable;

			destination.CompareInfo = source.CompareInfo;
			destination.XmlSchemaCollectionDatabase = source.XmlSchemaCollectionDatabase;
			destination.XmlSchemaCollectionOwningSchema = source.XmlSchemaCollectionOwningSchema;
			destination.XmlSchemaCollectionName = source.XmlSchemaCollectionName;
			destination.UdtTypeName = source.UdtTypeName;
			destination.TypeName = source.TypeName;
			destination.ParameterName = source.ParameterName;
			destination.Precision = source.Precision;
			destination.Scale = source.Scale;

			return destination;
		}

		/// <summary>
		/// Immediately deletes all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <typeparam name="TPrimaryKey">Represents the object type for the primary key of rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="primaryKey">Represents the primary key of the item to be removed from <paramref name="table"/>.</param>
		/// <returns>The number of rows deleted from the database (maximum of 1).</returns>
		/// <remarks>
		/// <para>If the primary key for <paramref name="table"/> is a composite key, <paramref name="primaryKey"/> should be an anonymous type with property names mapping to the property names of objects of type <typeparamref name="TEntity"/>.</para>
		/// </remarks>
		public static int DeleteByPK<TEntity>( this Table<TEntity> table, object primaryKey ) where TEntity : class
		{
			DbCommand delete = table.GetDeleteByPKCommand<TEntity>( primaryKey );

			var parameters = from p in delete.Parameters.Cast<DbParameter>()
							 select p.Value;

			return table.Context.ExecuteCommand( delete.CommandText, parameters.ToArray() );
		}

		/// <summary>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete a entity row via the supplied primary key.</summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <typeparam name="TPrimaryKey">Represents the object type for the primary key of rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="primaryKey">Represents the primary key of the item to be removed from <paramref name="table"/>.</param>
		/// <returns>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete a entity row via the supplied primary key.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string DeleteByPKPreview<TEntity>( this Table<TEntity> table, object primaryKey ) where TEntity : class
		{
			DbCommand delete = table.GetDeleteByPKCommand<TEntity>( primaryKey );
			return delete.PreviewCommandText( false ) + table.Context.GetLog();
		}

		private static DbCommand GetDeleteByPKCommand<TEntity>( this Table<TEntity> table, object primaryKey ) where TEntity : class
		{
			Type type = primaryKey.GetType();
			bool typeIsAnonymous = type.IsAnonymous();
			string dbName = table.GetDbName();

			var metaTable = table.Context.Mapping.GetTable( typeof( TEntity ) );

			var keys = from mdm in metaTable.RowType.DataMembers
					   where mdm.IsPrimaryKey
					   select new { mdm.MappedName, mdm.Name, mdm.Type };

			SqlCommand deleteCommand = new SqlCommand();
			deleteCommand.Connection = table.Context.Connection as SqlConnection;

			var whereSB = new StringBuilder();

			foreach ( var key in keys )
			{
				// Add new parameter with massaged name to avoid clashes.
				whereSB.AppendFormat( "[{0}] = @p{1}, ", key.MappedName, deleteCommand.Parameters.Count );

				object value = primaryKey;
				if ( typeIsAnonymous || ( type.IsClass && type != typeof( string ) ) )
				{
					if ( typeIsAnonymous )
					{
						PropertyInfo property = type.GetProperty( key.Name );

						if ( property == null )
						{
							throw new ArgumentOutOfRangeException( string.Format( "The property {0} which is defined as part of the primary key for {1} was not supplied by the parameter primaryKey.", key.Name, metaTable.TableName ) );
						}

						value = property.GetValue( primaryKey, null );
					}
					else
					{
						FieldInfo field = type.GetField( key.Name );

						if ( field == null )
						{
							throw new ArgumentOutOfRangeException( string.Format( "The property {0} which is defined as part of the primary key for {1} was not supplied by the parameter primaryKey.", key.Name, metaTable.TableName ) );
						}

						value = field.GetValue( primaryKey );
					}

					if ( value.GetType() != key.Type )
					{
						throw new InvalidCastException( string.Format( "The property {0} ({1}) does not have the same type as {2} ({3}).", key.Name, value.GetType(), key.MappedName, key.Type ) );
					}
				}
				else if ( value.GetType() != key.Type )
				{
					throw new InvalidCastException( string.Format( "The value supplied in primaryKey ({0}) does not have the same type as {1} ({2}).", value.GetType(), key.MappedName, key.Type ) );
				}

				deleteCommand.Parameters.Add( new SqlParameter( string.Format( "@p{0}", deleteCommand.Parameters.Count ), value ) );
			}

			string wherePK = whereSB.ToString();

			if ( wherePK == "" )
			{
				throw new MissingPrimaryKeyException( string.Format( "{0} does not have a primary key defined.  Batch updating/deleting can not be used for tables without a primary key.", metaTable.TableName ) );
			}

			deleteCommand.CommandText = string.Format( "DELETE {0}\r\nWHERE {1}", dbName, wherePK.Substring( 0, wherePK.Length - 2 ) );

			return deleteCommand;
		}

		/// <summary>
		/// Immediately deletes all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="entities">Represents the collection of items which are to be removed from <paramref name="table"/>.</param>
		/// <returns>The number of rows deleted from the database.</returns>
		/// <remarks>
		/// <para>Similiar to stored procedures, and opposite from DeleteAllOnSubmit, rows provided in <paramref name="entities"/> will be deleted immediately with no need to call <see cref="DataContext.SubmitChanges()"/>.</para>
		/// <para>Additionally, to improve performance, instead of creating a delete command for each item in <paramref name="entities"/>, a single delete command is created.</para>
		/// </remarks>
		public static int DeleteBatch<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities ) where TEntity : class
		{
			DbCommand delete = table.GetDeleteBatchCommand<TEntity>( entities );

			var parameters = from p in delete.Parameters.Cast<DbParameter>()
							 select p.Value;

			return table.Context.ExecuteCommand( delete.CommandText, parameters.ToArray() );
		}

		/// <summary>
		/// Immediately deletes all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <returns>The number of rows deleted from the database.</returns>
		/// <remarks>
		/// <para>Similiar to stored procedures, and opposite from DeleteAllOnSubmit, rows provided in <paramref name="entities"/> will be deleted immediately with no need to call <see cref="DataContext.SubmitChanges()"/>.</para>
		/// <para>Additionally, to improve performance, instead of creating a delete command for each item in <paramref name="entities"/>, a single delete command is created.</para>
		/// </remarks>
		public static int DeleteBatch<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter ) where TEntity : class
		{
			return table.DeleteBatch( table.Where( filter ) );
		}

		/// <summary>
		/// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="entities">Represents the collection of items which are to be removed from <paramref name="table"/>.</param>
		/// <returns>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string DeleteBatchPreview<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities ) where TEntity : class
		{
			DbCommand delete = table.GetDeleteBatchCommand<TEntity>( entities );
			return "Total Rows To Be Deleted By Query: " + entities.Count() + "\n\n" + delete.PreviewCommandText( false ) + table.Context.GetLog();
		}

		/// <summary>
		/// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <returns>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string DeleteBatchPreview<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter ) where TEntity : class
		{
			return table.DeleteBatchPreview( table.Where( filter ) );
		}

		/// <summary>
		/// Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="entities">Represents the collection of items which are to be removed from <paramref name="table"/>.</param>
		/// <returns>Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.</returns>
		/// <remarks>This method is useful for debugging purposes or when LINQ generated queries need to be passed to developers without LINQ/LINQPad.</remarks>
		public static string DeleteBatchSQL<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities ) where TEntity : class
		{
			DbCommand delete = table.GetDeleteBatchCommand<TEntity>( entities );
			return delete.PreviewCommandText( true );
		}

		/// <summary>
		/// Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <returns>Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to delete all entities from the collection with a single delete command.</returns>
		/// <remarks>This method is useful for debugging purposes or when LINQ generated queries need to be passed to developers without LINQ/LINQPad.</remarks>
		public static string DeleteBatchSQL<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter ) where TEntity : class
		{
			return table.DeleteBatchSQL( table.Where( filter ) );
		}

		/// <summary>
		/// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="entities">Represents the collection of items which are to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string UpdateBatchPreview<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			DbCommand update = table.GetUpdateBatchCommand<TEntity>( entities, evaluator );
            return "Total Rows To Be Updated By Query: " + entities.Count() + "\n\n" + update.PreviewCommandText( false ) + table.Context.GetLog();
		}

		/// <summary>
		/// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string UpdateBatchPreview<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			return table.UpdateBatchPreview( table.Where( filter ), evaluator );
		}

		/// <summary>
		/// Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="entities">Represents the collection of items which are to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.</returns>
		/// <remarks>This method is useful for debugging purposes or when LINQ generated queries need to be passed to developers without LINQ/LINQPad.</remarks>
		public static string UpdateBatchSQL<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			DbCommand update = table.GetUpdateBatchCommand<TEntity>( entities, evaluator );
			return update.PreviewCommandText( true );
		}

		/// <summary>
		/// Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to update all entities from the collection with a single update command.</returns>
		/// <remarks>This method is useful for debugging purposes or when LINQ generated queries need to be passed to developers without LINQ/LINQPad.</remarks>
		public static string UpdateBatchSQL<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			return table.UpdateBatchSQL( table.Where( filter ), evaluator );
		}

		/// <summary>
		/// Immediately updates all entities in the collection with a single update command based on a <typeparamref name="TEntity"/> created from a Lambda expression.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="entities">Represents the collection of items which are to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>The number of rows updated in the database.</returns>
		/// <remarks>
		/// <para>Similiar to stored procedures, and opposite from similiar InsertAllOnSubmit, rows provided in <paramref name="entities"/> will be updated immediately with no need to call <see cref="DataContext.SubmitChanges()"/>.</para>
		/// <para>Additionally, to improve performance, instead of creating an update command for each item in <paramref name="entities"/>, a single update command is created.</para>
		/// </remarks>
		public static int UpdateBatch<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			DbCommand update = table.GetUpdateBatchCommand<TEntity>( entities, evaluator );

			var parameters = from p in update.Parameters.Cast<DbParameter>()
							 select p.Value;
			return table.Context.ExecuteCommand( update.CommandText, parameters.ToArray() );
		}

		/// <summary>
		/// Immediately updates all entities in the collection with a single update command based on a <typeparamref name="TEntity"/> created from a Lambda expression.
		/// </summary>
		/// <typeparam name="TEntity">Represents the object type for rows contained in <paramref name="table"/>.</typeparam>
		/// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be updated.</param>
		/// <param name="filter">Represents a filter of items to be updated in <paramref name="table"/>.</param>
		/// <param name="evaluator">A Lambda expression returning a <typeparamref name="TEntity"/> that defines the update assignments to be performed on each item in <paramref name="entities"/>.</param>
		/// <returns>The number of rows updated in the database.</returns>
		/// <remarks>
		/// <para>Similiar to stored procedures, and opposite from similiar InsertAllOnSubmit, rows provided in <paramref name="entities"/> will be updated immediately with no need to call <see cref="DataContext.SubmitChanges()"/>.</para>
		/// <para>Additionally, to improve performance, instead of creating an update command for each item in <paramref name="entities"/>, a single update command is created.</para>
		/// </remarks>
		public static int UpdateBatch<TEntity>( this Table<TEntity> table, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			return table.UpdateBatch( table.Where( filter ), evaluator );
		}

		/// <summary>
		/// Returns the Transact SQL string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to perform the query's select statement.
		/// </summary>
		/// <param name="context">The DataContext to execute the batch select against.</param>
		/// <param name="query">Represents the SELECT query to execute.</param>
		/// <returns>Returns the Transact SQL string representation of the <see cref="DbCommand.CommandText"/> along with <see cref="DbCommand.Parameters"/> if present.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string PreviewSQL( this DataContext context, IQueryable query )
		{
			var cmd = context.GetCommand( query );
			return cmd.PreviewCommandText( true );
		}

		/// <summary>
		/// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to perform the query's select statement.
		/// </summary>
		/// <param name="context">The DataContext to execute the batch select against.</param>
		/// <param name="query">Represents the SELECT query to execute.</param>
		/// <returns>Returns a string representation of the <see cref="DbCommand.CommandText"/> along with <see cref="DbCommand.Parameters"/> if present.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		public static string PreviewCommandText( this DataContext context, IQueryable query )
		{
			var cmd = context.GetCommand( query );
			return cmd.PreviewCommandText( false);
		}

        /// <summary>
        /// Returns a string representation the LINQ <see cref="IProvider"/> command text and parameters used that would be issued to perform the query's select statement.
        /// </summary>
        /// <param name="context">The DataContext to execute the batch select against.</param>
        /// <param name="query">Represents the SELECT query to execute.</param>
        /// <returns>Returns a string representation of the <see cref="DbCommand.CommandText"/> along with <see cref="DbCommand.Parameters"/> if present.</returns>
        /// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
        public static string TranslateToSqlCommand( this DataContext context, IQueryable query )
        {
            var cmd = context.GetCommand( query );
            var output = new StringBuilder();

            var statements = cmd.CommandText.Replace( "\r\n", "\r" ).Split( '\r' );

            statements = statements.Take( 1 ).Select( s => "\"" + s + "\"" ).Concat(
                            statements.Skip( 1 ).Select( s => "              \"" + s + "\"" ) ).ToArray();

            output.AppendLine( "#region - SQL -\r\n" );
            output.AppendLine( string.Format( "var sqlText = {0};", string.Join( " +\r\n", statements ) ) );
            output.AppendLine( "#endregion\r\n" );

            output.AppendLine( "using ( var connection = new SqlConnection( ... ) )" );
            output.AppendLine( "{" );
                output.AppendLine( "\tusing ( var command = new SqlCommand( sqlText, connection ) )" );
                output.AppendLine( "\t{" );

                if ( cmd.Parameters.Count > 0 )
                {
                    foreach ( DbParameter p in cmd.Parameters )
                    {
                        var p2 = p as SqlParameter;
                        output.AppendLine( string.Format( "\t\tcommand.Parameters.Add( new SqlParameter( \"{0}\", {1} ) );", p.ParameterName, GetParameterTransactValue( p, p2, true ) ) );
                    }
                    output.AppendLine( "" );

                    output.AppendLine( "\t\t\tusing ( var reader = command.ExecuteReader() )" );
                    output.AppendLine( "\t\t\t{" );
                    output.AppendLine( "\t\t\t}" );
                }
                
                output.AppendLine( "\t}" );
            output.AppendLine( "}" );

            return output.ToString();
        }
        
        /// <summary>
		/// Returns a string representation of the <see cref="DbCommand.CommandText"/> along with <see cref="DbCommand.Parameters"/> if present.
		/// </summary>
		/// <param name="cmd">The <see cref="DbCommand"/> to analyze.</param>
		/// <param name="forTransactSQL">Whether or not the text should be formatted as 'logging' similiar to LINQ to SQL output, or in valid Transact SQL syntax ready for use with a 'query analyzer' type tool.</param>
		/// <returns>Returns a string representation of the <see cref="DbCommand.CommandText"/> along with <see cref="DbCommand.Parameters"/> if present.</returns>
		/// <remarks>This method is useful for debugging purposes or when used in other utilities such as LINQPad.</remarks>
		private static string PreviewCommandText( this DbCommand cmd, bool forTransactSQL )
		{
			var output = new StringBuilder();
			
			if ( !forTransactSQL ) output.AppendLine( cmd.CommandText );

			foreach ( DbParameter parameter in cmd.Parameters )
			{
				int num = 0;
				int num2 = 0;
				PropertyInfo property = parameter.GetType().GetProperty( "Precision" );
				if ( property != null )
				{
					num = (int)Convert.ChangeType( property.GetValue( parameter, null ), typeof( int ), CultureInfo.InvariantCulture );
				}
				PropertyInfo info2 = parameter.GetType().GetProperty( "Scale" );
				if ( info2 != null )
				{
					num2 = (int)Convert.ChangeType( info2.GetValue( parameter, null ), typeof( int ), CultureInfo.InvariantCulture );
				}
				SqlParameter parameter2 = parameter as SqlParameter;

				if ( forTransactSQL )
				{
					output.AppendFormat( "DECLARE {0} {1}{2}; SET {0} = {3}\r\n", 
						new object[] { 
								parameter.ParameterName, 
								( parameter2 == null ) ? parameter.DbType.ToString() : parameter2.SqlDbType.ToString(), 
								( parameter.Size > 0 ) ? "( " + parameter.Size.ToString( CultureInfo.CurrentCulture ) + " )" : "", 
								GetParameterTransactValue( parameter, parameter2 ) } );
				}
				else
				{
					output.AppendFormat( "-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]\r\n", new object[] { parameter.ParameterName, parameter.Direction, ( parameter2 == null ) ? parameter.DbType.ToString() : parameter2.SqlDbType.ToString(), parameter.Size.ToString( CultureInfo.CurrentCulture ), num, num2, ( parameter2 == null ) ? parameter.Value : parameter2.SqlValue } );
				}
			}

			if ( forTransactSQL ) output.Append( "\r\n" + cmd.CommandText );

			return output.ToString();
		}

		private static string GetParameterTransactValue( DbParameter parameter, SqlParameter parameter2, bool forSqlConversion = false )
		{
			if ( parameter2 == null ) return parameter.Value.ToString(); // Not going to deal with NON SQL parameters.

			switch( parameter2.SqlDbType ) 
			{
				case SqlDbType.Char:
				case SqlDbType.Date:
				case SqlDbType.DateTime:
				case SqlDbType.DateTime2:
				case SqlDbType.NChar:
				case SqlDbType.NText:
				case SqlDbType.NVarChar:
				case SqlDbType.SmallDateTime:
				case SqlDbType.Text:
				case SqlDbType.VarChar:
				case SqlDbType.UniqueIdentifier:
					return string.Format( "{0}{1}{0}", forSqlConversion ? "\"" : "'", parameter2.SqlValue );

				default:
					return parameter2.SqlValue.ToString();
			}
		}

		private static DbCommand GetDeleteBatchCommand<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities ) where TEntity : class
		{
			var deleteCommand = table.Context.GetCommand( entities );
			deleteCommand.CommandText = string.Format( "DELETE {0}\r\n", table.GetDbName() ) + GetBatchJoinQuery<TEntity>( table, entities );
			return deleteCommand;
		}

		private static DbCommand GetUpdateBatchCommand<TEntity>( this Table<TEntity> table, IQueryable<TEntity> entities, Expression<Func<TEntity, TEntity>> evaluator ) where TEntity : class
		{
			var updateCommand = table.Context.GetCommand( entities );

			var setSB = new StringBuilder();
			int memberInitCount = 1;

			// Process the MemberInitExpression (there should only be one in the evaluator Lambda) to convert the expression tree
			// into a valid DbCommand.  The Visit<> method will only process expressions of type MemberInitExpression and requires
			// that a MemberInitExpression be returned - in our case we'll return the same one we are passed since we are building
			// a DbCommand and not 'really using' the evaluator Lambda.
			evaluator.Visit<MemberInitExpression>( delegate( MemberInitExpression expression )
			{
				if ( memberInitCount > 1 )
				{
					throw new NotImplementedException( "Currently only one MemberInitExpression is allowed for the evaluator parameter." );
				}
				memberInitCount++;

				setSB.Append( GetDbSetStatement<TEntity>( expression, table, updateCommand ) );
				
				return expression; // just return passed in expression to keep 'visitor' happy.
			} );

			// Complete the command text by concatenating bits together.
			updateCommand.CommandText = string.Format( "UPDATE {0}\r\n{1}\r\n\r\n{2}", 
															table.GetDbName(),									// Database table name
															setSB.ToString(),									// SET fld = {}, fld2 = {}, ...
															GetBatchJoinQuery<TEntity>( table, entities ) );	// Subquery join created from entities command text

			if ( updateCommand.CommandText.IndexOf( "[arg0]" ) >= 0 || updateCommand.CommandText.IndexOf( "NULL AS [EMPTY]" ) >= 0 )
			{
				// TODO (Chris): Probably a better way to determine this by using an visitor on the expression before the
				//				 var selectExpression = Expression.Call... method call (search for that) and see which funcitons
				//				 are being used and determine if supported by LINQ to SQL
				throw new NotSupportedException( string.Format( "The evaluator Expression<Func<{0},{0}>> has processing that needs to be performed once the query is returned (i.e. string.Format()) and therefore can not be used during batch updating.", table.GetType() ) );
			}

			return updateCommand;
		}

		private static string GetDbSetStatement<TEntity>( MemberInitExpression memberInitExpression, Table<TEntity> table, DbCommand updateCommand ) where TEntity : class
		{
			var entityType = typeof( TEntity );

			if ( memberInitExpression.Type != entityType )
			{
				throw new NotImplementedException( string.Format( "The MemberInitExpression is intializing a class of the incorrect type '{0}' and it should be '{1}'.", memberInitExpression.Type, entityType ) );
			}

			var setSB = new StringBuilder();

			var tableName = table.GetDbName();
			var metaTable = table.Context.Mapping.GetTable( entityType );
			// Used to look up actual field names when MemberAssignment is a constant,
			// need both the Name (matches the property name on LINQ object) and the
			// MappedName (db field name).
			var dbCols = from mdm in metaTable.RowType.DataMembers
						 select new { mdm.MappedName, mdm.Name };

			// Walk all the expression bindings and generate SQL 'commands' from them.  Each binding represents a property assignment
			// on the TEntity object initializer Lambda expression.
			foreach ( var binding in memberInitExpression.Bindings )
			{
				var assignment = binding as MemberAssignment;

				if ( binding == null )
				{
					throw new NotImplementedException( "All bindings inside the MemberInitExpression are expected to be of type MemberAssignment." );
				}

				// TODO (Document): What is this doing?  I know it's grabbing existing parameter to pass into Expression.Call() but explain 'why'
				//					I assume it has something to do with fact we can't just access the parameters of assignment.Expression?
				//					Also, any concerns of whether or not if there are two params of type entity type?
				ParameterExpression entityParam = null;
				assignment.Expression.Visit<ParameterExpression>( delegate( ParameterExpression p ) { if ( p.Type == entityType ) entityParam = p; return p; } );

				// Get the real database field name.  binding.Member.Name is the 'property' name of the LINQ object
				// so I match that to the Name property of the table mapping DataMembers.
				string name = binding.Member.Name;
				var dbCol = ( from c in dbCols
							  where c.Name == name
							  select c ).FirstOrDefault();

				if ( dbCol == null )
				{
					throw new ArgumentOutOfRangeException( name, string.Format( "The corresponding field on the {0} table could not be found.", tableName ) );
				}

				var mappedName = dbCol.MappedName.StartsWith( "[" ) ? dbCol.MappedName : string.Format( "[{0}]", dbCol.MappedName );

				// If entityParam is NULL, then no references to other columns on the TEntity row and need to eval 'constant' value...
				if ( entityParam == null )
				{
					// Compile and invoke the assignment expression to obtain the contant value to add as a parameter.
					var constant = Expression.Lambda( assignment.Expression, null ).Compile().DynamicInvoke();

					// use the MappedName from the table mapping DataMembers - that is field name in DB table.
					if ( constant == null )
					{
						setSB.AppendFormat( "{0} = null, ", mappedName );
					}
					else
					{
						// Add new parameter with massaged name to avoid clashes.
						setSB.AppendFormat( "{0} = @p{1}, ", mappedName, updateCommand.Parameters.Count );
						updateCommand.Parameters.Add( new SqlParameter( string.Format( "@p{0}", updateCommand.Parameters.Count ), constant ) );
					}
				}
				else
				{
					// TODO (Documentation): Explain what we are doing here again, I remember you telling me why we have to call but I can't remember now.
					// Wny are we calling Expression.Call and what are we passing it?  Below comments are just 'made up' and probably wrong.

					// Create a MethodCallExpression which represents a 'simple' select of *only* the assignment part (right hand operator) of
					// of the MemberInitExpression.MemberAssignment so that we can let the Linq Provider do all the 'sql syntax' generation for
					// us. 
					//
					// For Example: TEntity.Property1 = TEntity.Property1 + " Hello"
					// This selectExpression will be only dealing with TEntity.Property1 + " Hello"
					var selectExpression = Expression.Call(
												typeof( Queryable ),
												"Select",
												new Type[] { entityType, assignment.Expression.Type },

					// TODO (Documentation): How do we know there are only 'two' parameters?  And what is Expression.Lambda
						//						 doing?  I assume it's returning a type of assignment.Expression.Type to match above?

												Expression.Constant( table ),
												Expression.Lambda( assignment.Expression, entityParam ) );

					setSB.AppendFormat( "{0} = {1}, ",
											mappedName,
											GetDbSetAssignment( table, selectExpression, updateCommand, name ) );
				}
			}

			var setStatements = setSB.ToString();
			return "SET " + setStatements.Substring( 0, setStatements.Length - 2 ); // remove ', '
		}

		/// <summary>
		/// Some LINQ Query syntax is invalid because SQL (or whomever the provider is) can not translate it to its native language.  
		/// DataContext.GetCommand() does not detect this, only IProvider.Execute or IProvider.Compile call the necessary code to 
		/// check this.  This function invokes the IProvider.Compile to make sure the provider can translate the expression.
		/// </summary>
		/// <remarks>
		/// An example of a LINQ query that previously 'worked' in the *Batch methods but needs to throw an exception is something
		/// like the following:
		/// 
		/// var pay = 
		///		from h in HistoryData
		///		where h.his.Groups.gName == "Ochsner" && h.hisType == "pay"
		///		select h;
		///		
		/// HistoryData.UpdateBatchPreview( pay, h => new HistoryData { hisIndex = ( int.Parse( h.hisIndex ) - 1 ).ToString() } ).Dump();
		/// 
		/// The int.Parse is not valid and needs to throw an exception like: 
		/// 
		///		Could not translate expression '(Parse(p.hisIndex) - 1).ToString()' into SQL and could not treat it as a local expression.
		///		
		///	Unfortunately, the IProvider.Compile is internal and I need to use Reflection to call it (ugh).  I've several e-mails sent into
		///	MS LINQ team members and am waiting for a response and will correct/improve code as soon as possible.
		/// </remarks>
		private static void ValidateExpression( ITable table, Expression expression )
		{
			// Simply compile the expression to see if it will work.
			// Needed to change this to use expression tree.  Original code (below) could fail with DataContextInterceptor.

			var compile = Expression.Call(
				Expression.Property( Expression.Constant( table.Context ), "Provider" ),
				"Compile",
				null,
				Expression.Constant( expression ) );

			Expression.Lambda<Action>( compile ).Compile()();
/*
			var context = table.Context;
			PropertyInfo providerProperty = context.GetType().GetProperty( "Provider", BindingFlags.Instance | BindingFlags.NonPublic );
			var provider = providerProperty.GetValue( context, null );
			var compileMI = provider.GetType().GetMethod( "System.Data.Linq.Provider.IProvider.Compile", BindingFlags.Instance | BindingFlags.NonPublic );

			compileMI.Invoke( provider, new object[] { expression } );*/
		}

		private static string GetDbSetAssignment( ITable table, MethodCallExpression selectExpression, DbCommand updateCommand, string bindingName )
		{
			ValidateExpression( table, selectExpression );

			// Convert the selectExpression into an IQueryable query so that I can get the CommandText
			var selectQuery = ( table as IQueryable ).Provider.CreateQuery( selectExpression );

			// Get the DbCommand so I can grab relavent parts of CommandText to construct a field 
			// assignment and based on the 'current TEntity row'.  Additionally need to massage parameter 
			// names from temporary command when adding to the final update command.
			var selectCmd = table.Context.GetCommand( selectQuery );
			var selectStmt = selectCmd.CommandText;
			selectStmt = selectStmt.Substring( 7,									// Remove 'SELECT ' from front ( 7 )
										selectStmt.IndexOf( "\r\nFROM " ) - 7 )		// Return only the selection field expression
									.Replace( "[t0].", "" )							// Remove table alias from the select
									.Replace( " AS [value]", "" )					// If the select is not a direct field (constant or expression), remove the field alias
									.Replace( "@p", "@p" + bindingName );			// Replace parameter name so doesn't conflict with existing ones.

			foreach ( var selectParam in selectCmd.Parameters.Cast<DbParameter>() )
			{
				var paramName = string.Format( "@p{0}", updateCommand.Parameters.Count );

				// DataContext.ExecuteCommand ultimately just takes a object array of parameters and names them p0-N.  
				// So I need to now do replaces on the massaged value to get it in proper format.
				selectStmt = selectStmt.Replace( 
									selectParam.ParameterName.Replace( "@p", "@p" + bindingName ), 
									paramName );

				updateCommand.Parameters.Add( new SqlParameter( paramName, selectParam.Value ) );
			}

			return selectStmt;
		}

		private static string GetBatchJoinQuery<TEntity>( Table<TEntity> table, IQueryable<TEntity> entities ) where TEntity : class
		{
			var metaTable = table.Context.Mapping.GetTable( typeof( TEntity ) );

			var keys = from mdm in metaTable.RowType.DataMembers
					   where mdm.IsPrimaryKey
					   select new { mdm.MappedName };

			var joinSB = new StringBuilder();
			var subSelectSB = new StringBuilder();
			
			foreach ( var key in keys )
			{
				joinSB.AppendFormat( "j0.[{0}] = j1.[{0}] AND ", key.MappedName );
				// For now, always assuming table is aliased as t0.  Should probably improve at some point.
				// Just writing a smaller sub-select so it doesn't get all the columns of data, but instead
				// only the primary key fields used for joining.
				subSelectSB.AppendFormat( "[t0].[{0}], ", key.MappedName );
			}

			var selectCommand = table.Context.GetCommand( entities );
			var select = selectCommand.CommandText;

			var join = joinSB.ToString();

			if ( join == "" )
			{
				throw new MissingPrimaryKeyException( string.Format( "{0} does not have a primary key defined.  Batch updating/deleting can not be used for tables without a primary key.", metaTable.TableName ) );
			}

            #region - Email Regarding code below
            /*
                So…to be honest an old employee helped me write some of this library.  Specifically the code around Expression tree manipulation and visiting.  I’ve reached out to him but testing…

                a) When you have the CompiledQuery.Compile() version of code, when I attempt to get the underlying ‘sql’ query that the variable posts would be created by, I am returned “SELECT NULL AS [EMPTY]” from the following code:

			                var selectCommand = table.Context.GetCommand( entities );
			                var select = selectCommand.CommandText;

                   I’m not sure why LINQ to SQL is returning that ‘empty’ query to represent posts variable.  And if there is something else I can evaluate/visit to find the real query.  Similar problem found here: http://stackoverflow.com/questions/1719264/how-to-extract-the-sql-command-from-a-complied-linq-query I’m still googling.

                b) Workaround #1: How often are you calling this method?  Would assume not very often?  If you remove the CompiledQuery.Compile() from you code it works.  Obviously it’ll have to compile the query each time, but if only running a handful of times, probably not a problem.

                c) Workaround #2: If you change your static Func<> into a ‘real’ static function() { } it works as well.  I’m not versed enough in ‘compiled code’ versus CompiledQuery.Compile() to know the actual differences in how the compiler might optimized the static function() and any performance hits you might hit, but this might be acceptable solution as well.

                I’ll let you know if I find anything.

                On Aug 18, 2015, at 6:30 AM, Tomas Pettersson <Tomas.Pettersson@firefly.se> wrote:

                Hi
                Here is the code that tries to do an update batch:
                           using (DataClasses1DataContext dbContext = new DataClasses1DataContext())
                           {
                               var posts = getXMgdParamRows(dbContext, 100);
                               dbContext.T_AllMgdParams.UpdateBatch(posts, p => new T_AllMgdParam { Copy = 2 });
                           }


                And getXMgdParamRows looks like this:
                       public static Func<DataClasses1DataContext, int, IQueryable<T_AllMgdParam>>
                            getXMgdParamRows = CompiledQuery.Compile((DataClasses1DataContext dcFrom, int getRows) =>
                            (from a in dcFrom.T_AllMgdParams
                             orderby a.DateAndTime
                             where a.Copy != 2
                             select a).Take(getRows));


                Yes, it's the same table.
                /Tomas
            */
            #endregion

            // Had to comment out again b/c if where has enumerable.Select( => ).Contains( ... ) you get a NULL AS [EMPTY] and the BatchJoin was returning entire query
            // which corrupted the join sql
            /*
            else if ( select.IndexOf( "NULL AS [EMPTY]" ) >= 0 )
            {
                return select;  // calling function will throw exception
            }
            */

            join = join.Substring( 0, join.Length - 5 );											// Remove last ' AND '
			#region - Better ExpressionTree Handling Needed -
			/*
			
			Below is a sample query where the let statement was used to simply the 'where clause'.  However, it produced an extra level
			in the query.
			 
			var manage =
				from u in User
				join g in Groups on u.User_Group_id equals g.gKey into groups
				from g in groups.DefaultIfEmpty()
				let correctGroup = groupsToManage.Contains( g.gName ) || ( groupsToManage.Contains( "_GLOBAL" ) && g.gKey == null )
				where correctGroup && ( users.Contains( u.User_Authenticate_id ) || userEmails.Contains( u.User_Email ) ) || userKeys.Contains( u.User_id )
				select u;
			 
			Produces this SQL:
			SELECT [t2].[User_id] AS [uKey], [t2].[User_Authenticate_id] AS [uAuthID], [t2].[User_Email] AS [uEmail], [t2].[User_Pin] AS [uPin], [t2].[User_Active] AS [uActive], [t2].[uAdminAuthID], [t2].[uFailureCount]
			FROM (
				SELECT [t0].[User_id], [t0].[User_Authenticate_id], [t0].[User_Email], [t0].[User_Pin], [t0].[User_Active], [t0].[uFailureCount], [t0].[uAdminAuthID], 
					(CASE 
						WHEN [t1].[gName] IN (@p0) THEN 1
						WHEN NOT ([t1].[gName] IN (@p0)) THEN 0
						ELSE NULL
					 END) AS [value]
				FROM [User] AS [t0]
				LEFT OUTER JOIN [Groups] AS [t1] ON [t0].[User_Group_id] = ([t1].[gKey])
				) AS [t2]
			WHERE (([t2].[value] = 1) AND (([t2].[User_Authenticate_id] IN (@p1)) OR ([t2].[User_Email] IN (@p2)))) OR ([t2].[User_id] IN (@p3))			 
			
			If I put the entire where in one line...
			where 	( groupsToManage.Contains( g.gName ) || ( groupsToManage.Contains( "_GLOBAL" ) && g.gKey == null ) ) && 
					( users.Contains( u.User_Authenticate_id ) || userEmails.Contains( u.User_Email ) ) || userKeys.Contains ( u.User_id )

			I get this SQL:
			SELECT [t0].[User_id] AS [uKey], [t0].[User_Authenticate_id] AS [uAuthID], [t0].[User_Email] AS [uEmail], [t0].[User_Pin] AS [uPin], [t0].[User_Active] AS [uActive], [t0].[uAdminAuthID], [t0].[uFailureCount]
			FROM [User] AS [t0]
			LEFT OUTER JOIN [Groups] AS [t1] ON [t0].[User_Group_id] = ([t1].[gKey])
			WHERE (([t1].[gName] IN (@p0)) AND (([t0].[User_Authenticate_id] IN (@p1)) OR ([t0].[User_Email] IN (@p2)))) OR ([t0].[User_id] IN (@p3))			
			
			The second 'cleaner' SQL worked with my original 'string parsing' of simply looking for [t0] and stripping everything before it
			to get rid of the SELECT and any 'TOP' clause if present.  But the first SQL introduced a layer which caused [t2] to be used.  So
			I have to do a bit different string parsing.  There is probably a more efficient way to examine the ExpressionTree and figure out
			if something like this is going to happen.  I will explore it later.
			*/
			#endregion

			var endSelect = select.IndexOf( "[t" );													// Get 'SELECT ' and any TOP clause if present
			var selectClause = select.Substring( 0, endSelect );
			var selectTableNameStart = endSelect + 1;												// Get the table name LINQ to SQL used in query generation
			var selectTableName = select.Substring( selectTableNameStart,							// because I have to replace [t0] with it in the subSelectSB
										select.IndexOf( "]", selectTableNameStart ) - ( selectTableNameStart ) );
			
			// TODO (Chris): I think instead of searching for ORDER BY in the entire select statement, I should examine the ExpressionTree and see
			// if the *outer* select (in case there are nested subselects) has an orderby clause applied to it.
			var needsTopClause = selectClause.IndexOf( " TOP " ) < 0 && select.IndexOf( "\r\nORDER BY " ) > 0;
			
			var subSelect = selectClause
								+ ( needsTopClause ? "TOP 100 PERCENT " : "" )							// If order by in original select without TOP clause, need TOP
								+ subSelectSB.ToString()												// Append just the primary keys.
											 .Replace( "[t0]", string.Format( "[{0}]", selectTableName ) );												
			subSelect = subSelect.Substring( 0, subSelect.Length - 2 );									// Remove last ', '

			subSelect += select.Substring( select.IndexOf( "\r\nFROM " ) ); // Create a sub SELECT that *only* includes the primary key fields

			var batchJoin = String.Format( "FROM {0} AS j0 INNER JOIN (\r\n\r\n{1}\r\n\r\n) AS j1 ON ({2})\r\n", table.GetDbName(), subSelect, join );
			return batchJoin;
		}

		private static string GetDbName<TEntity>( this Table<TEntity> table ) where TEntity : class
		{
			var entityType = typeof( TEntity );
			var metaTable = table.Context.Mapping.GetTable( entityType );
			var tableName = metaTable.TableName;

			string[] parts = tableName.Split( '.' );
			tableName = string.Join( ".", parts.Select( p => p.StartsWith( "[" ) ? p : string.Format( "[{0}]", p ) ) );

			return tableName;
		}

		private static string GetLog( this DataContext context )
		{
            var providerProperty = context.GetType().GetProperty("Provider", BindingFlags.Instance | BindingFlags.NonPublic );
            var provider = providerProperty.GetValue( context, null );
            var providerType = provider.GetType();

            var modeProperty = providerType.GetProperty("Mode", BindingFlags.Instance | BindingFlags.NonPublic );
            var servicesField = providerType.GetField("services", BindingFlags.Instance | BindingFlags.NonPublic );
            var services = servicesField != null ? servicesField.GetValue( provider ) : null;
            var modelProperty = services != null ? services.GetType().GetProperty("Model", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty ) : null;
            var taType = providerType.Assembly.GetType("ThisAssembly", false );
            var taVersion = taType != null ? taType.GetField("InformationalVersion", BindingFlags.Static | BindingFlags.NonPublic ) : null;

            return string.Format("-- Context: {0} ({1}) Model: {2} Build: {3}\r\n",
                providerType.Name,
                modeProperty != null ? modeProperty.GetValue( provider, null ) : "unknown",
                modelProperty != null ? modelProperty.GetValue( services, null ).GetType().Name : "unknown",
                taVersion != null ? taVersion.GetValue( null ) : "3.5.21022.8");
        }

		/// <summary>
		/// Returns a list of changed items inside the Context before being applied to the data store.
		/// </summary>
		/// <param name="context">The DataContext to interogate for pending changes.</param>
		/// <returns>Returns a list of changed items inside the Context before being applied to the data store.</returns>
		/// <remarks>Based on Ryan Haney's code at http://dotnetslackers.com/articles/csharp/GettingChangedEntitiesFromLINQToSQLDataContext.aspx.  Note that this code relies on reflection of private fields and members.</remarks>
		public static List<ChangedItems<TEntity>> GetChangedItems<TEntity>( this DataContext context )
		{
			// create a dictionary of type TItem for return to caller
			List<ChangedItems<TEntity>> changedItems = new List<ChangedItems<TEntity>>();

			PropertyInfo providerProperty = context.GetType().GetProperty( "Provider", BindingFlags.Instance | BindingFlags.NonPublic );
			var provider = providerProperty.GetValue( context, null );
			Type providerType = provider.GetType();

			// use reflection to get changed items from data context
			object services = providerType.GetField( "services",
			  BindingFlags.NonPublic |
			  BindingFlags.Instance |
			  BindingFlags.GetField ).GetValue( provider );

			object tracker = services.GetType().GetField( "tracker",
			  BindingFlags.NonPublic |
			  BindingFlags.Instance |
			  BindingFlags.GetField ).GetValue( services );

			System.Collections.IDictionary trackerItems =
			  (System.Collections.IDictionary)tracker.GetType().GetField( "items",
			  BindingFlags.NonPublic |
			  BindingFlags.Instance |
			  BindingFlags.GetField ).GetValue( tracker );

			// iterate through each item in context, adding
			// only those that are of type TItem to the changedItems dictionary
			foreach ( System.Collections.DictionaryEntry entry in trackerItems )
			{
				object original = entry.Value.GetType().GetField( "original",
								  BindingFlags.NonPublic |
								  BindingFlags.Instance |
								  BindingFlags.GetField ).GetValue( entry.Value );

				if ( entry.Key is TEntity && original is TEntity )
				{
					changedItems.Add(
					  new ChangedItems<TEntity>( (TEntity)entry.Key, (TEntity)original )
					);
				}
			}
			return changedItems;
		}
	}

	public class ChangedItems<TEntity>
	{
		public ChangedItems( TEntity Current, TEntity Original )
		{
			this.Current = Current;
			this.Original = Original;
		}
		public TEntity Current { get; set; }
		public TEntity Original { get; set; }
	}
}
