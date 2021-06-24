//extern alias AndroidSupportMedia; // Because "The type 'MediaSessionCompat' exists in both 'Xamarin.Android.Support.Media.Compat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' and 'Xamarin.AndroidX.Media, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'"

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Service.Media;
using Android.Support.V4.App;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using AndroidX.Media.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(Pilot.Droid.Services.WidgetService))]
namespace Pilot.Droid.Services
{
    class WidgetService : IWidgetService
    {
        public Context context;
        private NotificationManager notificationManager;

        const string channelId = "default";
        const int pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const int messageId = 0;

        public static bool initialised = false;

        //private void CreateMediaSession()
        //{
        //    mediaSessionCompat = new MediaSessionCompat(context, "MediaSession");

        //    playbackStateCompat = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious).Build();
        //    mediaSessionCompat.SetMediaButtonReceiver(null);
        //    mediaSessionCompat.SetPlaybackState(playbackStateCompat);
        //    mediaSessionCompat.SetCallback(null);

        //    mediaSessionCompat.Active = true;
        //}

        public void CreateWidget()
        {
            if (initialised)
                return;

            //CreateMediaSession();
            context = AndroidApp.Context;

            CreateNotificationChannel();

            Intent intent = new Intent(context, typeof(MainActivity));
            intent.PutExtra(TitleKey, TitleKey);
            intent.PutExtra(MessageKey, MessageKey);

            PendingIntent pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(TitleKey)
                .SetContentText(MessageKey)
                .SetSmallIcon(Resource.Mipmap.icon)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            notificationManager.Notify(messageId, notification);

            initialised = true;
        }

        private void CreateNotificationChannel()
        {
            notificationManager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                string description = "Pilot WidgetService notification channel";
                NotificationChannel channel = new NotificationChannel(channelId, "pilotChannelCharSequence", NotificationImportance.Default);
                channel.Description = description;
                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void RemoveWidget()
        {
            throw new NotImplementedException();
        }

        public void SendCommandToServer()
        {
            throw new NotImplementedException();
        }

        public void UpdateWidget()
        {
            throw new NotImplementedException();
        }
    }
}