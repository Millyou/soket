using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MSD.Client.TCP;
using MSD.Crux.Core.Models;

/// <summary>
/// 소켓통신 커스텀 프로토콜 테스트용 클라이언트
/// </summary>
internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Socket Client Test Application");

        string serverAddress = "13.125.114.64";
        int port = 51900;

       

        InjectionCum injectionCum = new InjectionCum
        {
            LineId = "ip02",
            Time = DateTime.UtcNow,
            LotId = "AAAA120250107-1",
            Shift = "A",
            EmployeeNumber = 202340014,
            Total = 1000
        };

        Users users = new Users
        {
            Id = 30,
            EmployeeNumber = 202340014,
            LoginId = "nati",
            LoginPw = "ix83VPAon+7AjG0JPkavXDaHtyYCeObFml4iI6WQK2w",
            Salt = "eY4uyzWqHi8BeVyl6BbV2w",
            Name = "나띠 욘따라락",
            Roles = "vision"
        };
        TcpClientHandler tcpClientHandler = new TcpClientHandler(serverAddress, port);

        await tcpClientHandler.StartAsync(input =>
        {
            return input switch
            {
                //"1" => MessageBuilder.CreateJwtTypeFrame(1, users, configuration),
                "2" => MessageBuilder.CreateInjectionTypeFrame(1, injectionCum),
                _ => throw new ArgumentException("Invalid FrameType")
            };
        },CancellationToken.None);
    }
}
