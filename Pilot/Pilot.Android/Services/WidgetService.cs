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
        private MediaSessionCompat mediaSessionCompat;
        private PlaybackStateCompat playbackStateCompat;

        private void CreateMediaSession()
        {
            mediaSessionCompat = new MediaSessionCompat(context, "MediaSession");

            playbackStateCompat = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious).Build();
            mediaSessionCompat.SetMediaButtonReceiver(null);
            mediaSessionCompat.SetPlaybackState(playbackStateCompat);
            mediaSessionCompat.SetCallback(null);

            mediaSessionCompat.Active = true;
        }

        public void CreateWidget()
        {
            if (initialised)
                return;

            context = AndroidApp.Context;

            CreateMediaSession();

            CreateNotificationChannel();

            NotificationCompat.Action playPauseAction = new NotificationCompat.Action(Resource.Mipmap.icon, "default", MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionPlayPause));

            AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
            builder.SetVisibility(NotificationCompat.VisibilityPublic)
                .SetSmallIcon(Resource.Mipmap.icon)
                // Add media control buttons that invoke intents in your media service
                .AddAction(Resource.Drawable.ic_skip_previous, "Previous", MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionSkipToPrevious)) // #0
                .AddAction(Resource.Drawable.ic_play_circle_outline, "Pause", MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionPlayPause))  // #1
                .AddAction(Resource.Drawable.ic_skip_next, "Next", MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionSkipToNext))     // #2
                .SetContentTitle("Wonderful music")
                .SetContentText("My Awesome Band")
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(1 /* #1: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
                .SetOngoing(true)
                .SetNotificationSilent()
                .SetVibrate(new long[] { 0L });

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