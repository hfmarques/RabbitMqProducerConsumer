var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

// var webapi = builder.AddProject<Projects.WebApi>("webapi")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
//     .WithReference(messaging)
//     .WithExternalHttpEndpoints();

builder.Build().Run();