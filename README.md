# Changes

The iOS and Android versions now uses SQLitePCL.raw as a nuget dependency, so your app will use the latest version of SQLite instead of the buggy one provided by the OS !

See https://github.com/ericsink/SQLitePCL.raw/ for more information on how to use SQLitePCL.raw

Since SQLitePCL.raw 0.9 you have to add a new nuget package in your platform project: 

    SQLitePCL.plugin.sqlite3.ios_unified for ios

and add an init code:

                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_internal());

For information:  
iOS 9.2 is shipped with sqlite 3.8.10.2  
iOS 8.4 is shipped with sqlite 3.8.5  

# New Features compared to oysteinkrog

 Multiple primary key support		
 Ex: 		
 		
     public class PrivacyGroupItem		
     {		
 		[PrimaryKey]		
 		public int GroupId {get;set;}		
 		
 		[PrimaryKey]		
 		public int ContactId {get;set;}		
     }		
 		
     db.Delete<PrivacyGroupItem>(groupId, contactId);		
 		
 		
 Projections now have the expected result type		
 Ex: `IEnumerable<int> ids = from pgi in db.Table<PrivacyGroupItem>() where pgi.PrivacyGroupId == groupId select pgi.ContactId;`		
 		
 New method to query simple types (ie: string, ...) as Query<T> can query only complex types (ie: T must be a class/stuct with a default constructor)		
 Signature: `IEnumerable<T> ExecuteSimpleQuery<T>(string query, params object[] args)`		
 Usage: `ExecuteSimpleQuery<string>("select 'drop table ' || name || ';' from sqlite_master where type = 'table'")`		

# Fork

This is a fork of oysteinkrog fork which is a fork of the original sqlite-net library (https://github.com/praeclarum/sqlite-net), 
which aims to improve the code quality by using modern technologies such as PCL (portable class library).

This is a fork of the original sqlite-net library (https://github.com/praeclarum/sqlite-net), which aims to improve the code quality by using modern technologies such as PCL (portable class library).

The project will avoid the use of #if-based conditional code and use platform-specific code injection instead.

I welcome pull requests, but keep in mind that this library is in heavy use and all changes must be:
- Backwards-compatible (don't change database defaults).
- Well tested (please add unit tests).

# Versioning

This project uses semantic versioning.

# API Changes

As part of the cleanup there are now some API changes.
For the most part I hope these are self-explanatory, but here is a non-exhaustive list of changes.

## SQLiteConnection
You now have to pass in an implementation of ISQlitePlatform in the SQLiteConnectionWithLock and SQLiteConnection constructors.
The correct platform implementation is automatically added to the project. 

At the moment these platforms are supported:
- Win32 (bundles sqlite binaries for windows, works on both x86 and x64 automatically) (very well tested)
- XamarinIOS and XamarinIOS.Unified
- XamarinAndroid
- WindowsPhone8 (contributed by Nick Cipollina, thanks!)
- WinRT (Windows 8 and Windows Phone 8.1+) (contributed by Nick Cipollina and Micah Lewis, thanks!)
- Generic (net4 project without any sqlite3 binaries, requires sqlite installed in the OS) (contributed by James Ottaway)

Note: 
To use the WP8.1/WinRT platform you must install the "SQLite for Windows Phone"/"SQLite for Windows" VSIX extension. 
Then, in the project, add a reference to the extension (in the Extensions section of the Add Reference dialog)
If you have problems with signed apps take a look here: https://github.com/oysteinkrog/SQLite.Net-PCL/issues/25

## SQliteAsyncConnection
The SQLiteAsyncConnection class now takes a Func<SQLiteConnectionWithLock> in the constructor instead of a path.
This is done because the async classes are now just meant to be wrappers around the normal sqlite connection.

To use SQLiteAsyncConnection just create an instance of a SQLiteConnectionWithLock and pass in that through a func, e.g.:
new SQLiteAsyncConnection(()=>_sqliteConnectionWithLock);

Please be aware that the Task.Run pattern used in SQLiteAsyncConnection can be considered an anti-pattern (libraries should not provide async methods unless they are truly async).
This class is maintained for backwards compatability and for use-cases where async-isolation is handy.

## DateTime serialization

DateTime serialization is changed, in order to be consistent.
When using the storeDateTimeAsTicks option, the DateTime is now serialized as Utc, and the returned DateTime is also in Utc.
You can get the local time by using dateTime.ToLocalTime()

# SQLite.Net

SQLite.Net is an open source, minimal library to allow .NET and Mono applications to store data in [http://www.sqlite.org SQLite 3 databases]. It is written in C# and is meant to be simply compiled in with your projects. It was first designed to work with [MonoTouch](http://xamarin.com) on the iPhone, but has grown up to work on all the platforms (Mono for Android, .NET, Silverlight, WP7, WinRT, Azure, etc.).

SQLite.Net was designed as a quick and convenient database layer. Its design follows from these *goals*:

* Very easy to integrate with existing projects and with MonoTouch projects.
  
* Thin wrapper over SQLite and should be fast and efficient. (The library should not be the performance bottleneck of your queries.)
  
* Very simple methods for executing CRUD operations and queries safely (using parameters) and for retrieving the results of those query in a strongly typed fashion.
  
* Works with your data model without forcing you to change your classes. (Contains a small reflection-driven ORM layer.)
  
* 0 dependencies aside from a [compiled form of the sqlite2 library](http://www.sqlite.org/download.html).

*Non-goals* include:

* Not an ADO.NET implementation. This is not a full SQLite driver. If you need that, use [Mono.Data.SQLite](http://www.mono-project.com/SQLite) or [csharp-sqlite](http://code.google.com/p/csharp-sqlite/).

## License
This projected is licensed under the terms of the MIT license.
See LICENSE.TXT

## Meta

This is an open source project that welcomes contributions/suggestions/bug reports from those who use it. If you have any ideas on how to improve the library, please [post an issue here on github](https://github.com/praeclarum/SQLite.Net/issues). Please check out the [How to Contribute](https://github.com/praeclarum/SQLite.Net/wiki/How-to-Contribute).


# Example Time!

Please consult the source code (see unit tests) for more examples.

The library contains simple attributes that you can use to control the construction of tables. In a simple stock program, you might use:

    public class Stock
    {
    	[PrimaryKey, AutoIncrement]
    	public int Id { get; set; }
    	[MaxLength(8)]
    	public string Symbol { get; set; }
    }

    public class Valuation
    {
    	[PrimaryKey, AutoIncrement]
    	public int Id { get; set; }
    	[Indexed]
    	public int StockId { get; set; }
    	public DateTime Time { get; set; }
    	public decimal Price { get; set; }
    }

Once you've defined the objects in your model you have a choice of APIs. You can use the "synchronous API" where calls
block one at a time, or you can use the "asynchronous API" where calls do not block. You may care to use the asynchronous
API for mobile applications in order to increase reponsiveness.

Both APIs are explained in the two sections below.

## Synchronous API

Once you have defined your entity, you can automatically generate tables in your database by calling `CreateTable`:

    var db = new SQLiteConnection(sqlitePlatform, "foofoo");
    db.CreateTable<Stock>();
    db.CreateTable<Valuation>();

You can insert rows in the database using `Insert`. If the table contains an auto-incremented primary key, then the value for that key will be available to you after the insert:

    public static void AddStock(SQLiteConnection db, string symbol) {
    	var s = db.Insert(new Stock() {
    		Symbol = symbol
    	});
    	Console.WriteLine("{0} == {1}", s.Symbol, s.Id);
    }

Similar methods exist for `Update` and `Delete`.

The most straightforward way to query for data is using the `Table` method. This can take predicates for constraining via WHERE clauses and/or adding ORDER BY clauses:

		var conn = new SQLiteConnection(sqlitePlatform, "foofoo");
		var query = conn.Table<Stock>().Where(v => v.Symbol.StartsWith("A"));

		foreach (var stock in query)
			Debug.WriteLine("Stock: " + stock.Symbol);

You can also query the database at a low-level using the `Query` method:

    public static IEnumerable<Valuation> QueryValuations (SQLiteConnection db, Stock stock)
    {
    	return db.Query<Valuation> ("select * from Valuation where StockId = ?", stock.Id);
    }

The generic parameter to the `Query` method specifies the type of object to create for each row. It can be one of your table classes, or any other class whose public properties match the column returned by the query. For instance, we could rewrite the above query as:

    public class Val {
    	public decimal Money { get; set; }
    	public DateTime Date { get; set; }
    }
    public static IEnumerable<Val> QueryVals (SQLiteConnection db, Stock stock)
    {
    	return db.Query<Val> ("select 'Price' as 'Money', 'Time' as 'Date' from Valuation where StockId = ?", stock.Id);
    }

You can perform low-level updates of the database using the `Execute` method.

## Asynchronous API

The asynchronous library uses the Task Parallel Library (TPL). As such, normal use of `Task` objects, and the `async` and `await` keywords 
will work for you.

Once you have defined your entity, you can automatically generate tables by calling `CreateTableAsync`:

	var conn = new SQLiteAsyncConnection(()=>sqliteConnection, "foofoo");
	conn.CreateTableAsync<Stock>().ContinueWith((results) =>
	{
		Debug.WriteLine("Table created!");
	});

You can insert rows in the database using `Insert`. If the table contains an auto-incremented primary key, then the value for that key will be available to you after the insert:

		Stock stock = new Stock()
		{
			Symbol = "AAPL"
		};

		var conn = new SQLiteAsyncConnection(()=>sqliteConnection, "foofoo");
		conn.InsertAsync(stock).ContinueWith((t) =>
		{
			Debug.WriteLine("New customer ID: {0}", stock.Id);
		});

Similar methods exist for `UpdateAsync` and `DeleteAsync`.

Querying for data is most straightforwardly done using the `Table` method. This will return an `AsyncTableQuery` instance back, whereupon
you can add predictates for constraining via WHERE clauses and/or adding ORDER BY. The database is not physically touched until one of the special 
retrieval methods - `ToListAsync`, `FirstAsync`, or `FirstOrDefaultAsync` - is called.

		var conn = new SQLiteAsyncConnection(()=>sqliteConnection, "foofoo");
		var query = conn.Table<Stock>().Where(v => v.Symbol.StartsWith("A"));
			
		query.ToListAsync().ContinueWith((t) =>
		{
			foreach (var stock in t.Result)
				Debug.WriteLine("Stock: " + stock.Symbol);
		});

There are a number of low-level methods available. You can also query the database directly via the `QueryAsync` method. Over and above the change 
operations provided by `InsertAsync` etc you can issue `ExecuteAsync` methods to change sets of data directly within the database.

Another helpful method is `ExecuteScalarAsync`. This allows you to return a scalar value from the database easily:

		var conn = new SQLiteAsyncConnection("foofoo");
		conn.ExecuteScalarAsync<int>("select count(*) from Stock", null).ContinueWith((t) =>
		{
			Debug.WriteLine(string.Format("Found '{0}' stock items.", t.Result));
		});


