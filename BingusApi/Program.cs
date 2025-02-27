using BingusLib.FaqHandling;
using BingusLib.HNSW;
using MessagePack;
using MessagePack.Resolvers;

var builder = WebApplication.CreateBuilder(args);

// Add dependencies
builder.Services.AddSingleton(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    // Load the config
    var faqConfig = FaqConfigUtils.InitializeConfig(loggerFactory.CreateLogger<FaqConfig>());

    // Setup the FAQ handler
    var modelPath = Path.Join(Environment.CurrentDirectory, faqConfig.ModelPath);
    var faqHandler = new FaqHandler(loggerFactory, modelPath);

    // Add all questions
    faqHandler.AddItems(faqConfig.QAEntryEnumerator());

    // Setup HNSW serialization stuff
    StaticCompositeResolver.Instance.Register(MessagePackSerializer.DefaultOptions.Resolver);
    StaticCompositeResolver.Instance.Register(new LazyKeyItemFormatter<int, float[]>(i => faqHandler.GetEntry(i).Vector!.AsArray()));
    MessagePackSerializer.DefaultOptions.WithResolver(StaticCompositeResolver.Instance);

    return faqHandler;
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
