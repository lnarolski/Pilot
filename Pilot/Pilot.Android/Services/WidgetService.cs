//using Android.App;
//using Android.Content;
//using Android.Graphics;
//using Android.Media;
//using Android.Media.Session;
//using Android.OS;
//using Android.Runtime;
//using Android.Service.Media;
//using Android.Support.V4.App;
//using Android.Support.V4.Media;
//using Android.Support.V4.Media.Session;
//using Android.Views;
//using Android.Widget;
//using AndroidX.Media.Session;
//using Java.Interop;
//using Java.Lang;
//using Pilot.Resx;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using AndroidApp = Android.App.Application;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui;

//[assembly: Dependency(typeof(Pilot.Droid.Services.WidgetService))]
//namespace Pilot.Droid.Services
//{
//    class MediaSessionCompatCallbacks : MediaSessionCompat.Callback
//    {
//        private WidgetService widgetService;

//        public override void OnPlay()
//        {
//            base.OnPlay();

//            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
//        }

//        public override void OnPause()
//        {
//            base.OnPause();

//            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
//        }

//        public override void OnStop()
//        {
//            base.OnStop();

//            widgetService.SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
//        }

//        public override void OnSkipToNext()
//        {
//            base.OnSkipToNext();

//            widgetService.SendCommandToServer(CommandsFromClient.SEND_NEXT);
//        }

//        public override void OnSkipToPrevious()
//        {
//            base.OnSkipToPrevious();

//            widgetService.SendCommandToServer(CommandsFromClient.SEND_PREVIOUS);
//        }

//        public override bool OnMediaButtonEvent(Intent mediaButtonEvent)
//        {
//            return base.OnMediaButtonEvent(mediaButtonEvent);
//        }

//        public MediaSessionCompatCallbacks(WidgetService widgetService)
//        {
//            this.widgetService = widgetService;
//        }
//    }

//    class WidgetService : BroadcastReceiver, IWidgetService
//    {
//        public Context context;
//        private Intent previousIntent;
//        private Intent playStopIntent;
//        private Intent nextIntent;
//        private Intent volumeUpIntent;
//        private Intent volumeDownIntent;
//        private Intent mediaButtonReceiverIntent;
//        private NotificationManager notificationManager;

//        private const string channelId = "pilotNotificationChannelId";

//        private const int messageId = 0;

//        private static bool initialised = false;
//        private static MediaSessionCompat mediaSessionCompat;
//        private PlaybackStateCompat playbackStateCompat;

//        private IntentFilter intentFilter;
//        private MediaSessionCompatCallbacks mediaSessionCompatCallback;

//        private MediaPlayer mediaPlayer;
//        private Notification notification;

//        private void CreateMediaSession()
//        {
//            mediaSessionCompatCallback = new MediaSessionCompatCallbacks(this);

//            mediaSessionCompat = new MediaSessionCompat(context, channelId);

//            Android.Support.V4.Media.MediaMetadataCompat.Builder mediaMetadataCompat = new Android.Support.V4.Media.MediaMetadataCompat.Builder();
//            mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyArtist, AppResources.UnknownArtist);
//            mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyTitle, AppResources.UnknownTitle);
//            mediaSessionCompat.SetMetadata(mediaMetadataCompat.Build());
//            mediaSessionCompat.SetMediaButtonReceiver(PendingIntent.GetBroadcast(context, 5, mediaButtonReceiverIntent, PendingIntentFlags.CancelCurrent)); //TODO: Not working

//            playbackStateCompat = new PlaybackStateCompat.Builder().SetActions(PlaybackStateCompat.ActionStop | PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious)
//                .SetState(PlaybackStateCompat.StateBuffering, PlaybackStateCompat.PlaybackPositionUnknown, 0)
//                .Build();

//            mediaPlayer = MediaPlayer.Create(context, Resource.Raw.silence);
//            mediaPlayer.Start();

//            mediaSessionCompat.SetPlaybackState(playbackStateCompat);
//            mediaSessionCompat.SetCallback(mediaSessionCompatCallback);

//            mediaSessionCompat.Active = true;
//        }

//        public void CreateWidget()
//        {
//            if (!initialised)
//            {
//                context = AndroidApp.Context;

//                previousIntent = new Intent("Previous");
//                playStopIntent = new Intent("PlayStop");
//                nextIntent = new Intent("Next");
//                volumeUpIntent = new Intent("VolumeUp");
//                volumeDownIntent = new Intent("VolumeDown");
//                mediaButtonReceiverIntent = new Intent(Intent.ActionMediaButton);

//                intentFilter = new IntentFilter();
//                intentFilter.AddAction("Previous");
//                intentFilter.AddAction("PlayStop");
//                intentFilter.AddAction("Next");
//                intentFilter.AddAction("VolumeUp");
//                intentFilter.AddAction("VolumeDown");
//                intentFilter.AddAction(Intent.ActionMediaButton);

//                context.RegisterReceiver(this, intentFilter);

//                CreateMediaSession();

//                CreateNotificationChannel();

//                initialised = true;
//            }

//            // Works only on Android 11
//            var resultIntent = new Intent(context, typeof(MainActivity));
//            PendingIntent notifyPendingIntent = PendingIntent.GetActivity(
//                    context, 0, resultIntent, PendingIntentFlags.UpdateCurrent
//            );
//            //

//            AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
//            builder.SetVisibility(NotificationCompat.VisibilityPublic)
//                .SetSmallIcon(Resource.Mipmap.icon)
//                .AddAction(Resource.Drawable.ic_volume_down, "VolumeDown", PendingIntent.GetBroadcast(context, 0, volumeDownIntent, PendingIntentFlags.CancelCurrent)) // #0
//                .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, PendingIntentFlags.CancelCurrent)) // #1
//                .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 2, playStopIntent, PendingIntentFlags.CancelCurrent))  // #2
//                .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 3, nextIntent, PendingIntentFlags.CancelCurrent))     // #3
//                .AddAction(Resource.Drawable.ic_volume_up, "VolumeUp", PendingIntent.GetBroadcast(context, 4, volumeUpIntent, PendingIntentFlags.CancelCurrent))     // #4
//                .SetContentTitle(AppResources.UnknownTitle)
//                .SetContentText(AppResources.UnknownArtist)
//                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(2 /* #2: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
//                .SetOngoing(true)
//                .SetSilent(true)
//                .SetContentIntent(notifyPendingIntent)
//                .SetVibrate(new long[] { 0L });

//            notification = builder.Build();
//            notificationManager.Notify(messageId, notification);
//        }

//        private void CreateNotificationChannel()
//        {
//            notificationManager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

//            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
//            {
//                NotificationChannel channel = new NotificationChannel(channelId, AppResources.NotificationName, NotificationImportance.Default);
//                channel.Description = AppResources.NotificationDescription;
//                // Register the channel with the system; you can't change the importance
//                // or other notification behaviors after this
//                notificationManager.CreateNotificationChannel(channel);
//            }
//        }

//        public void RemoveWidget()
//        {
//            if (notificationManager != null)
//            {
//                notificationManager.CancelAll();
//            }
//        }

//        public void SendCommandToServer(Pilot.CommandsFromClient command)
//        {
//            ConnectionClass.Send(command);
//        }

//        public void UpdateWidget(string artist, string title, byte[] thumbnail)
//        {
//            if (notification != null && mediaSessionCompat != null)
//            {
//                AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId);
//                builder.SetVisibility(NotificationCompat.VisibilityPublic)
//                    .SetSmallIcon(Resource.Mipmap.icon)
//                    .AddAction(Resource.Drawable.ic_volume_down, "VolumeDown", PendingIntent.GetBroadcast(context, 0, volumeDownIntent, PendingIntentFlags.CancelCurrent)) // #0
//                    .AddAction(Resource.Drawable.ic_skip_previous, "Previous", PendingIntent.GetBroadcast(context, 1, previousIntent, PendingIntentFlags.CancelCurrent)) // #1
//                    .AddAction(Resource.Drawable.ic_play_circle_outline, "PlayStop", PendingIntent.GetBroadcast(context, 2, playStopIntent, PendingIntentFlags.CancelCurrent))  // #2
//                    .AddAction(Resource.Drawable.ic_skip_next, "Next", PendingIntent.GetBroadcast(context, 3, nextIntent, PendingIntentFlags.CancelCurrent))     // #3
//                    .AddAction(Resource.Drawable.ic_volume_up, "VolumeUp", PendingIntent.GetBroadcast(context, 4, volumeUpIntent, PendingIntentFlags.CancelCurrent))     // #4
//                    .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(2 /* #2: pause button */).SetMediaSession(mediaSessionCompat.SessionToken))
//                    .SetOngoing(true)
//                    .SetSilent(true)
//                    .SetVibrate(new long[] { 0L });

//                Android.Support.V4.Media.MediaMetadataCompat.Builder mediaMetadataCompat = new Android.Support.V4.Media.MediaMetadataCompat.Builder();

//                if (artist.Replace("\0", "") == "")
//                {
//                    builder.SetContentTitle(AppResources.UnknownArtist);
//                    mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyArtist, AppResources.UnknownArtist);
//                }
//                else
//                {
//                    builder.SetContentTitle(artist);
//                    mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyArtist, artist);
//                }

//                if (title.Replace("\0", "") == "")
//                {
//                    builder.SetContentText(AppResources.UnknownTitle);
//                    mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyTitle, AppResources.UnknownTitle);
//                }
//                else
//                {
//                    builder.SetContentText(title);
//                    mediaMetadataCompat.PutString(Android.Support.V4.Media.MediaMetadataCompat.MetadataKeyTitle, title);
//                }

//                Bitmap bitmap = null;
//                if (thumbnail != null)
//                {
//                    bitmap = BitmapFactory.DecodeByteArray(thumbnail, 0, thumbnail.Length);
//                }
//                builder.SetLargeIcon(bitmap);

//                notification = builder.Build();
//                notificationManager.Notify(messageId, notification);

//                mediaSessionCompat.SetMetadata(mediaMetadataCompat.Build());
//            }
//        }

//        public override void OnReceive(Context context, Intent intent)
//        {
//            if (intent.Action.Equals("Previous"))
//            {
//                SendCommandToServer(CommandsFromClient.SEND_PREVIOUS);
//            }
//            else if (intent.Action.Equals("PlayStop"))
//            {
//                SendCommandToServer(CommandsFromClient.SEND_PLAYSTOP);
//            }
//            else if (intent.Action.Equals("Next"))
//            {
//                SendCommandToServer(CommandsFromClient.SEND_NEXT);
//            }
//            else if (intent.Action.Equals("VolumeUp"))
//            {
//                SendCommandToServer(CommandsFromClient.SEND_VOLUP);
//            }
//            else if (intent.Action.Equals("VolumeDown"))
//            {
//                SendCommandToServer(CommandsFromClient.SEND_VOLDOWN);
//            }
//        }
//    }
//}