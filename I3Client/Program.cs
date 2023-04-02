﻿using System;
using System.IO.Pipes;
using System.IO;
using H.Pipes;
using H.Formatters;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await using var client = new PipeClient<string>(
            "I3Server.cs",
            formatter: new SystemTextJsonFormatter()
        );
        client.MessageReceived += (o, args) =>
            Console.WriteLine("MessageReceived: " + args.Message);
        client.Disconnected += (o, args) => Console.WriteLine("Disconnected from server");
        client.Connected += (o, args) => Console.WriteLine("Connected to server");
        client.ExceptionOccurred += (o, args) => Console.WriteLine("ExceptionOccurred");
        await client.ConnectAsync();
        await client.WriteAsync(args[0]);
        await client.DisconnectAsync();
        await client.DisposeAsync();
    }
}
