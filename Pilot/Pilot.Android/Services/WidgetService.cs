//extern alias AndroidSupportMedia; // Because "The type 'MediaSessionCompat' exists in both 'Xamarin.Android.Support.Media.Compat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' and 'Xamarin.AndroidX.Media, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'"

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
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
    class WidgetService : BroadcastReceiver, IWidgetService
    {
        public Context context;
        private Intent previousIntent;
        private Intent playStopIntent;
        private Intent nextIntent;
        private NotificationManager notificationManager;

        const string channelId = "default";
        const int pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const int messageId = 0;

        public static bool initialised = false;
        private MediaSessionCompat mediaSessionCompat;
        private PlaybackStateCompat playbackStateCompat;

        private IntentFilter intentFilter;

        private void CreateMediaSession()
        {
            mediaSessionCompat = new MediaSessionCompat(context, channelId);

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

            previousIntent = new Intent("Previous");
            playStopIntent = new Intent("PlayStop");
            nextIntent = new Intent("Next");

            intentFilter = new IntentFilter();
            intentFilter.AddAction("Previous");
            intentFilter.AddAction("PlayStop");
            intentFilter.AddAction("Next");

            context.RegisterReceiver(this, intentFilter);

            CreateMediaSession();

            CreateNotificationChannel();

            AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
            builder.SetVisibility(NotificationCompat.VisibilityPublic)
                .SetSmallIcon(Resource.Mipmap.icon)
                .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, 0)) // #0
                .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 1, playStopIntent, 0))  // #1
                .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 1, nextIntent, 0))     // #2
                .SetContentTitle("Unknown title")
                .SetContentText("Unknown artist")
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

        public void SendCommandToServer(Pilot.Commands command)
        {
            if (ConnectionClass.Send(command) == ConnectionState.SEND_NOT_SUCCESS)
            {
                //RemoveWidget();
            }
        }

        public void UpdateWidget()
        {
            throw new NotImplementedException();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals("Previous"))
            {
                SendCommandToServer(Commands.SEND_PREVIOUS);
            }
            else if (intent.Action.Equals("PlayStop"))
            {
                SendCommandToServer(Commands.SEND_PLAYSTOP);
            }
            else if (intent.Action.Equals("Next"))
            {
                SendCommandToServer(Commands.SEND_NEXT);
            }
        }
    }
}