using cultureland.cs;
using System;

/*
string certificatePath = @"C:\certs\http-toolkit.crt";
string proxyAddress = "http://127.0.0.1:8000";
Cultureland cultureland = new(proxyAddress, certificatePath);
*/

Cultureland cultureland = new Cultureland();

Console.Write("keeplogincookie: ");
string keepLoginCookie = Console.ReadLine();
await cultureland.LoginAsync(keepLoginCookie);


var Balance = await cultureland.GetBalanceAsync();
Console.WriteLine($"Total balance: {Balance.totalBalance}");

Console.WriteLine("1: Charge Pin 2: Gift Pin");
string type = Console.ReadLine();
if (type is "1")
{
    Console.Write("Enter Pin (including dash): ");
    string PinNumber = Console.ReadLine();
    var result = await cultureland.ChargeAsync(new Pin(PinNumber));
    Console.WriteLine($"amount: {result.amount}");
    Console.WriteLine($"message: {result.message}");
}
else if (type is "2")
{
    Console.Write("Enter Amount: ");
    string AmountString = Console.ReadLine();
    if (int.TryParse(AmountString, out var Amount))
    {
        var result = await cultureland.GiftAsync(Amount);
        Console.WriteLine($"Pin: {string.Join("-", result.pin.Parts)}");
        Console.WriteLine($"URL: {result.url}");
    }
}