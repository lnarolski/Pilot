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
using Pilot.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(Pilot.Droid.Services.WidgetService))]
namespace Pilot.Droid.Services
{
    class MediaSessionCompatCallbacks : MediaSessionCompat.Callback
    {
        public override void OnPlay()
        {
            base.OnPlay();
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnStop()
        {
            base.OnStop();
        }

        public override bool OnMediaButtonEvent(Intent mediaButtonEvent)
        {
            return base.OnMediaButtonEvent(mediaButtonEvent);
        }
    }

    class WidgetService : BroadcastReceiver, IWidgetService
    {
        public Context context;
        private Intent previousIntent;
        private Intent playStopIntent;
        private Intent nextIntent;
        private Intent volumeUpIntent;
        private Intent volumeDownIntent;
        private Intent mediaButtonReceiverIntent;
        private NotificationManager notificationManager;

        const string channelId = "default";

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const int messageId = 0;

        public static bool initialised = false;
        private MediaSessionCompat mediaSessionCompat;
        private PlaybackStateCompat playbackStateCompat;

        private IntentFilter intentFilter;
        private MediaSessionCompatCallbacks mediaSessionCompatCallback = new MediaSessionCompatCallbacks();

        private void CreateMediaSession()
        {
            mediaSessionCompat = new MediaSessionCompat(context, channelId);

            Android.Support.V4.Media.MediaMetadataCompat.Builder mediaMetadataCompat = new Android.Support.V4.Media.MediaMetadataCompat.Builder();
            mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyArtist, AppResources.UnknownArtist);
            mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyTitle, AppResources.UnknownTitle);
            mediaSessionCompat.SetMetadata(mediaMetadataCompat.Build());
            mediaSessionCompat.SetMediaButtonReceiver(PendingIntent.GetBroadcast(context, 1, mediaButtonReceiverIntent, PendingIntentFlags.Immutable));

            playbackStateCompat = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionStop | PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious)
                .SetState(PlaybackStateCompat.StatePaused, PlaybackStateCompat.PlaybackPositionUnknown, 0)
                .Build();

            mediaSessionCompat.SetPlaybackState(playbackStateCompat);
            mediaSessionCompat.SetCallback(mediaSessionCompatCallback);

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
            volumeUpIntent = new Intent("VolumeUp");
            volumeDownIntent = new Intent("VolumeDown");
            mediaButtonReceiverIntent = new Intent(Intent.ActionMediaButton);

            intentFilter = new IntentFilter();
            intentFilter.AddAction("Previous");
            intentFilter.AddAction("PlayStop");
            intentFilter.AddAction("Next");
            intentFilter.AddAction("VolumeUp");
            intentFilter.AddAction("VolumeDown");
            intentFilter.AddAction(Intent.ActionMediaButton);

            context.RegisterReceiver(this, intentFilter);

            CreateMediaSession();

            CreateNotificationChannel();

            AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
            builder.SetVisibility(NotificationCompat.VisibilityPublic)
                .SetSmallIcon(Resource.Mipmap.icon)
                .AddAction(Resource.Drawable.ic_volume_down, "VolumeDown", PendingIntent.GetBroadcast(context, 1, volumeDownIntent, 0)) // #0
                .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, 0)) // #1
                .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 1, playStopIntent, 0))  // #2
                .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 1, nextIntent, 0))     // #3
                .AddAction(Resource.Drawable.ic_volume_up, "VolumeUp", PendingIntent.GetBroadcast(context, 1, volumeUpIntent, 0))     // #4
                .SetContentTitle(AppResources.UnknownTitle)
                .SetContentText(AppResources.UnknownArtist)
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(2 /* #2: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
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
            else if (intent.Action.Equals("VolumeUp"))
            {
                SendCommandToServer(Commands.SEND_VOLUP);
            }
            else if (intent.Action.Equals("VolumeDown"))
            {
                SendCommandToServer(Commands.SEND_VOLDOWN);
            }
            else if (intent.Action.Equals(Intent.ActionMediaButton))
            {
                Bundle extras = intent.Extras;
            }
        }
    }
}