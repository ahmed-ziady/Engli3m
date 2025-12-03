//using FirebaseAdmin;
//using FirebaseAdmin.Messaging;
//using Google.Apis.Auth.OAuth2;
//namespace Engli3m.Infrastructure.Services
//{
//    public class FirebaseService
//    {

//        public FirebaseService()
//        {
//            if (FirebaseApp.DefaultInstance == null)
//            {
//                FirebaseApp.Create(new AppOptions
//                {
//                    Credential = GoogleCredential.FromFile("englizm--notifications-firebase-adminsdk-fbsvc-cba192965a.json")
//                });
//            }
//        }

//        public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
//        {
//            var message = new Message()
//            {
//                Token = deviceToken,
//                Notification = new Notification
//                {
//                    Title = title,
//                    Body = body
//                },
//                Data = data ?? new Dictionary<string, string>()
//            };

//            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
//        }
//    }
//}