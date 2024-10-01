using Production.WorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();

builder.Services.AddSingleton<IConnectionMultiplexer>( 
    cm => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!)
    );

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
