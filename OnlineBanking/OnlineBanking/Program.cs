using OnlineBanking.Domain;
using OnlineBanking.Persistance;

var builder = WebApplication.CreateBuilder(args);
//builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AerospikeOptions>(builder.Configuration.GetSection("Aerospike"));
builder.Services.AddScoped<IFinancialTransactionRepository, AerospikeFinancialTransactionRepository>();
builder.Services.AddSingleton<AerospikeSetup>();

var app = builder.Build();

var aerospikeSetup = app.Services.GetRequiredService<AerospikeSetup>();
aerospikeSetup.Setup();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

