using Grpc;
using Library.Storage.Logic;
using Messenger.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

var services = builder.Services;

services.AddGrpc();
services.AddSqlServerStorage(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<MessengerService>();
app.MapGrpcService<UserService>();

app.Run();