\ ===================================================================
\           File: opengllib-1.13.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/31/2010
\    Description: Bitmap fonts
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 12
\ ===================================================================
\ This code was created by Jeff Molofee '99 
\ (ported to Linux/SDL by Ti Leggett '01)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ UseLibUtil ]--------------------------------------------------
\ Conditional compilation of the libcc interfaces:
\ -- Set to 1 if you use the LibUtil script to copy the files to
\    the ~/.gforth directory.
\ -- Set to 0 to use the files from the Lesson directory (PWD).

0 constant UseLibUtil

UseLibUtil [if]

require ~/.gforth/opengl-libs/mini-opengl-current.fs
require ~/.gforth/opengl-libs/mini-sdl-current.fs
require ~/.gforth/opengl-libs/sdlkeysym.fs

[else]

require mini-opengl-1.13.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : concat-string               { *str _len *dst -- }
\ : IntToStr                    ( n -- *str len )
\ : KillFont                    ( -- )
\ : BuildFont                   ( -- )
\ : glPrint                     ( *str _len -- )
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

variable baselist                \ base display list for the font set
fvariable count1     \ 1st counter used to move text and for coloring
fvariable count2     \ 2nd counter used to move text and for coloring

\ ---[ Variable Initializations ]------------------------------------

0 baselist !
0e count1 F!
0e count2 F!

255 constant string-len
create temp-string here string-len 1+ dup allot 0 fill

\ ---[ concat-string ]---
\ A basic string concatenation function, to add one string to another

: concat-string { *str _len *dst -- }
  *str *dst 1+ *dst C@ + _len cmove
  *dst C@ _len + *dst C!                \ use *dst[0] as length byte
;

\ ---[ IntToStr ]---
\ Converts an integer value to a string; returns addr/len

: IntToStr ( n -- *str len ) 0 <# #S #> ;

\ ===[ Back to our regularly scheduled code ]========================

\ ---[ KillFont ]----------------------------------------------------
\ Recover memory from our list of characters

: KillFont ( -- )
  baselist @ 96 gl-delete-lists
;  

\ ---[ BuildFont ]---------------------------------------------------
\ Builds our font lists

\ Accesses X11 library functions.
\ XOpenDisplay is referenced in glx.h->xlib.h as an extern definition

: BuildFont ( -- )
  0 0 0 { _dpy _finfo _status -- }
  96 gl-gen-lists baselist !
  \ Get our current display so we can get the fonts
  0 x-opendisplay to _dpy
  \ Get the font information
  _dpy s" -adobe-helvetica-medium-r-normal--18-*-*-*-p-*-iso8859-10"
  zstring x-loadqueryfont to _finfo
  _finfo 0= if
    cr ." First font selection failed.  Trying again." cr
    \ The helvetica font was not found - try for a fixed font
    _dpy s" fixed0" zstring x-loadqueryfont to _finfo
    _finfo 0= if
      cr ." Second font selection failed." cr
    else
      cr ." Using <fixed> font." cr
      TRUE to _status
    then
  else
    cr ." Using <helvetica> font." cr
    TRUE to _status
  then
  \ If _status==TRUE, we have a font, so continue
  _status if
    _finfo cell + @ 32 96 baselist @ gl-xusexfont
    \ Release the font, freeing the memory
    _dpy _finfo x-freefont drop
  then
  \ Close the X display now we are done with it
  _dpy x-closedisplay drop
  \ Return status code
  _status
;

\ ---[ glPrint ]-----------------------------------------------------
\ Print our GL text to the screen.

\ The passed string should have an extra character appended to the
\ end, so the string can be zero-delimited string by *this* function
\ - not by the calling function.  We want to have the string length
\ on the stack when glPrint is called.

: glPrint ( *str _len -- )
  \ skip if string length==0
  dup 0> if
    dup >R                                              \ save length
    zstring                        \ convert to zero-delimited string
    GL_LIST_BIT gl-push-attrib           \ push the display list bits
    baselist @ 32 - gl-list-base           \ Set base character to 32
    R> GL_UNSIGNED_BYTE rot gl-call-lists             \ Draw the text
    gl-pop-attrib                         \ Pop the display list bits
  else
    2DROP
  then
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 value key-ESC
0 value key-F1

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
  endcase
;

\ ---[ HandleKeyRelease ]--------------------------------------------
\ Function to handle key release events
\ I have added all of the key flags, even though not all are being
\ accessed in the HandleKeyPress function.

: HandleKeyRelease ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of FALSE to key-ESC   endof
    SDLK_F1       of FALSE to key-F1    endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Enable smooth shading
  GL_SMOOTH gl-shade-model
  \ Set the background black
  0e 0e 0e 0e gl-clear-color
  \ Depth buffer setup
  1e gl-clear-depth
  \ Enable depth testing
  GL_DEPTH_TEST gl-enable
  \ Type of depth test to do
  GL_LEQUAL gl-depth-func
  \ Really nice perspective calculations
  GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
  \ Return a good value
  BuildFont
  \ returns result from BuildFont
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  KillFont                                       \ clean up font list
  sdl-quit                               \ close down the SDL systems
;

fvariable fps-seconds
fvariable fps-count
0 value   fps-ticks
0 value   fps-t0
0 value   fps-frames
0 value   fps-line

\ ---[ Reset-FPS-Counter ]-------------------------------------------

: Reset-FPS-Counter ( -- )
  sdl-get-ticks to fps-t0
  0 to fps-frames
;

\ ---[ Display-FPS ]-------------------------------------------------

: Display-FPS ( -- )
  sdl-get-ticks to fps-ticks
  fps-ticks fps-t0 - 1000 >= if
    fps-ticks fps-t0 - S>F 1000e F/ fps-seconds F!
    fps-frames S>F fps-seconds F@ F/ fps-count F!
    0 fps-line at-xy 50 spaces           \ clear previous fps display
    0 fps-line at-xy                          \ display new fps count
    fps-frames . ." frames in " 
    fps-seconds F@ F>S . ." seconds = " 
    fps-count F@ F>S . ." FPS" cr
    fps-ticks to fps-t0
    0 to fps-frames
  then
;

\ ---[ DrawGLScene ]-------------------------------------------------
\ Here goes our drawing code 

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  gl-load-identity
  \ Move into the screen 1 unit
  0e 0e -1e gl-translate-f
  \ Pulsing colors based on text position
  1e count1 F@ FCOS F*
  1e count2 F@ FSIN F*
  1e 0.5e count1 F@ count2 F@ F+ FCOS F* F- gl-color-3f
  \ Position the text on the screen
  -0.45e 0.05e count1 F@ FCOS F* F+
  0.35e count2 F@ FSIN F*
  gl-raster-pos-2f
  \ Build the text string to display
  \ zero temp string length - where we will build our string at
  0 temp-string !
  \ Copy the main text to the temp string
  s" Active OpenGL Text With NeHe - " temp-string concat-string
  \ Convert the whole part of count1 to a string and concat it
  count1 F@ F>S IntToStr temp-string concat-string
  \ Add a character to allow for zero-delimiting the string
  s" 0" temp-string concat-string
  \ Print the text to the screen
  temp-string dup 1+ swap C@ glPrint

  count1 F@ 0.051e F+ count1 F!          \ increase the first counter
  count2 F@ 0.005e F+ count2 F!         \ increase the second counter
  
  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  \ Return a good value
  TRUE
;

