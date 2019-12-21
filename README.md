# ![](/Doremi_ColorLogoHoriz.png)
Doremi is a music playing system for Unity games that makes it easy to include seamlessly-looped music with multiple variations.

## Note
Doremi is very much a work in progress. It may well have some bugs and issues. If you find any, please report them so we can fix them (or fix them and make a pull request)!

## Basic usage
1. Attach a `DoremiPlayer` component to a Game Object in your scene.
2. In your Project view, create a new `DoremiMusic` asset via "Doremi Audio Engine > Multichannel Loop".
3. Place the `AudioClip`s of the variations for your track in the `Audio` array.
4. Set `Loop Start Samples` to the sample point in your music where the loop should start, and set `Loop End Samples` to the sample point in your music where it should return to the loop start point.
6. Call `DoremiPlayer.Play(DoremiMusic)` with your `DoremiMusic` asset to make your music play!

## Functions

### `DoremiPlayer`

#### `Play(DoremiMusic track)`
Begins playback of the `DoremiMusic` asset `track`, defaulting to variation 0.

#### `Stop()`
Stops playback, losing the music's position. In order to resume playback, you must call `Play()`, which will start the music from the beginning.

#### `Pause()`
Pauses playback. The music's position is not lost, so you can call `Resume()` to resume playback from this point.

#### `Resume()`
Resumes playback from the point where it was paused.

#### `SetChannelVolume(int channel, float volume, float duration = 0f)`
Sets the song variation `channel` to `volume` over the course of `duration` seconds.

#### `FadeOut(float duration)`
Fades the music to a volume of 0 over the course of `duration` seconds.

#### `CurrentPosition`
Returns the current position, in samples, of the music.
**Note:** While it is possible to modify this value in order to "seek" through the music, this is not stable and should not be used.

#### `CurrentPositionFraction`
Returns the normalized current position (where 0 is the beginning of the music, and 1 is the loop end point).

#### `InLoopPortion`
Returns `true` if the music is currently in the loop portion, and `false` otherwise.

#### `CurrentChannel` **(BAD)**
Setting this property will set all variations to a volume of `0` except for the variation specified, which will be set to a volume of `1`. 


### `DoremiMusic`

#### `List<AudioClip> audio`
Contains the `AudioClip`s for each variation of the music track.

#### `int loopEndSamples`
The point in the music, in samples, where the music should loop back to the loop start point.

#### `int loopStartSamples`
The point in the music, in samples, where the music should loop back to, when the loop end point is hit.
