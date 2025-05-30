// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Samples.Cosmos.NoSQL.CosmicWorks.Tool.Extensions;

internal static class AnsiConsoleExtensions
{
    public static void WriteItem<T>(this IAnsiConsole console, T item) => console.MarkupLine($"[green][bold][[SEED]][/]\t{item}[/]");

    public static void WriteCredentials(this IAnsiConsole console, IConnectionConfiguration settings)
    {
        console.Write(new Rule("[yellow]Parsing connection string[/]").LeftJustified().RuleStyle("olive"));

        string? credential = settings.GetCredential();

        if (credential is not null)
        {
            console.Write(
                new Panel(settings.HideCredentials is false ? $"[teal]{credential}[/]" : $"[teal dim]{new String('*', credential.Length)}[/]")
                    .Header("[green]Credential[/]")
                    .BorderColor(Color.White)
                    .RoundedBorder()
                    .Expand()
            );
        }
    }

    public static void WriteConfiguration<T>(this IAnsiConsole console, IItemSettings<T> settings) where T : IItem
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow("[green bold]Database[/]", $"[teal]{settings.DatabaseName}[/]");
        grid.AddRow("[green bold]Container[/]", $"[teal]{settings.ContainerName}[/]");
        if (settings.Quantity is not null)
        {
            grid.AddRow("[green]Count[/]", $"[teal]{settings.Quantity:##,#}[/]");
        }

        console.Write(
            new Panel(grid)
                .Header("[green]Products configuration[/]")
                .BorderColor(Color.White)
                .RoundedBorder()
                .Expand()
        );
    }
}