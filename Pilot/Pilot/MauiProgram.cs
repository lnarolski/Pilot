

using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.Embedding;

namespace Pilot;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCompatibility()
            .UseMauiEmbedding<Application>();

        return builder.Build();
    }
}
