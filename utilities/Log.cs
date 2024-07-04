using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

namespace Preach.CS2.Plugins.RollTheDiceV2.Utilities;

public static class Log 
{
    private static string Prefix => GetLocalizedText("prefix");

    private static string GetLogTypePrefix(LogType? type)
    {
        return type switch 
        {
            LogType.DEFAULT => "",
            LogType.SUCCSS => $"{ChatColors.Green}[SUCCESS]{default}",
            LogType.INFO => $"{ChatColors.Blue}[INFO]{default}",
            LogType.WARNING => $"{ChatColors.Yellow}[WARNING]{default}",
            LogType.ERROR => $"{ChatColors.Red}[ERROR]{default}",
            LogType.DEBUG => $"{ChatColors.Magenta}[DEBUG]{default}",
            _ => ""
        };
    }

    private static string GetLogTypePrefixHtml(LogType? type)
    {
        return type switch 
        {
            LogType.DEFAULT => "",
            LogType.SUCCSS => "<font color=\"green\">SUCCESS</font>",
            LogType.INFO => "<font color=\"blue\">INFO</font>",
            LogType.WARNING => "<font color=\"yellow\">WARNING</font>",
            LogType.ERROR => "<font color=\"red\">ERROR</font>",
            LogType.DEBUG => "<font color=\"magenta\">DEBUG</font>",
            _ => ""
        };
    }

    // Clean all colors in message, taken from:
    // https://discord.com/channels/1160907911501991946/1175947333880524962/1236581641036759044
    private static readonly Dictionary<string, char> PredefinedColors = typeof(ChatColors)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .ToDictionary(field => $"{{{field.Name}}}", field => (char)(field.GetValue(null) ?? '\x01'));
    private static string RemoveColorCodes(string input)
    {
        return PredefinedColors.Aggregate(input, (current, color) => current.Replace(color.Key, "").Replace(color.Value.ToString(), ""));
    }

    private static string GetFormatedText(string message, LogType? type = LogType.DEFAULT, bool replaceColorsEmpty = false)
    {
        var formatedLog = Prefix + GetLogTypePrefix(type) + " " + message;

        return replaceColorsEmpty ? RemoveColorCodes(formatedLog) : formatedLog;
    }
    private static string GetFormatedHtml(string message, LogType? type = LogType.DEFAULT)
    {
        return GetLogTypePrefixHtml(type) + " " + message;
    }

    public static void PrintCenter(CCSPlayerController playerController, string message, LogType? type = LogType.DEFAULT)
    {
        playerController.PrintToCenter(GetFormatedText(message, type, true));
    }
    public static void PrintCenterHtml(CCSPlayerController playerController, string message, LogType? type = LogType.DEFAULT)
    {
        playerController.PrintToCenterHtml(GetFormatedHtml(message, type));
    }

    public static void PrintChat(CCSPlayerController playerController, string message, LogType? type = LogType.DEFAULT)
    {
        playerController.PrintToChat(GetFormatedText(message, type));
    }

    public static void PrintChatAll(string message, LogType? type = LogType.DEFAULT, bool printServerConsole = false)
    {
        Server.PrintToChatAll(GetFormatedText(message, type));

        if(printServerConsole)
            PrintServerConsole(message, LogType.INFO);
    }

    public static void PrintServerConsole(string message, LogType? type = LogType.INFO)
    {
        if(!RollTheDice.DEBUG && type == LogType.DEBUG)
            return;

        switch(type)
        {
            case LogType.INFO:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case LogType.ERROR:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogType.WARNING:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
            case LogType.DEBUG:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                break;
            case LogType.SUCCSS:
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                break;
        }

        Console.WriteLine(GetFormatedText(message, type, true));
        Console.ResetColor();
    }
    public static string GetEffectLocale(string name, string type)
    {
        return $"effect_{type}_{name}";
    }
    public static string GetLocalizedText(string key, params object[] args)
    {
        return RollTheDice.Instance!.Localizer[key, args];
    }
}