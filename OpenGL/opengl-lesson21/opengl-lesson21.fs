\ ===================================================================
\           File: opengl-lesson21.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 08/01/2010
\    Description: Lines, timing, ortho, sound
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 21
\ ===================================================================
\ This code was created by Jeff Molofee '00
\ (ported to Linux/SDL by Ti Leggett '01)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ Marker ]------------------------------------------------------
\ Allows for easy removal of a previous loading of the program, for
\ re-compilation of changes.
\ If ---marker--- exists, execute it to restore the dictionary.

[IFDEF] ---marker---
  ---marker---
[ENDIF]

\ ---[ New Marker ]--------------------------------------------------
\ Set a marker point in the dictionary.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ If/when the program is re-loaded, everything in the dictionary
\ after this point will be unlinked (removed). Essentially 'forget'.
\ Does NOT affect linked libcc code, however.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ Some programmers prefer to exit/re-enter gforth to ensure that they
\ are starting with a clean slate each time.  Your choice.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

marker ---marker---

\ Set markers for base free dictionary memory, and start time
unused constant  base-user-memory
utime  2constant compilation-start-time

\ ---[ Set Number Base ]---------------------------------------------
\ Set the numeric system to base 10

decimal

\ ---[ Prototype Listing ]-------------------------------------------
\ : zprint                      { *str -- }
\ : zstring                     ( *str len -- *str )
\ : SF,                         ( r -- )
\ : Initialize-SDL              ( -- )
\ : Get-Video-Info              ( -- )
\ : Compile-Video-Flags         ( -- )
\ : Check-HW-Surfaces           ( -- )
\ : Check-HW-Blits              ( -- )
\ : Init-Double-Buffering       ( -- )
\ : Init-Video                  ( -- )
\ : Init-Caption                ( -- )
\ : Init-Keyboard               ( -- )
\ : ResizeWindow                { _width _height -- boolean }
\ : Init-Audio                  ( -- )                            NEW
\ : Help-Msg                    ( -- )
\ : Init-Game-Vars              ( -- )
\ : lesson21                    ( -- )
\ ------------------------------------------------[End Prototypes]---

cr cr .( Loading Tutorial...) cr

\ ---[ opengl-exit-flag ]--------------------------------------------
\ Boolean flag set by HandleKeyPress if the ESC key is pressed.
\ Will be used in a begin..while..repeat loop in the main function.

FALSE value opengl-exit-flag

\ ---[ screen ]------------------------------------------------------
\ Pointer for working SDL surface

0 value screen

\ ---[ Screen Dimensions ]-------------------------------------------
\ These specify the size/depth of the SDL display surface

640 constant screen-width
480 constant screen-height
16  constant screen-bpp

\ ---[ Additional Variable Declarations ]----------------------------


\ ---[ Sound ]-------------------------------------------------------
\ Lesson 21 includes the use of the SDL_Mixer package.  If you wish
\ to implement the sound capability, set this VALUE to TRUE.  If not,
\ then set it to FALSE.  The audio adds something to the Lesson.

TRUE value Sound                                         \ sound flag

0 value game-time

\ ===[ Ancilliary Support Routines ]=================================
\ These are support routines that help with the normal programming.

\ ---[ zprint ]------------------------------------------------------
\ Displays a zero-terminated string
  
: zprint { *str -- }
  begin
    *str C@ 0<>
  while
    *str C@ emit
    *str 1+ to *str
  repeat
;

\ ---[ zstring ]-----------------------------------------------------
\ Most SDL and OpenGL functions that require string data also require
\ that those strings be in a zero-delimited format. The Forth method
\ of passing strings is to pass the address followed by the length of
\ the string to the stack.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ To convert a Forth string to a zero-delimited string requires that
\ we add a character to the Forth string (at the end) which is then
\ passed to the zstring function, which changes that character to a
\ NULL (0) value, and drops the length parameter from the stack.
\ I add the character "0" to the end, since it signifies visually the
\ additional character that is to be converted.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ Example:
\       s" data/my_funky_picture.bmp" Load-Picture
\   becomes
\       s" data/my_funky_picture.bmp0" zstring Load-Picture
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

: zstring ( *str len -- *str ) over + 1- 0 swap C! ; 

\ ---[ SF, ]---------------------------------------------------------
\ Allocate and store a short float - 4 bytes - to the dictionary.
\ Suggested by Anton Ertl 06/03/2010 - Thanks Anton!
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ <here>      returns the address of the next free dictionary byte
\ <1 sfloats> calculates the size of an sfloat variable - 4 bytes
\ <allot>     allocates space at the next free dictionary address
\ <SF!>       stores the floating point value at the address <here>,
\             which is already on the stack.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

: SF, ( r -- ) here 1 sfloats allot SF! ;

\ ===[ Load Graphics Framework ]=====================================
\ This loads the opengllib-1.xx.fs file, which contains all of the
\ OpenGL scene generation code functions.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ -- Loads the OpenGL and SDL libcc interface dependancy files
\ -- Loads the high-level OpenGL code
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

require opengllib-1.21.fs

\ ===[ gforth BaseCode ]=============================================
\ The following functions are "common" to all of the tutorials, and
\ comprise the basic framework for all of the tutorials in the series

0 value videoflags             \ pointer to video hardware info array
0 value isActive                    \ "focus" indicator for the mouse
0 value VideoInfo                        \ pointer to video mode data

\ Create an event structure for accessing the SDL Event subsystems
create event here sdl-event% nip dup allot 0 fill

\ Initialize the SDL Video subsystem
: Initialize-SDL ( -- )
  SDL_INIT_EVERYTHING sdl-init 0< if
    cr ." Video Initialization failed: "
    sdl-geterror zprint cr
    bye
  then
;

\ Load information about the video hardware in the computer
: Get-Video-Info ( -- )
  sdl-getvideoinfo dup to VideoInfo 0= if
    cr ." Video query failed: " sdl-geterror zprint cr
    bye
  then
;

\ Build a flag variable specifying the video characteristics to set
: Compile-Video-Flags ( -- )
  SDL_OPENGL                                   \ enable OpenGL in SDL
  SDL_GL_DOUBLEBUFFER OR                    \ Enable double buffering
  SDL_HWPALETTE OR                    \ Store the palette in hardware
  SDL_RESIZABLE OR                           \ Enable window resizing
  to videoflags                                      \ save the flags
;

\ Add flag for if hardware surfaces can be created
: Check-HW-Surfaces ( -- )
  VideoInfo sdl-video-info-hw-available @ 0<> if
    SDL_HWSURFACE
  else
    SDL_SWSURFACE
  then
  videoflags OR to videoflags
;

\ Add flag for if hardware-to-hardware blits is available
: Check-HW-Blits ( -- )
  VideoInfo sdl-video-info-blit-hw @ 0<> if
    videoflags SDL_HWACCEL OR to videoflags
  then
;

\ Enable double buffering
: Init-Double-Buffering ( -- )
  SDL_GL_DOUBLEBUFFER 1 sdl-gl-set-attribute
;

\ Create an SDL surface and open the display window
: Init-Video ( -- )
  screen-width screen-height screen-bpp videoflags sdl-set-video-mode
  dup 0= if                            \ verify a surface was created
    drop                              \ window not created, error out
    cr ." Video mode set failed: " sdl-geterror zprint cr
    bye                                     \ exit to terminal window
  then
  to screen                                 \ save pointer to surface
;

\ Put a title onto the window caption bar
: Init-Caption ( -- )
  s" gforth/OpenGL: NeHe Lesson 210" zstring NULL sdl-wm-set-caption
;

\ Enable the keyboard repeat functionality
: Init-Keyboard ( -- )
  100 SDL_DEFAULT_REPEAT_INTERVAL sdl-enable-keyrepeat if
    sdl-quit
    cr ." Setting keyboard repeat failed: " sdl-geterror zprint cr
    bye
  then
;

\ Attempt to initialize the audio sub-system
: Init-Audio ( -- )
  SDL_INIT_AUDIO sdl-init-sub-system if         \ returns -1 on error
    cr ." Could not initialize audio subsystem: "
    sdl-geterror zprint cr
    sdl-quit
    bye
  then
  22060 AUDIO_S16SYS 2 512 mix-open-audio if    \ returns -1 on error
    cr ." Unable to open audio: "
    sdl-geterror zprint cr
    sdl-quit
    bye
  then
  \ Audio initialized, so load the music
  s" data/lktheme.mod0" zstring mix-load-mus to Music
;

\ Reset our viewport after a window resize 
: ResizeWindow { _width _height -- boolean }
  _height 0= if                         \ protect from divide by zero
    1 to _height
  then
  \ set up the viewport
  0 0 _width _height gl-viewport
  \ Change to the projection matrix and set our viewing volume
  GL_PROJECTION gl-matrix-mode
  \ Reset the matrix
  gl-load-identity
  \ Set our ortho view
  0e _width S>F _height S>F 0e -1e 1e gl-ortho
  \ Make sure we are changing the model view and not the projection
  GL_MODELVIEW gl-matrix-mode
  \ Reset the matrix
  gl-load-identity
  \ Return a good value
  TRUE
;

\ Display keyboard/mouse help information
: Help-Msg ( -- )
  page
  0                                                 \ init line count
     ." Keyboard Functions:"                            1+
  cr ."     ESC   - exits program"                      1+
  cr ."     F1    - toggles fullscreen/windowed screen" 1+
  cr ."     a     - toggles line anti-aliasing"         1+
  cr ."     SPACE - starts the game"                    1+
  cr ."   Arrow Keys:"                                  1+
  cr ."     Left  - move the player left"               1+
  cr ."     Right - move the player right"              1+
  cr ."     Up    - move the player up"                 1+
  cr ."     Down  - move the player down"               1+
  cr                                                    1+
  cr ." Mouse Functions:"                               1+
  cr ."     Move off window to pause demo"              1+
  cr ."     Move onto window to resume demo"            1+
  cr ."     Resize window by dragging frame"            1+
  cr ."     Minimize/Maximize/Exit with window buttons" 1+
  cr                                                    1+
  to fps-line                           \ where to display the fps at
;

\ Initialize the game variables, for each time the game is executed
: Init-Game-Vars ( -- )
  0 to enemy-delay
  3 to speed-adjust
  5 to player-lives
  1 to internal-level
  1 to displayed-level
  1 to game-stage
  hline[] 11 cells 11 * 0 fill
  vline[] 11 cells 11 * 0 fill
  player object% nip 0 fill
  hourglass object% nip 0 fill
  enemies[] object% nip 9 * 0 fill
;

: lesson21 ( -- )
  Initialize-SDL                             \ Init the SDL subsystem
  Get-Video-Info                        \ Get the video info from SDL
  Compile-Video-Flags \ Compile the flags to pass to SDL_SetVideoMode  
  Check-HW-Surfaces       \ Check if surfaces can be stored in memory
  Check-HW-Blits                \ Check if hardware blits can be done
  Init-Double-Buffering              \ Set up OpenGL double buffering
  Init-Video                        \ create SDL surface; open window
  Init-Caption                         \ set the window title caption  
  Init-Keyboard                     \ enable key repeat functionality
  InitGL FALSE = if                               \ initialize OpenGL
    sdl-quit                          \ on error, close down and exit
    cr ." Could not initialize OpenGL." cr
    bye
  then

  Sound if                          \ initialize audio if Sound==TRUE
    Init-Audio                    \ Will exit to terminal on an error
  then
  
  screen-width screen-height ResizeWindow drop    \ resize the window

  Sound if
    Music -1 mix-play-music                 \ start playing the music
  then
  
  Init-Game-Vars                      \ initialize the game variables
  ResetObjects                                    \ reset our objects
  Reset-FPS-Counter       \ initialize the counter with sdl-get-ticks
  Help-Msg                       \ display the keyboard function info
  
  begin                                             \ wait for events 
    opengl-exit-flag 0=             \ repeat until this flag set TRUE
  while
    sdl-get-ticks to game-time                         \ get our time
    begin
      event sdl-pollevent             \ are there any pending events?
    while
      event sdl-event-type c@               \ yes, process the events
      case
        SDL_ACTIVEEVENT of    \ application visibility event occurred
            event sdl-active-event-gain C@ if
              TRUE              \ gained focus - draw in window again
            else
              FALSE             \ lost focus - stop drawing in window
            then
            to isActive
          endof

        SDL_VIDEORESIZE of             \ window resize event occurred
            event sdl-resize-event-width @            \ get new width
            event sdl-resize-event-height @          \ get new height
            screen-bpp                              \ use current bpp
            videoflags                                    \ get flags
            sdl-set-video-mode       \ attempt to create a new window
            dup 0= if                   \ error out if not successful
              drop
              sdl-quit
              cr ." Could not get a surface after resize: "
              sdl-geterror zprint cr
              bye
            then
            to screen                     \ success! save new pointer
            event sdl-resize-event-width @
            event sdl-resize-event-height @
            ResizeWindow drop             \ calculate new perspective
          endof

        SDL_KEYDOWN of                     \ key press event occurred
            event HandleKeyPress
          endof

        SDL_KEYUP of                     \ key release event occurred
            event HandleKeyRelease
          endof

        SDL_QUIT of     \ window close box clicked, or ALT-F4 pressed
            TRUE to opengl-exit-flag
          endof
      endcase
    repeat                    \ until no more events are in the queue
        
    \ Waste some cycles
    begin
      sdl-get-ticks game-time speed-adjust steps-ndx @ 2* + >
    until
    
    isActive if              \ if we have mouse focus, draw the scene
      DrawGLScene drop
    then
        
    gameover 0= isActive AND if
      game-stage internal-level * 0 do                        \ loop1
        \ Move the enemy right
        i enemies-ndx .x @ player .x @ <
        i enemies-ndx .fy @ i enemies-ndx .y @ 40 * = AND if
          1 i enemies-ndx .x +!
        then
        \ Move the enemy left
        i enemies-ndx .x @ player .x @ >
        i enemies-ndx .fy @ i enemies-ndx .y @ 40 * = AND if
          -1 i enemies-ndx .x +!
        then
        \ Move the enemy down
        i enemies-ndx .y @ player .y @ <
        i enemies-ndx .fx @ i enemies-ndx .x @ 60 * = AND if
          1 i enemies-ndx .y +!
        then
        \ Move the enemy up
        i enemies-ndx .y @ player .y @ >
        i enemies-ndx .fx @ i enemies-ndx .x @ 60 * = AND if
          -1 i enemies-ndx .y +!
        then
        \ Should the enemies move?
        enemy-delay 3 internal-level - >
        hourglass .fx @ 2 <> AND if
          0 to enemy-delay                       \ reset counter to 0
          game-stage internal-level * 0 do                    \ loop2
            i enemies-ndx .spin F@        \ put spin fvalue on fstack
            \ Is fine pos on x axis lower than intended pos?
            i enemies-ndx .fx @ i enemies-ndx .x @ 60 * < if
              \ Increase fine pos on x axis
              speed-adjust steps-ndx @ i enemies-ndx .fx +!
              \ Spin enemy clockwise
              speed-adjust steps-ndx @ S>F F+           \ add to spin
              \ -- keeping spin value on fstack here
            then
            \ Is fine pos on x axis higher than intended pos?
            i enemies-ndx .fx @ i enemies-ndx .x @ 60 * > if
              \ Decrease fine pos on x axis
              speed-adjust steps-ndx @ negate i enemies-ndx .fx +!
              \ Spin enemy counter-clockwise
              speed-adjust steps-ndx @ S>F F-    \ subtract from spin
              \ -- keeping spin value on fstack here
            then
            \ Is fine pos on y axis lower than intended pos?
            i enemies-ndx .fy @ i enemies-ndx .y @ 40 * < if
              \ Increase fine pos on y axis
              speed-adjust steps-ndx @ i enemies-ndx .fy +!
              \ Spin enemy clockwise
              speed-adjust steps-ndx @ S>F F+           \ add to spin
              \ -- keeping spin value on fstack here
            then
            \ Is fine pos on y axis higher than intended pos?
            i enemies-ndx .fy @ i enemies-ndx .y @ 40 * > if
              \ Decrease fine pos on y axis
              speed-adjust steps-ndx @ negate i enemies-ndx .fy +!
              \ Spin enemy clockwise
              speed-adjust steps-ndx @ S>F F-    \ subtract from spin
              \ -- keeping spin value on fstack here
            then
            i enemies-ndx .spin F!           \ save final spin fvalue
          loop
        then
        
        \ Are any of the enemies on top of the player?
        i enemies-ndx .fx @ player .fx @ =
        i enemies-ndx .fy @ player .fy @ = AND if
          player-lives 1- to player-lives       \ Player loses a life
          
          player-lives 0= if                   \ Are we out of lives?
            TRUE to gameover
          then
        
          Sound if
            s" data/die.wav0" 0 PlaySound       \ play the death sound
          then
          ResetObjects
        then
      loop
      
      \ Move the player
      \ Is fine pos on x axis lower than intended pos?
      player .fx @ player .x @ 60 * < if
        \ Increase the fine x position
        speed-adjust steps-ndx @ player .fx +!
      then
      
      \ Is fine pos on x axis greater than intended pos?
      player .fx @ player .x @ 60 * > if
        \ Decrease the fine x position
        speed-adjust steps-ndx @ negate player .fx +!
      then
      
      \ Is fine pos on y axis lower than intended pos?
      player .fy @ player .y @ 40 * < if
        \ Increase the fine x position
        speed-adjust steps-ndx @ player .fy +!
      then
      
      \ Is fine pos on y axis greater than intended pos?
      player .fy @ player .y @ 40 * > if
        \ Decrease the fine x position
        speed-adjust steps-ndx @ negate player .fy +!
      then
    then      

    \ Is the grid filled in?
    grid-filled if
      Sound if
        \ Play the level complete sound
        s" data/complete.wav0" 0 PlaySound
      then
      \ Increase the stage
      game-stage 1+ to game-stage
      \ Is the stage higher than 3?
      game-stage 3 > if
        1 to game-stage                     \ yes, reset stage to one
        internal-level 1+ to internal-level         \ increase levels
        displayed-level 1+ to displayed-level
        \ Is the level greater than 3?
        internal-level 3 > if
          3 to internal-level                      \ clamp to level 3
          \ Give a free life - but limit to 5 lives max
          player-lives 1+ 5 MIN to player-lives
        then
      then
      \ Reset the player/enemy positions
      ResetObjects
        
      \ Clear the grid x and grid y coordinate arrays
      hline[] 11 cells 11 * 0 fill
      vline[] 11 cells 11 * 0 fill
    then \ grid-filled if
      
    \ If the player hits the hourglass while it is displayed:
    player .fx @ hourglass .x @ 60 * =
    player .fy @ hourglass .y @ 40 * = AND
    hourglass .fx @ 1 = AND if
      Sound if
        \ Play freeze enemy sound
        s" data/freeze.wav0" -1 PlaySound
      then
      \ Set hourglass fx variable to Two
      2 hourglass .fx !
      \ Set Hourglass fy variable to Zero
      0 hourglass .fy !
    then
      
    \ Spin the player clockwise
    player .spin F@
    0.5e speed-adjust steps-ndx @ S>F F* F+
    \ Is the spin fvalue >360?
    FDUP 360e F> if 360e F- then
    player .spin F!
      
    \ Spin the hourglass counter-clockwise
    hourglass .spin F@
    0.25e speed-adjust steps-ndx @ S>F F* F-
    \ Is the spin value <0?
    FDUP 0e F< if 360e F+ then
    hourglass .spin F!
      
    \ Increment hourglass .fy
    speed-adjust steps-ndx @ hourglass .fy +!
      
    \ Make the hourglass appear if hourglass .fx==0, 
    \ and .fy>(6000/internal-level)
      
    hourglass .fx @ 0=
    hourglass .fy @ 6000 internal-level / > AND if
      Sound if
        \ Play the hourglass appears sound
        s" data/hourglass.wav0" 0 PlaySound
      then
      \ Give the hourglass random .x/.y values; init .fx/.fy
      10 random 1+ hourglass .x !
      11 random hourglass .y !
      1 hourglass .fx !                       \ hourglass will appear
      0 hourglass .fy !                               \ reset counter
    then

    \ Make the hourglass disappear if the hourglass .fx==1, and
    \ the hourglass .fy > (6000/internal-level)
    hourglass .fx @ 1 =
    hourglass .fy @ 6000 internal-level / > AND if
      0 hourglass .fx !                    \ hourglass will disappear
      0 hourglass .fy !                               \ reset counter
    then

    \ Unfreeze the enemies if hourglass .fx = 2, and
    \ hourglass .fy > (500+(500*internal-level))
    hourglass .fx @ 2 =
    hourglass .fy @ 500 500 internal-level * + > AND if
      Sound if
        \ Kill the freeze sound
        0 0 0 PlaySound
      then
      0 hourglass .fx !                        \ unfreeze the enemies
      0 hourglass .fy !                               \ reset counter
    then
    enemy-delay 1+ to enemy-delay
  repeat                      \ until opengl-exit-flag is set to TRUE

  Shutdown                    \ close down the SDL systems gracefully
  TRUE to isActive                       \ reset for next program run
;

\ Display the amount of dictionary space used during compilation
base-user-memory unused -
cr .( Compilation Size: ) . .( bytes)
\ Display the time taken during compilation
utime compilation-start-time d- 1 1000 m*/
cr .( Compilation Time: ) d. .( msec) cr

cr .( gforth/OpenGL: NeHe Lesson 21)
cr .( type "lesson21" to execute) cr

