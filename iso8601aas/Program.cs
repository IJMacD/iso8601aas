var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.MapGet("/{date}", TryParseDate);

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

object TryParseDate (string date) {
    try {
        return ISO8601.Parse(date);
    }
    catch (FormatException) {
        return new { Error = "Cannot parse input" };
    }
    catch (ArgumentOutOfRangeException) {
        return new { Error = "Invalid input" };
    }
    catch (NotImplementedException) {
        return new { Error = "Not implemented" };
    }
}