\ ===================================================================
\           File: opengl-lesson24.fs
\         Author: Jeff Molofee
\  Linux Version: DarkAlloy
\ gForth Version: Timothy Trussell, 05/01/2011
\    Description: Tokens, extensions, scissor testing, TGA
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.32-31
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 24
\ ===================================================================
\ This code was created by Jeff Molofee '00
\ (ported to Linux/SDL by Dark Alloy)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ Marker ]------------------------------------------------------
\ Allows for easy removal of a previous loading of the program, for
\ re-compilation of changes.
\ If ---marker--- exists, execute it to restore the dictionary.

[IFDEF] ---marker---
  ---marker---
[ENDIF]

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
32  constant screen-bpp

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
\ Zero-delimits a Forth-type counted string; requires extra character
\ at end of string to be changed to a 0 byte value.

: zstring ( *str len -- *str ) over + 1- 0 swap C! ; 

\ ---[ SF, ]---------------------------------------------------------
\ Allocate and store a short float - 4 bytes - to the dictionary.
\ Suggested by Anton Ertl 06/03/2010 - Thanks Anton!

: SF, ( r -- ) here 1 sfloats allot SF! ;

\ ===[ Load Graphics Framework ]=====================================
\ This loads the opengllib-1.xx.fs file, which contains all of the
\ OpenGL scene generation code functions.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ -- Loads the OpenGL and SDL libcc interface dependancy files
\ -- Loads the high-level OpenGL code
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

require opengllib-1.24.fs

\ ===[ gforth BaseCode ]=============================================
\ The following functions are "common" to all of the tutorials, and
\ comprise the basic framework for all of the tutorials in the series

0 value videoflags             \ pointer to video hardware info array
1 value isActive                    \ "focus" indicator for the mouse
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
  s" gforth/OpenGL: NeHe Lesson 240" zstring NULL sdl-wm-set-caption
;

\ Enable the keyboard repeat functionality
: Init-Keyboard ( -- )
  100 SDL_DEFAULT_REPEAT_INTERVAL sdl-enable-keyrepeat if
    sdl-quit
    cr ." Setting keyboard repeat failed: " sdl-geterror zprint cr
    bye
  then
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
\  0e _width S>F 0e _height S>F -1e 1e gl-ortho
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
  cr ."   Arrow Keys:"                                  1+
  cr ."     Up    - scrolls the extension list up"      1+
  cr ."     Down  - scrolls the extension list down"    1+
  cr                                                    1+
  cr ." Mouse Functions:"                               1+
  cr ."     Move off window to pause demo"              1+
  cr ."     Move onto window to resume demo"            1+
  cr ."     Resize window by dragging frame"            1+
  cr ."     Minimize/Maximize/Exit with window buttons" 1+
  cr                                                    1+
  to fps-line                           \ where to display the fps at
;

: lesson24 ( -- )
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

  screen-width screen-height ResizeWindow drop    \ resize the window
  
  screen-width to swidth
  screen-height to sheight
  
  Reset-FPS-Counter       \ initialize the counter with sdl-get-ticks
  Help-Msg                       \ display the keyboard function info
  
  DrawGLScene drop                 \ initial call to display the data

  begin                                             \ wait for events 
    opengl-exit-flag 0=             \ repeat until this flag set TRUE
  while
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
            screen sdl-surface-w @ to swidth
            screen sdl-surface-h @ to sheight
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
        
    scroller case
      -1 of scroll 0> if 
              scroll 2 - to scroll
            then
         endof
       1 of scroll 32 maxtokens 9 - * < if
              scroll 2 + to scroll
            then
         endof
    endcase

    scroller -1 = if
      scroll 0> if
        scroll 2 - to scroll
      then
    then

    scroller 1 = if
      scroll 32 maxtokens 9 - * < if
        scroll 2 + to scroll
      then
    then

    isActive if              \ if we have mouse focus, draw the scene
      DrawGLScene drop
    then
\    10 sdl-delay                        \ delay to free some CPU time
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

cr .( gforth/OpenGL: NeHe Lesson 24)
cr .( type "lesson24" to execute) cr

