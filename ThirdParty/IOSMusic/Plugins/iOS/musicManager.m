#import "myManager.h"

// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isn't empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil

void _setupiOSMusic()
{
    [[myManager MusicManager] setupiOSMusic];
}

void _openNativeMusicPicker(bool shouldAppendToPlaylist)
{
    [[myManager MusicManager] openNativeMusicPicker:shouldAppendToPlaylist];
}

void _playPause()
{
    [[myManager MusicManager] playPause];
}

void _loadAudioClip(bool shouldAppendToPlaylist)
{
    [[myManager MusicManager] loadAudioClip:shouldAppendToPlaylist];
}

void _nextSong()
{
    [[myManager MusicManager] nextSong];
}

void _previousSong()
{
    [[myManager MusicManager] previousSong];
}

void _randomSongWithNativeAudioPlayer()
{
    [[myManager MusicManager] randomSongWithNativeAudioPlayer];
}

void _randomSongWithAudioSource()
{
    [[myManager MusicManager] randomSongWithAudioSource];
}

void _queryAppleMusic(const char* productID)
{
    [[myManager MusicManager] queryAppleMusic:GetStringParam(productID)];
}

void _stopAppleMusic()
{
    [[myManager MusicManager] stopAppleMusic];
}
