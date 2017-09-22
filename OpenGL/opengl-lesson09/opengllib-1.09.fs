\ ===================================================================
\           File: opengllib-1.09.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/25/2010
\    Description: Moving bitmaps in 3D space
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 09
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

require mini-opengl-1.09.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

require random.fs

\ ---[ Prototype Listing ]-------------------------------------------
\ : Generate-Texture            { *src -- }
\ : LoadGLTextures              ( -- boolean )
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

\ Define the star structure

struct
  cell%  field .red                                     \ stars color
  cell%  field .green
  cell%  field .blue
  float% field .dist                     \ stars distance from center
  float% field .angle                           \ stars current angle
end-struct star%

1 constant NumTextures                    \ number of textures to use
50 constant #Stars                                  \ number of stars


\ allocate space for the array of stars
create Stars[] here star% nip #Stars * dup allot 0 fill

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;
: stars-ndx ( n -- *stars[n] ) star% nip * Stars[] + ;

FALSE value twinkle                                        \ do they?

fvariable zoom                     \ viewing distance away from stars
fvariable tilt                                        \ tilt the view
fvariable spin                               \ for spinning the stars

\ ---[ Variable Initializations ]------------------------------------

-15e zoom F!
90e  tilt F!
0e   spin F!

\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as a GL texture

: Generate-Texture { *src -- }
  GL_TEXTURE_2D 0 3
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0 GL_BGR                                \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  gl-tex-image-2d                               \ finally generate it
;

: LoadGLTextures ( -- status )
  FALSE { _status -- status }                     \ init return value
  s" data/star.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value
    1 texture gl-gen-textures                    \ create the texture

    \ Load in texture
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    \ Generate the texture
    R@ Generate-Texture

    \ Free the image, as we are done with it
    R> sdl-freesurface
  else
    drop                                       \ unable to load image
    cr ." Error: texture image could not be loaded!" cr
  then
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.
\
\ PgUp/PgDn and the Arrow keys will be allowed to be constantly read
\ until they are released.

0 value key-ESC
0 value key-F1
0 value key-PgUp
0 value key-PgDn
0 value key-t
0 value key-Up
0 value key-Dn

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   t        toggles twinkling of stars
\   PageUp   zooms into the scene
\   PageDown zooms out of the scene
\   Up       changes tilt of the stars more positive
\   Down     changes tilt of the stars more negative

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_t        of key-t FALSE = if       \ skip if being held down
                       twinkle 0= if 1 else 0 then to twinkle
                       TRUE to key-t           \ set key pressed flag
                     then
                  endof
    SDLK_PAGEUP   of zoom F@ 0.2e F+ zoom F! endof
    SDLK_PAGEDOWN of zoom F@ 0.2e F- zoom F! endof
    SDLK_UP       of tilt F@ 0.5e F- tilt F! endof
    SDLK_DOWN     of tilt F@ 0.5e F+ tilt F! endof
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
    SDLK_PAGEUP   of FALSE to key-PgUp  endof
    SDLK_PAGEDOWN of FALSE to key-PgDn  endof
    SDLK_t        of FALSE to key-t     endof
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    false
  else
    \ Enable texture mapping
    GL_TEXTURE_2D gl-enable
    \ Enable smooth shading
    GL_SMOOTH gl-shade-model
    \ Set the background black
    0e 0e 0e 0e gl-clear-color
    \ Depth buffer setup
    1e gl-clear-depth
    \ Type of depth test to do
    GL_LEQUAL gl-depth-func
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
    \ Blending translucency based on source alpha value
    GL_SRC_ALPHA GL_ONE gl-blend-func
    \ Enable blending
    GL_BLEND gl-enable
    \ Loop thru all of the stars
    #Stars 0 do
      \ Start all the stars at angle zero
      0e i stars-ndx .angle F!
      \ calculate distance from the center
      i S>F #Stars S>F F/ 5e F* i stars-ndx .dist F!
      \ Set .red to a random intensity
      rnd 256 MOD i stars-ndx .red !
      \ Set .green to a random intensity
      rnd 256 MOD i stars-ndx .green !
      \ Set .blue to a random intensity
      rnd 256 MOD i stars-ndx .blue !
    loop
    \ Return a good value
    TRUE
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag               \ reset these for next time
  -15e zoom F!
  90e  tilt F!
  0e   spin F!
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
  \ Select our texture
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
  \ restore matrix
  gl-load-identity

  #Stars 0 do
    \ Reset the view before we draw each star
    gl-load-identity
    \ Zoom into the screen
    0e 0e zoom F@ gl-translate-f
    \ Tilt the view
    tilt F@ 1e 0e 0e gl-rotate-f
    \ Rotate to the current stars angle
    i stars-ndx .angle F@ 0e 1e 0e gl-rotate-f
    \ Move forward on the x plane
    i stars-ndx .dist F@ 0e 0e gl-translate-f
    \ Cancel the current stars angle
    i stars-ndx .angle F@ fnegate 0e 1e 0e gl-rotate-f
    \ Cancel the screen tilt
    tilt F@ fnegate 1e 0e 0e gl-rotate-f
    \ Twinkling stars enabled
    twinkle if
      \ Assign a color
      #Stars i - 1- stars-ndx .red   @
      #Stars i - 1- stars-ndx .green @
      #Stars i - 1- stars-ndx .blue  @ 255 gl-color-4ub
      \ Begin drawing the textured quad
      GL_QUADS gl-begin
        0e 0e gl-tex-coord-2f -1e -1e 0e gl-vertex-3f
        1e 0e gl-tex-coord-2f  1e -1e 0e gl-vertex-3f
        1e 1e gl-tex-coord-2f  1e  1e 0e gl-vertex-3f
        0e 1e gl-tex-coord-2f -1e  1e 0e gl-vertex-3f
      gl-end
    then
    \ Rotate the star on the z axis
    spin F@ 0e 0e 1e gl-rotate-f
    \ Assign a color
    i stars-ndx .red   @
    i stars-ndx .green @
    i stars-ndx .blue  @ 255 gl-color-4ub
    \ Begin drawing the textured quad
    GL_QUADS gl-begin
      0e 0e gl-tex-coord-2f -1e -1e 0e gl-vertex-3f
      1e 0e gl-tex-coord-2f  1e -1e 0e gl-vertex-3f
      1e 1e gl-tex-coord-2f  1e  1e 0e gl-vertex-3f
      0e 1e gl-tex-coord-2f -1e  1e 0e gl-vertex-3f
    gl-end
    \ Used to spin the stars
    spin F@ 0.01e F+ spin F!
    \ Change the angle of a star
    i stars-ndx .angle F@ i S>F #Stars S>F F/
    F+ i stars-ndx .angle F!
    \ Change the distance of a star
    i stars-ndx .dist F@ 0.01e F- i stars-ndx .dist F!
    \ Is the star in the middle yet?
    i stars-ndx .dist F@ F0< if
      \ Move the star 5 units from the center
      i stars-ndx .dist F@ 5e F+ i stars-ndx .dist F!
      \ Give it a new red value
      rnd 256 MOD i stars-ndx .red !
      \ Give it a new green value
      rnd 256 MOD i stars-ndx .green !
      \ Give it a new blue value
      rnd 256 MOD i stars-ndx .blue !
    then
  loop

  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  TRUE                                          \ Return a good value
;

