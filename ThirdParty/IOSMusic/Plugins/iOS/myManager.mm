#import "myManager.h"

void UnityPause(int pause);
void UnitySetAudioSessionActive(bool active);
UIViewController *UnityGetGLViewController();

@interface myManager()

@property(nonatomic, retain)MPMediaPickerController *myPicker;
@property(nonatomic,retain)AVAudioPlayer *myPlayer;
@property(nonatomic,retain)NSMutableArray *myPlaylist;
@property(nonatomic)NSUInteger currentIndex;
@property(nonatomic)bool isAppending;

@end

@implementation myManager

BOOL isPad() {
#ifdef UI_USER_INTERFACE_IDIOM
    return (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad);
#else
    return NO;
#endif
}

+(myManager*)MusicManager
{
    static myManager *sharedSingleton;
    
    if(!sharedSingleton)
        sharedSingleton = [[myManager alloc] init];
    
    return sharedSingleton;
}

-(id)init
{
    if((self = [super init]))
    {
        
    }
    
    return self;
}

 - (BOOL)prefersStatusBarHidden {

   return YES;
}

- (UIViewController *)childViewControllerForStatusBarHidden {
    return self.myPicker;
}

-(void)setupiOSMusic {
    [MPMediaLibrary requestAuthorization:^(MPMediaLibraryAuthorizationStatus status){
        switch (status) {
            case MPMediaLibraryAuthorizationStatusNotDetermined: {
                NSLog(@"Authorization status not determined");
                break;
            }
            case MPMediaLibraryAuthorizationStatusRestricted: {
                NSLog(@"Authorization status restricted");
                break;
            }
            case MPMediaLibraryAuthorizationStatusDenied: {
                NSLog(@"Authorization status denied");
                break;
            }
            case MPMediaLibraryAuthorizationStatusAuthorized: {
                NSLog(@"Authorization status authorized");
                break;
            }
            default: {
                break;
            }
        }
    }];
}

-(void)showMusicLibrary:(bool)shouldAppendToPlaylist
{
    UnityPause( 1 );
    
    [_myPlayer stop];
    
    _isAppending = shouldAppendToPlaylist;
    NSLog(@"Opening music library with append mode: %d", _isAppending);
    
    self.myPicker = [[MPMediaPickerController alloc] initWithMediaTypes:MPMediaTypeMusic];
    self.myPicker.delegate = self;
    self.myPicker.allowsPickingMultipleItems = true;
    self.myPicker.showsCloudItems = NO;
    self.myPicker.showsItemsWithProtectedAssets = NO;
    self.myPicker.modalPresentationCapturesStatusBarAppearance = YES;
    [self.myPicker setModalPresentationStyle:UIModalPresentationPopover];
    
    if (isPad )
    {
        //self.myPicker.modalPresentationStyle = UIModalPresentationFormSheet;
		self.myPicker.modalPresentationStyle = UIModalPresentationFullScreen;
    }
    
    UIViewController *containerController = [[UIViewController alloc] init];
    [containerController.view addSubview:self.myPicker.view];
    
    [UnityGetGLViewController() presentViewController:self.myPicker animated:YES completion:nil];
}

-(void)playPause
{
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    if([[myDefaults objectForKey:@"isLoadingAudioClip"] isEqualToString:@"0"])
    {
        if([_myPlayer isPlaying])
        {
            [_myPlayer pause];
        } else
        {
            [_myPlayer setDelegate:self];
            [_myPlayer prepareToPlay];
            [_myPlayer play];
        }
    }
}

-(void)cueNextAudioClip
{
    [self nextSong];
}

-(void)nextSong
{
    _currentIndex++;
    
    MPMediaItem *mySong;
    if(_currentIndex < [_myPlaylist count])
    {
        mySong = (MPMediaItem*)[_myPlaylist objectAtIndex:_currentIndex];
    } else
    {
        // Loop back to the start of the playlist if user presses "Next Song" while on the playlist's last song.
        _currentIndex = 0;
        mySong = (MPMediaItem*)[_myPlaylist objectAtIndex:_currentIndex];
    }
    
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    if([[myDefaults objectForKey:@"isLoadingAudioClip"] isEqualToString:@"1"])
    {
        NSLog(@"Loading %@ as an AudioClip!", [mySong title]);
        [self exportSong:mySong];
    } else {
        NSError *err;
        self.myPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:[mySong assetURL] error:&err];
        
        if(err)
        {
            NSLog(@"Error when playing %@ %@: %@", [mySong artist], [mySong title], err);
        }
        
        [_myPlayer setDelegate:self];
        [_myPlayer prepareToPlay];
        [_myPlayer play];
        
        [self extractMetaData:mySong];
        
        UnitySendMessage("IOSAudioPlayer", "ResetButtonStates", "");
    }
}

-(void)previousSong
{
    MPMediaItem *mySong;
    if(_currentIndex > 0)
    {
        _currentIndex--;
        mySong = (MPMediaItem*)[_myPlaylist objectAtIndex:_currentIndex];
    } else
    {
        // Loop back to the end of the playlist if user presses "Previous Song" while on the playlist's first song.
        _currentIndex = [_myPlaylist count] - 1;
        mySong = (MPMediaItem*)[_myPlaylist objectAtIndex:_currentIndex];
    }
    
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    if([[myDefaults objectForKey:@"isLoadingAudioClip"] isEqualToString:@"1"])
    {
        NSLog(@"Loading %@ as an AudioClip!", [mySong title]);
        [self exportSong:mySong];
    } else {
        NSError *err;
        self.myPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:[mySong assetURL] error:&err];
        
        if(err)
        {
            NSLog(@"Error when playing %@ %@: %@", [mySong artist], [mySong title], err);
        }
        
        [_myPlayer setDelegate:self];
        [_myPlayer prepareToPlay];
        [_myPlayer play];
        
        [self extractMetaData:mySong];
        
        UnitySendMessage("IOSAudioPlayer", "ResetButtonStates", "");
    }
}

#pragma mark - MPMediaPickerController delegate

-(void)mediaPicker:(MPMediaPickerController *)mediaPicker didPickMediaItems:(MPMediaItemCollection *) mediaItemCollection
{
    NSLog(@"reached didpickmediaitems");
    
    if(!_isAppending)
    {
        self.myPlaylist = [[NSMutableArray alloc] initWithArray:[mediaItemCollection items]];
    } else {
        self.myPlaylist = [[NSMutableArray alloc] initWithArray:_myPlaylist];
        for(int i=0; i<[mediaItemCollection items].count; i++)
        {
            [_myPlaylist addObject:[[mediaItemCollection items] objectAtIndex:i]];
        }
    }
    
    for(int i=0; i<_myPlaylist.count; i++)
    {
        NSLog(@"index %i: %@", i, [(MPMediaItem*)[_myPlaylist objectAtIndex:i] title]);
        
        if([[(MPMediaItem*)[mediaItemCollection.items objectAtIndex:0] title] isEqualToString:[(MPMediaItem*)[_myPlaylist objectAtIndex:i] title]])
        {
            _currentIndex = i;
        }
    }
    
    MPMediaItem *mySong = (MPMediaItem*)[mediaItemCollection.items objectAtIndex:0];
    
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    
    if([[myDefaults objectForKey:@"isLoadingAudioClip"] isEqualToString:@"1"])
    {
        [self exportSong:mySong];
        
    } else {
        
        NSError *err;
        self.myPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:[mySong assetURL] error:&err];
        
        if(err)
        {
            NSLog(@"Error when selecting %@ %@: %@", [mySong artist], [mySong title], err);
        }
        
        [_myPlayer setDelegate:self];
        [_myPlayer prepareToPlay];
        [_myPlayer play];
        
        [self extractMetaData:mySong];
    }
    
    [UnityGetGLViewController() dismissViewControllerAnimated:YES completion:nil];
    UnityPause(0);
}

-(void)mediaPickerDidCancel:(MPMediaPickerController *)mediaPicker{
    
    UnitySendMessage("IOSAudioPlayer", "UserDidCancel", "");
    [UnityGetGLViewController() dismissViewControllerAnimated:YES completion:nil];
    UnityPause(0);
}

-(void)openNativeMusicPicker:(bool)shouldAppendToPlaylist
{
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    [myDefaults setObject:@"0" forKey:@"isLoadingAudioClip"];
    [myDefaults synchronize];
    
    [self showMusicLibrary:shouldAppendToPlaylist];
}

-(void)loadAudioClip:(bool)shouldAppendToPlaylist
{
    NSUserDefaults *myDefaults = [NSUserDefaults standardUserDefaults];
    [myDefaults setObject:@"1" forKey:@"isLoadingAudioClip"];
    [myDefaults synchronize];
    
    [self showMusicLibrary:shouldAppendToPlaylist];
}

-(void)randomSongWithNativeAudioPlayer
{
    MPMediaQuery *songsQuery = [MPMediaQuery songsQuery];
    [songsQuery setGroupingType:MPMediaGroupingArtist];
    NSArray *mediaCollections = [songsQuery collections];
    NSMutableArray *allItems = [[NSMutableArray alloc] init];

    for(int i=0; i<mediaCollections.count; i++)
    {
        MPMediaItemCollection *songCollection = mediaCollections[i];
        
        for(int j=0; j<songCollection.items.count; j++)
        {
            [allItems addObject:songCollection.items[j]];
        }
    }
    
    MPMediaItem *chosenItem = (MPMediaItem*)[allItems objectAtIndex:(arc4random() % [allItems count])];
    
    NSError *err;
    self.myPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:[chosenItem assetURL] error:&err];
    
    if(err)
    {
        NSLog(@"Error when randomly choosing %@ %@: %@", [chosenItem artist], [chosenItem title], err);
    }
    
    [_myPlayer setDelegate:self];
    [_myPlayer prepareToPlay];
    [_myPlayer play];
    
    [self extractMetaData:chosenItem];
    
}

-(void)randomSongWithAudioSource
{
    MPMediaQuery *songsQuery = [MPMediaQuery songsQuery];
    [songsQuery addFilterPredicate:[MPMediaPropertyPredicate predicateWithValue:@(NO)
                       forProperty:MPMediaItemPropertyIsCloudItem]];
    [songsQuery setGroupingType:MPMediaGroupingArtist];
    NSArray *mediaCollections = [songsQuery collections];
    
    NSMutableArray *allItems = [[NSMutableArray alloc] init];

    for(int i=0; i<mediaCollections.count; i++)
    {
        MPMediaItemCollection *songCollection = mediaCollections[i];
        for(int j=0; j<songCollection.items.count; j++)
        {
            [allItems addObject:songCollection.items[j]];
        }
    }
    
    [self exportSong:[allItems objectAtIndex:(arc4random() % [allItems count])]];
}

-(void)extractMetaData:(MPMediaItem*)song
{
    NSString *songTitle = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyTitle]];
    NSString *songArtist = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyArtist]];
    NSString *songAlbumTitle = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyAlbumTitle]];
    NSString *songBPM = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyBeatsPerMinute]];
    NSString *songGenre = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyGenre]];
    NSString *songLyrics = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyLyrics]];
    NSString *songDuration = [NSString stringWithFormat:@"%@", [song valueForProperty:MPMediaItemPropertyPlaybackDuration]];
    UIImage *songArtwork = [[song valueForProperty:MPMediaItemPropertyArtwork] imageWithSize:CGSizeMake(320.0f, 320.0f)];
    
    [self extractArtwork:songArtwork];
    
    UnitySendMessage("IOSAudioPlayer", "ExtractTitle", [songTitle UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractArtist", [songArtist UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractAlbumTitle", [songAlbumTitle UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractBPM", [songBPM UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractGenre", [songGenre UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractLyrics", [songLyrics UTF8String]);
    UnitySendMessage("IOSAudioPlayer", "ExtractDuration", [songDuration UTF8String]);
}

- (void)extractArtwork:(UIImage *)artwork
{
    NSData *imageData = UIImagePNGRepresentation(artwork);
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    NSString *filename = [NSString stringWithFormat:@"%@.png",@"songArtwork"];
    NSString *imagePath = [documentsDirectory stringByAppendingPathComponent:filename];
    if (![imageData writeToFile:imagePath atomically:NO])
    {
        NSLog(@"Extraction of song artwork has failed.");
    }
    else
    {
        UnitySendMessage("IOSAudioPlayer", "ExtractArtwork", "");
        UnitySendMessage("AppleMusicController", "ExtractArtwork", "");
    }
}

-(void)exportSong:(MPMediaItem*)song{
    
    if (![song valueForProperty:MPMediaItemPropertyAssetURL]) {
        return;
    }
    
    NSURL *assetURL = [song valueForProperty:MPMediaItemPropertyAssetURL];
    AVURLAsset *songAsset = [AVURLAsset URLAssetWithURL:assetURL options:nil];
    
    AVAssetExportSession *exporter = [[AVAssetExportSession alloc]
                                      initWithAsset:songAsset
                                      presetName:AVAssetExportPresetAppleM4A];
    exporter.outputFileType = @"com.apple.m4a-audio";
    NSString *exportFile = [[self myDocumentsDirectory] stringByAppendingPathComponent:[NSString stringWithFormat:@"song.m4a"]];
    
    [self deleteFile:exportFile];
    NSURL *exportURL = [NSURL fileURLWithPath:exportFile];
    exporter.outputURL = exportURL;
    
    NSLog(@"Starting export of song %@ ...", [song title]);
    [exporter exportAsynchronouslyWithCompletionHandler:^{
        int exportStatus = exporter.status;
        switch (exportStatus) {
            case AVAssetExportSessionStatusFailed: {
                NSError *exportError = exporter.error;
                NSLog (@"AVAssetExportSessionStatusFailed: %@", exportError);
                break;
            }
            case AVAssetExportSessionStatusCompleted: {
                NSLog (@"Song %@ export completed. Loading into the AudioSource's AudioClip...", [song title]);
                UnitySendMessage("IOSAudioPlayer", "LoadAudioClip", "");
                [self extractMetaData:song];
                break;
            }
            case AVAssetExportSessionStatusUnknown: {
                NSLog (@"AVAssetExportSessionStatusUnknown");
                break;
            }
            case AVAssetExportSessionStatusExporting: {
                NSLog (@"AVAssetExportSessionStatusExporting");
                break;
            }
            case AVAssetExportSessionStatusCancelled: {
                NSLog (@"AVAssetExportSessionStatusCancelled");
                break;
            }
            case AVAssetExportSessionStatusWaiting: {
                NSLog (@"AVAssetExportSessionStatusWaiting");
                break;
            }
            default: {
                NSLog (@"Could not determine status of song export...");
                break;
            }
        }
    }];
}

-(void)queryAppleMusic:(NSString*)productID
{
    [SKCloudServiceController requestAuthorization:^(SKCloudServiceAuthorizationStatus status) {
        NSLog(@"status is %ld", (long)status);
        SKCloudServiceController *cloudServiceController;
        cloudServiceController = [[SKCloudServiceController alloc] init];
        [cloudServiceController requestCapabilitiesWithCompletionHandler:^(SKCloudServiceCapability capabilities, NSError * _Nullable error) {
            if (capabilities >= SKCloudServiceCapabilityAddToCloudMusicLibrary)
            {
                [[MPMediaLibrary defaultMediaLibrary] addItemWithProductID:productID completionHandler:^(NSArray <__kindof MPMediaEntity *> *entities, NSError * __nullable error)
                 {
                     NSArray *tracksToPlay = [NSArray arrayWithObject:productID];
                     [[MPMusicPlayerController systemMusicPlayer] setQueueWithStoreIDs:tracksToPlay];
                     [[MPMusicPlayerController systemMusicPlayer] play];
                     [self performSelectorOnMainThread:@selector(waitForResponseFromAppleMusic:) withObject:productID waitUntilDone:YES];
                 }];
            }
            else
            {
                UnitySendMessage("AppleMusicController", "DidReceiveResponse", "Unable to access Apple Music Library.");
            }
        }];
    }];
}

-(void)waitForResponseFromAppleMusic:(NSString*)productID
{
    if (([[MPMusicPlayerController systemMusicPlayer] nowPlayingItem]) && ([[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] playbackDuration]))
    {
        UnitySendMessage("AppleMusicController", "ExtractTitle", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyTitle] UTF8String]);
        UnitySendMessage("AppleMusicController", "ExtractArtist", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyArtist] UTF8String]);
        UnitySendMessage("AppleMusicController", "ExtractDuration", [[NSString stringWithFormat:@"%i", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyPlaybackDuration] intValue]] UTF8String]);
        UnitySendMessage("AppleMusicController", "ExtractAlbumTitle", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyAlbumTitle] UTF8String]);
        UnitySendMessage("AppleMusicController", "ExtractGenre", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyGenre] UTF8String]);
        UnitySendMessage("AppleMusicController", "ExtractLyrics", [[[[MPMusicPlayerController systemMusicPlayer] nowPlayingItem] valueForProperty:MPMediaItemPropertyLyrics] UTF8String]);
    }
    else
    {
        NSLog(@"Waiting for response from Apple Music...");
        [self performSelector:@selector(waitForResponseFromAppleMusic:) withObject:productID afterDelay:1.0];
    }
}

-(void)stopAppleMusic
{
    [[MPMusicPlayerController systemMusicPlayer] stop];
}

- (void)audioPlayerDidFinishPlaying:(AVAudioPlayer *)player successfully:(BOOL)flag
{
    [self cueNextAudioClip];
}

-(NSString*)myDocumentsDirectory
{
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    return [paths objectAtIndex:0];
}

-(void)deleteFile:(NSString*)path
{
    if ([[NSFileManager defaultManager] fileExistsAtPath:path]) {
        NSError *deleteError = nil;
        [[NSFileManager defaultManager] removeItemAtPath:path error:&deleteError];
        if (deleteError) {
            NSLog (@"Can't delete %@: %@", path, deleteError);
        }
    }
}

@end
