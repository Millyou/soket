using System.Text;
using Microsoft.Extensions.Configuration;
using MSD.Crux.Core.Models;

namespace MSD.Client.TCP;

/// <summary>
/// 소켓통신으로 전송할 데이터(메시지)를 만든다
/// </summary>
public static class MessageBuilder
{
    /// <summary>
    /// Jwt 인증용 소켓통신 프로토콜로 전송할 바이너리 데이터를 만든다.
    /// </summary>
    /// <param name="frameType">프레임 타입 번호 </param>
    /// <param name="user">토큰을 생성할 유저 객체</param>
    /// <param name="configuration">구성파일 객체</param>
    /// <returns>소켓통신 바이너리 frame</returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] CreateJwtTypeFrame(byte frameType, Users users, IConfiguration configuration)
    {
        CruxClaim cruxClaim = new()
        {
            LoginId = users.LoginId ?? string.Empty,
            EmployeeName = users.Name,
            EmployeeNumber = users.EmployeeNumber,
            Roles = users.Roles ?? string.Empty,
        };

        string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6InN0cmluZyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiLsi5zsiqTthZzqtIDrpqzsnpAiLCJFbXBsb3llZU51bWJlciI6IjIwMDAxMDAwMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6InN5c3RlbUFkbWluIiwiZXhwIjoxNzM2OTI3NjIxLCJpc3MiOiJNU0QgQ3J1eCIsImF1ZCI6Ik1TRCBDbGllbnQifQ.NqQklhu2zeYoqD_3-hLRnq0KVvX52X_aYRgkZe4FNBLJz1Gq3iOSf2BX-yrbNSN93Cigu1DAPrRurnbfrRSJVsRAyeZB1_mR6w1rs453uv9TTT4qH_i50jimMT3kGcF--e_5TxmjzUsuPZbVKXdZEUbC_4LmF7-x9Cs7a9KbKewY8H3wxmqMG3mxEaIPs59yOkDPPfC7WNhsAits_r8DEDk8MhYZDJnKTkzu2gVK5-BLXLSCt4JjbLc8inz3aJJbfl3ibWe0JGoDYAZhfAYIoTibSPnxvHY2-Cc0GGalhu-jU4fgCelxmhWoXS4YbSk7b7e13-Bl-z2b4eOTwytG3Q";

        byte[] payload = Encoding.UTF8.GetBytes(token);

        if (payload.Length < 200 || payload.Length > 1024)
        {
            throw new ArgumentException($"JWT 길이는 200~1024 bytes 사이어여합니다. 현재 길이는: {payload.Length}");
        }

        int totalLength = 6 + payload.Length;
        byte[] message = new byte[totalLength];

        message[0] = frameType;
        // 페이로드 길이 2Byte: 토큰길이는 700글자 이상이고 1 byte가 표현 가능한 숫자는 0~255이 한계이므로 길이에대한 숫자는 2Byte(ushort) 표현해야한다.
        BitConverter.GetBytes((ushort)payload.Length).CopyTo(message, 1);
        message[3] = 1;
        message[4] = 1;
        message[5] = 0;

        Array.Copy(payload, 0, message, 6, payload.Length);

        return message;
    }

    /// <summary>
    /// Injection 생산 누적생산량 소켓통신 프로토콜로 전송할 바이너리 데이터를 만든다
    /// </summary>
    /// <param name="frameType">프레임 타입 번호</param>
    /// <param name="injection">비전검사 누적 검사량 객체</param>
    /// <returns>소켓통신 바이너리 frame</returns>
    public static byte[] CreateInjectionTypeFrame(byte frameType, InjectionCum injectionCum)
    {
        int payloadSize = 50;
        int totalSize = 6 + payloadSize;
        byte[] message = new byte[totalSize];

        message[0] = frameType;
        BitConverter.GetBytes((ushort)payloadSize).CopyTo(message, 1);
        message[3] = 1;
        message[4] = 0;
        message[5] = 0;

        Encoding.ASCII.GetBytes(injectionCum.LineId.PadRight(4, '\0')).CopyTo(message, 6);
        BitConverter.GetBytes(new DateTimeOffset(injectionCum.Time).ToUnixTimeSeconds()).CopyTo(message, 10);
        Encoding.ASCII.GetBytes(injectionCum.LotId?.PadRight(20, '\0') ?? new string('\0', 20)).CopyTo(message, 18);
        Encoding.ASCII.GetBytes(injectionCum.Shift?.PadRight(4, ' ') ?? "    ").CopyTo(message, 38);
        BitConverter.GetBytes((injectionCum.EmployeeNumber ?? 0)).CopyTo(message, 42);
        BitConverter.GetBytes(injectionCum.Total).CopyTo(message, 50);

        return message;
    }

}
