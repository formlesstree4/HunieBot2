# This project has been deprecated!
Hey guys. The reason HunieBot2 was started was to make commands easier to implement using [Discord.NET](https://github.com/RogueException/Discord.Net). Well, that is no longer necessary. [Discord.NET](https://github.com/RogueException/Discord.Net) now uses a roughly similar command system that HunieBot2 implemented. I will leave this repository up for the time being, but, it is no longer being actively maintained and hasn't for some time.



# HunieBot
HunieBot is a small Discord Bot Framework built on top of [Discord.NET](https://github.com/RogueException/Discord.Net) that aims to make the initial startup cost of creating a Bot for Discord simple and easy by using Attributes to decorate classes, methods, and functions.

## Getting started with HunieBot
HunieBot requires [Discord.NET](https://github.com/RogueException/Discord.Net) and optionally likes Discord.Net.Audio (if you prefer to use the built-in MusicStream bot). You also must understand .NET Attributes.

Now then, let's get started with creating your first bot for HunieBot.

Here's a simple class to get you started:
```csharp
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;

namespace HunieBot.HelloWorld
{
    [HunieBot(nameof(HelloWorldBot))]
    public sealed class HelloWorldBot
    {
        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, commands: new[] { "ping" })]
        public async Task HandleCommand(IHunieCommand command)
        {
            await command.Channel.SendMessage("pong");
        }
    }
}
```
There's a lot going on in just 17 lines of code. Let's take a moment to break down what's going on, focusing in particular on two lines of code:
```csharp
[HunieBot(nameof(HelloWorldBot))]
```
The [HunieBotAttribute](https://github.com/formlesstree4/HunieBot2/blob/master/HunieBot.Host/Attributes/HunieBotAttribute.cs) is an indicator for HunieBot that the class it is attached to is a Bot that must be instantiated and loaded at runtime. It has one required parameter and that is the name of your Bot.

The second Attribute has quite a bit more going on:
```csharp
[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, commands: new[] { "ping" })]
```
The [HandleCommandAttribute](https://github.com/formlesstree4/HunieBot2/blob/master/HunieBot.Host/Attributes/HandleCommandAttribute.cs) is a bit more of an advanced Attribute that describes to HunieBot how your method call is to be invoked when a command is received. For the sake of brevity, please refer to the linked file for documentation and comments.

## Running your HunieBot
By default, the HunieBot.Host project will scan whatever path is in Environment.CurrentDirectory to load assemblies that might contain HunieBotAttribute classes. That means building HunieBot instances is as simple as compiling a DLL and dropping it into the same folder that HunieBot.Host resides in.

Warning: This documentation is sparse and incomplete. I am working on documenting everything but it will take some time.
