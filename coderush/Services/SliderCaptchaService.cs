using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace coderush.Services
{
    public sealed class SliderCaptchaService : ISliderCaptchaService
    {
        private readonly byte[] _secretKey = RandomNumberGenerator.GetBytes(32);

        public (string Token, int PuzzleX) GenerateChallenge()
        {
            int x = RandomNumberGenerator.GetInt32(60, 230);
            string nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            string json = JsonSerializer.Serialize(new ChallengePayload
            {
                X = x,
                Nonce = nonce,
                Timestamp = timestamp
            });

            return (Sign(json), x);
        }

        public string Validate(string challengeToken, int userX, long solveTimeMs)
        {
            try
            {
                string json = Verify(challengeToken);
                if (json is null) return null;

                var payload = JsonSerializer.Deserialize<ChallengePayload>(json);
                if (payload is null) return null;

                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (now - payload.Timestamp > 120_000) return null;
                if (Math.Abs(payload.X - userX) > 5) return null;
                if (solveTimeMs < 400 || solveTimeMs > 60_000) return null;

                string verifyJson = JsonSerializer.Serialize(new VerifyPayload
                {
                    Nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    SolveTimeMs = solveTimeMs
                });

                return Sign(verifyJson);
            }
            catch
            {
                return null;
            }
        }

        public bool IsVerified(string verificationToken)
        {
            if (string.IsNullOrEmpty(verificationToken)) return false;

            try
            {
                string json = Verify(verificationToken);
                if (json is null) return false;

                var payload = JsonSerializer.Deserialize<VerifyPayload>(json);
                if (payload is null) return false;

                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return now - payload.Timestamp <= 300_000;
            }
            catch
            {
                return false;
            }
        }

        private string Sign(string payload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            string b64 = Convert.ToBase64String(bytes);
            using var hmac = new HMACSHA256(_secretKey);
            string sig = Convert.ToBase64String(hmac.ComputeHash(bytes));
            return $"{b64}.{sig}";
        }

        private string Verify(string token)
        {
            string[] parts = token?.Split('.');
            if (parts is not { Length: 2 }) return null;

            try
            {
                byte[] payloadBytes = Convert.FromBase64String(parts[0]);
                using var hmac = new HMACSHA256(_secretKey);
                string expectedSig = Convert.ToBase64String(hmac.ComputeHash(payloadBytes));

                if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSig),
                    Encoding.UTF8.GetBytes(parts[1])))
                    return null;

                return Encoding.UTF8.GetString(payloadBytes);
            }
            catch
            {
                return null;
            }
        }

        private sealed class ChallengePayload
        {
            public int X { get; set; }
            public string Nonce { get; set; } = "";
            public long Timestamp { get; set; }
        }

        private sealed class VerifyPayload
        {
            public string Nonce { get; set; } = "";
            public long Timestamp { get; set; }
            public long SolveTimeMs { get; set; }
        }
    }
}
