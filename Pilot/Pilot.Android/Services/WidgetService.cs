using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Pilot.Resx;
using AndroidApp = Android.App.Application;
using Microsoft.Maui.Controls;
using AndroidX.Core.App;
using Android.Support.V4.Media.Session;
using Android.Support.V4.Media;
using System;

//[assembly: Dependency(typeof(Pilot.Droid.Services.WidgetService))]
namespace Pilot.Droid.Services
{
    class MediaSessionCompatCallbacks : MediaSessionCompat.Callback
    {
        private WidgetService widgetService;

        public override void OnPlay()
        {
            base.OnPlay();

            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
        }

        public override void OnPause()
        {
            base.OnPause();

            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
        }

        public override void OnStop()
        {
            base.OnStop();

            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
        }

        public override void OnSkipToNext()
        {
            base.OnSkipToNext();

            widgetService.SendCommandToServer(CommandsFromClient.SEND_NEXT);
        }

        public override void OnSkipToPrevious()
        {
            base.OnSkipToPrevious();

            widgetService.SendCommandToServer(CommandsFromClient.SEND_PREVIOUS);
        }

        public override bool OnMediaButtonEvent(Intent mediaButtonEvent)
        {
            return base.OnMediaButtonEvent(mediaButtonEvent);
        }

        public MediaSessionCompatCallbacks(WidgetService widgetService)
        {
            this.widgetService = widgetService;
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

        private const string channelId = "pilotNotificationChannelId";

        private const int messageId = 0;

        private static bool initialised = false;
        private static MediaSessionCompat mediaSessionCompat;
        private PlaybackStateCompat playbackStateCompat;

        private IntentFilter intentFilter;
        private MediaSessionCompatCallbacks mediaSessionCompatCallback;

        private MediaPlayer mediaPlayer;
        private Notification notification;

        private void CreateMediaSession()
        {
            mediaSessionCompatCallback = new MediaSessionCompatCallbacks(this);

            mediaSessionCompat = new MediaSessionCompat(context, channelId);

            MediaMetadataCompat.Builder mediaMetadataCompat = new MediaMetadataCompat.Builder();
            mediaMetadataCompat.PutString(MediaMetadata.MetadataKeyArtist, AppResources.UnknownArtist);
            mediaMetadataCompat.PutString(MediaMetadata.MetadataKeyTitle, AppResources.UnknownTitle);
            mediaSessionCompat.SetMetadata(mediaMetadataCompat.Build());

            PendingIntentFlags pendingIntentFlags;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                pendingIntentFlags = PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable;
            }
            else
            {
                pendingIntentFlags = PendingIntentFlags.CancelCurrent;
            }
            mediaSessionCompat.SetMediaButtonReceiver(PendingIntent.GetBroadcast(context, 5, mediaButtonReceiverIntent, pendingIntentFlags)); //TODO: Not working

            playbackStateCompat = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionStop | PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious)
                .SetState(PlaybackStateCompat.StatePlaying, PlaybackStateCompat.PlaybackPositionUnknown, 0)
                .Build();

            mediaPlayer = MediaPlayer.Create(context, Resource.Raw.silence);
            mediaPlayer.Start();

            mediaSessionCompat.SetPlaybackState(playbackStateCompat);
            mediaSessionCompat.SetCallback(mediaSessionCompatCallback);

            mediaSessionCompat.Active = true;
        }

        public void CreateWidget()
        {
            if (!initialised)
            {
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

                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                {
                    context.RegisterReceiver(this, intentFilter, ReceiverFlags.NotExported);
                }
                else
                {
                    context.RegisterReceiver(this, intentFilter);
                }

                CreateMediaSession();

                CreateNotificationChannel();

                initialised = true;
            }

            // Works only on Android 11
            var resultIntent = new Intent(context, typeof(MainActivity));

            PendingIntentFlags pendingIntentFlags;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                pendingIntentFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
            }
            else
            {
                pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
            }

            PendingIntent notifyPendingIntent = PendingIntent.GetActivity(
                    context, 0, resultIntent, pendingIntentFlags
            );
            //

            PendingIntentFlags cancelCurrentPendingIntentFlags;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                cancelCurrentPendingIntentFlags = PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable;
            }
            else
            {
                cancelCurrentPendingIntentFlags = PendingIntentFlags.CancelCurrent;
            }
            NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(context, channelId);
            notificationBuilder.SetVisibility(NotificationCompat.VisibilityPublic)
                .SetSmallIcon(Resource.Mipmap.icon)
                .AddAction(Resource.Drawable.ic_volume_down, "VolumeDown", PendingIntent.GetBroadcast(context, 0, volumeDownIntent, cancelCurrentPendingIntentFlags)) // #0
                .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, cancelCurrentPendingIntentFlags)) // #1
                .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 2, playStopIntent, cancelCurrentPendingIntentFlags))  // #2
                .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 3, nextIntent, cancelCurrentPendingIntentFlags))     // #3
                .AddAction(Resource.Drawable.ic_volume_up, "VolumeUp", PendingIntent.GetBroadcast(context, 4, volumeUpIntent, cancelCurrentPendingIntentFlags))     // #4
                .SetContentTitle(AppResources.UnknownTitle)
                .SetContentText(AppResources.UnknownArtist)
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(2 /* #2: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
                .SetOngoing(true)
                .SetSilent(true)
                .SetContentIntent(notifyPendingIntent)
                .SetVibrate(new long[] { 0L });
            //;

            notification = notificationBuilder.Build();
            notificationManager.Notify(messageId, notification);
        }

        private void CreateNotificationChannel()
        {
            notificationManager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel channel = new NotificationChannel(channelId, AppResources.NotificationName, NotificationImportance.Default);
                channel.Description = AppResources.NotificationDescription;
                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void RemoveWidget()
        {
            if (notificationManager != null)
            {
                notificationManager.CancelAll();
            }
        }

        public void SendCommandToServer(Pilot.CommandsFromClient command)
        {
            ConnectionClass.Send(command);
        }

        public void UpdateWidget(string artist, string title, byte[] thumbnail)
        {
            if (notification != null && mediaSessionCompat != null)
            {
                PendingIntentFlags cancelCurrentPendingIntentFlags;
                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    cancelCurrentPendingIntentFlags = PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable;
                }
                else
                {
                    cancelCurrentPendingIntentFlags = PendingIntentFlags.CancelCurrent;
                }
                AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
                builder.SetVisibility(NotificationCompat.VisibilityPublic)
                    .SetSmallIcon(Resource.Mipmap.icon)
                    .AddAction(Resource.Drawable.ic_volume_down, "VolumeDown", PendingIntent.GetBroadcast(context, 0, volumeDownIntent, cancelCurrentPendingIntentFlags)) // #0
                    .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, cancelCurrentPendingIntentFlags)) // #1
                    .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 2, playStopIntent, cancelCurrentPendingIntentFlags))  // #2
                    .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 3, nextIntent, cancelCurrentPendingIntentFlags))     // #3
                    .AddAction(Resource.Drawable.ic_volume_up, "VolumeUp", PendingIntent.GetBroadcast(context, 4, volumeUpIntent, cancelCurrentPendingIntentFlags))     // #4
                    .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(2 /* #2: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
                    .SetOngoing(true)
                    .SetSilent(true)
                    .SetVibrate(new long[] { 0L });

                MediaMetadataCompat.Builder mediaMetadataCompat = new MediaMetadataCompat.Builder();

                if (artist.Replace("\0", "") == "")
                {
                    builder.SetContentTitle(AppResources.UnknownArtist);
                    mediaMetadataCompat.PutString(MediaMetadataCompat.MetadataKeyArtist, AppResources.UnknownArtist);
                }
                else
                {
                    builder.SetContentTitle(artist);
                    mediaMetadataCompat.PutString(MediaMetadataCompat.MetadataKeyArtist, artist);
                }

                if (title.Replace("\0", "") == "")
                {
                    builder.SetContentText(AppResources.UnknownTitle);
                    mediaMetadataCompat.PutString(MediaMetadataCompat.MetadataKeyTitle, AppResources.UnknownTitle);
                }
                else
                {
                    builder.SetContentText(title);
                    mediaMetadataCompat.PutString(MediaMetadataCompat.MetadataKeyTitle, title);
                }

                Bitmap bitmap = null;
                if (thumbnail != null)
                {
                    bitmap = BitmapFactory.DecodeByteArray(thumbnail, 0, thumbnail.Length);
                }
                builder.SetLargeIcon(bitmap);

                notification = builder.Build();
                notificationManager.Notify(messageId, notification);

                mediaSessionCompat.SetMetadata(mediaMetadataCompat.Build());
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals("Previous"))
            {
                SendCommandToServer(CommandsFromClient.SEND_PREVIOUS);
            }
            else if (intent.Action.Equals("PlayStop"))
            {
                SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
            }
            else if (intent.Action.Equals("Next"))
            {
                SendCommandToServer(CommandsFromClient.SEND_NEXT);
            }
            else if (intent.Action.Equals("VolumeUp"))
            {
                SendCommandToServer(CommandsFromClient.SEND_VOLUP);
            }
            else if (intent.Action.Equals("VolumeDown"))
            {
                SendCommandToServer(CommandsFromClient.SEND_VOLDOWN);
            }
        }
    }
}