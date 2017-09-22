\ ===================================================================
\        Program: opengl-tut07.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/25/2010
\    Description: Filters, lighting and keyboard
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 07
\ ===================================================================
\ This code was created by Jeff Molofee '99 
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
unused constant  free-dictionary-memory
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
\ : Help-Msg                    ( -- )
\ : lesson07                    ( -- )
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

require opengllib-1.07.fs

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
  s" gforth/OpenGL: NeHe Lesson 70" zstring NULL sdl-wm-set-caption
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
  \ Set our perspective - the F/ calcs the aspect ratio of w/h
  45e _width S>F _height S>F F/ 0.1e 100e glu-perspective
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
  cr ."     ESC - exits program"                        1+
  cr ."     F1  - toggles fullscreen/windowed screen"   1+
  cr ."     PageUp   zooms into the scene"              1+
  cr ."     PageDown zooms out of the scene"            1+
  cr ."     f        pages thru the different filters"  1+
  cr ."     l        toggles the light on/off"          1+
  cr ."   Arrow Keys:"                                  1+
  cr ."     Up       makes x rotation more negative"    1+
  cr ."     Down     makes x rotation more positive"    1+
  cr ."     Left     makes y rotation more negative"    1+
  cr ."     Right    makes y rotation more positive"    1+
  cr                                                    1+
  cr ." Mouse Functions:"                               1+
  cr ."     Move off window to pause demo"              1+
  cr ."     Move onto window to resume demo"            1+
  cr ."     Resize window by dragging frame"            1+
  cr ."     Minimize/Maximize/Exit with window buttons" 1+
  cr                                                    1+
  to fps-line                           \ where to display the fps at
;

: lesson07 ( -- )
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
  Reset-FPS-Counter       \ initialize the counter with sdl-get-ticks
  Help-Msg                       \ display the keyboard function info
  
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

    isActive if              \ if we have mouse focus, draw the scene
      DrawGLScene drop
    then
\   2 sdl-delay           \ delay to allow the demo screen to be seen
  repeat                      \ until opengl-exit-flag is set to TRUE

  Shutdown                    \ close down the SDL systems gracefully
  TRUE to isActive                       \ reset for next program run
;

\ Display the amount of dictionary space used during compilation
free-dictionary-memory unused -
cr .( Compilation Size: ) . .( bytes)
\ Display the time taken during compilation
utime compilation-start-time d- 1 1000 m*/
cr .( Compilation Time: ) d. .( msec) cr

cr .( gforth/OpenGL: NeHe Lesson 7)
cr .( type "lesson07" to execute) cr

