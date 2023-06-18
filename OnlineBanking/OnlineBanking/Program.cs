using Newtonsoft.Json.Converters;
using OnlineBanking.Domain;
using OnlineBanking.Domain.Repositories;
using OnlineBanking.Persistance;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AerospikeOptions>(builder.Configuration.GetSection("Aerospike"));
builder.Services.AddScoped<IFinancialTransactionRepository, AerospikeFinancialTransactionRepository>();
builder.Services.AddScoped<IUserRepository, AerospikeUserRepository>();
builder.Services.AddSingleton<AerospikeSetup>();

var app = builder.Build();

var aerospikeSetup = app.Services.GetRequiredService<AerospikeSetup>();
aerospikeSetup.Setup();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

