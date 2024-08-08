var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("user", secret: true);
var password = builder.AddParameter("password", secret: true);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", username, password)
    .WithManagementPlugin();

var webapi = builder.AddProject<Projects.WebApi>("webapi")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints();

builder.Build().Run();