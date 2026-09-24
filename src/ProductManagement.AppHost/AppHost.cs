var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var db = sql.AddDatabase("productsdb");

var migrations = builder.AddProject<Projects.ProductManagement_MigrationService>("migrations")
	.WithReference(db)
	.WaitFor(db);

builder.AddProject<Projects.ProductManagement_Api>("api")
	.WithReference(db)
	.WaitForCompletion(migrations)
	.WithReplicas(2);

builder.Build().Run();