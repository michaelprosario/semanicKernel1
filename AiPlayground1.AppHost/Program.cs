var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AiPlayground1>("aiplayground1");

builder.Build().Run();
