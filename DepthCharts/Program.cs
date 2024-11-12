using DepthCharts;
using DepthCharts.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure logging and configuration sources
builder.Logging.AddConsole();

// Configure configuration
var dir = Directory.GetCurrentDirectory();
builder.Configuration
    .SetBasePath(dir)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging();

// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



builder.Services.AddScoped<DepthChartScraperHttpClientHelper>();
builder.Services.AddHttpClient<DepthChartScraperHttpClientHelper>();
builder.Services.Configure<DepthChartsUrlsSettings>(builder.Configuration.GetSection("DepthChartsUrls"));

builder.Services.AddSingleton<DepthChartsUrlsSettings>();

builder.Services.AddScoped<IDepthChartScraper, NflDepthChartScraper>();
builder.Services.AddScoped<DepthChartScraperService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();