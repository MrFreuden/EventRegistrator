using EventRegistrator.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventRegistrator
{
    public class EnvLoader
    {
        public static void Load()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            var envFile = environment == "Production" ? ".env.production" : ".env";
            DotNetEnv.Env.Load(envFile);
            Console.WriteLine($"[ENV] Loaded: {envFile}");
        }

        public static string GetApiToken()
        {
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
            if (string.IsNullOrEmpty(apiToken)) throw new Exception("API_TOKEN is missing in environment.");
            return apiToken;
        }

        public static string GetDataPath()
        {
            return Environment.GetEnvironmentVariable("DATA_PATH") ?? "data.json";
        }

        public static string GetListenerPrefix()
        {
            return Environment.GetEnvironmentVariable("LISTENER") ?? "http://localhost:8080/";
        }

        public static void LoadDefaultUser1(UserRepository userRepository)
        {
            var userId = long.Parse(Environment.GetEnvironmentVariable("USER1_ID"));
            var user = new UserAdmin(userId)
            { 
                PrivateChatId = userId,
                ChannelId = long.Parse(Environment.GetEnvironmentVariable("USER1_CHANNELID")),
                ChannelName = Environment.GetEnvironmentVariable("USER1_CHANNELNAME"),
                TargetChatId = long.Parse(Environment.GetEnvironmentVariable("USER1_TARGETCHATID")),
                HashtagName = Environment.GetEnvironmentVariable("USER1_HASHTAGNAME"),
            };
            userRepository.AddUser(user);
        }

        public static void LoadDefaultUser2(UserRepository userRepository)
        {
            var userId = long.Parse(Environment.GetEnvironmentVariable("USER2_ID"));
            var user = new UserAdmin(userId)
            {
                PrivateChatId = userId,
                ChannelId = long.Parse(Environment.GetEnvironmentVariable("USER2_CHANNELID")),
                ChannelName = Environment.GetEnvironmentVariable("USER2_CHANNELNAME"),
                TargetChatId = long.Parse(Environment.GetEnvironmentVariable("USER2_TARGETCHATID")),
                HashtagName = Environment.GetEnvironmentVariable("USER2_HASHTAGNAME"),
            };
            userRepository.AddUser(user);
        }

        public static void LoadDefaultUser3(UserRepository userRepository)
        {
            var userId = long.Parse(Environment.GetEnvironmentVariable("USER3_ID"));
            var user = new UserAdmin(userId)
            {
                PrivateChatId = userId,
                ChannelId = long.Parse(Environment.GetEnvironmentVariable("USER3_CHANNELID")),
                ChannelName = Environment.GetEnvironmentVariable("USER3_CHANNELNAME"),
                TargetChatId = long.Parse(Environment.GetEnvironmentVariable("USER3_TARGETCHATID")),
                HashtagName = Environment.GetEnvironmentVariable("USER3_HASHTAGNAME"),
            };
            userRepository.AddUser(user);
        }
    }
}
