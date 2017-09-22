\ ===================================================================
\           File: opengllib-1.11.fs
\         Author: Bosco
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/25/2010
\    Description: Flag effect (waving texture)
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 11
\ ===================================================================
\ This code was created by Bosco
\ ported to Linux/SDL by Ti Leggett
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

require mini-opengl-1.11.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

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

1 constant NumTextures                    \ number of textures to use

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

PI 2e F* fconstant 2PI                                  \ common calc

fvariable x-rot
fvariable y-rot
fvariable z-rot

0 value wiggle-count                    \ how fast the flag waves

\ Need to create a [45][45][3] fp array here
\ These can be fixed as 8-byte fp values, as they are sent to the
\ OpenGL system via the gl-vertex-3f call, not by passing the address
\ of the array.

\ One way of thinking of this is as (3) (45x45) arrays
\ So, x and y have a range of [0..44], and z has a range of [0..2]

\       +-------+               |---x ---|
\       |       |-+             +--------+ -+-
\       |       | |-+           |        |  |
\       |    z=0| | |           |        |  y
\       +-------+1| |           |        |  |
\         +-------+2|           +--------+ -+-
\           +-------+

create points[] here 45 floats 45 * 3 * dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

: points-ndx { _x _y _z -- *points[x][y][z] }
  points[]                      \ *points[] 
  \ calculate row of the y coordinate
  _y 45 floats *                \ *points[] yofs
  \ calculate column of the x coordinate
  _x floats +                   \ *points[] yofs+xofs
  \ calculate page of the z coordinate
  _z 45 floats 45 * * +         \ *points[] yofs+xofs+zofs
  +                             \ *points[yofs+xofs+zofs]
;

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
  s" data/tim.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value

    1 texture gl-gen-textures                    \ create the texture

    \ texture generation using data from the bitmap
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture

    \ Generate the texture
    R@ Generate-Texture

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    \ free the image now we are done with it
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
  \ Load in the texture
  LoadGLTextures 0= if
    false
  else
    \ Enable texture mapping
    GL_TEXTURE_2D gl-enable
    \ Enable smooth shading
    GL_SMOOTH gl-shade-model
    \ Set the background black
    0e 0e 0e 0.5e gl-clear-color
    \ Depth buffer setup
    1e gl-clear-depth
    \ Enable depth testing
    GL_DEPTH_TEST gl-enable
    \ Type of depth test to do
    GL_LEQUAL gl-depth-func
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
    \ Fill the back with texture; the front will only be wireline
    GL_BACK GL_FILL gl-polygon-mode
    GL_FRONT GL_LINE gl-polygon-mode
    \ Apply the wave to our mesh array
    45 0 do
      45 0 do
        j S>F 5e F/ 4.5e F-                    j i 0 points-ndx F!
        i S>F 5e F/ 4.5e F-                    j i 1 points-ndx F!
        j S>F 5e F/ 40e F* 360e F/ 2PI F* FSIN j i 2 points-ndx F!
      loop
    loop
    \ Return a good value
    TRUE
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
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

fvariable f-x                 \ use to break the flag into tiny quads
fvariable f-y
fvariable f-xb
fvariable f-yb

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  gl-load-identity                                   \ restore matrix
  \ Translate 17 units into the screen
  0e 0e -17e gl-translate-f
  \ Rotate on the x/y/z axes
  x-rot F@ 1e 0e 0e gl-rotate-f
  y-rot F@ 0e 1e 0e gl-rotate-f
  z-rot F@ 0e 0e 1e gl-rotate-f
  \ Select our texture
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
  \ Start drawing our quads
  GL_QUADS gl-begin
    44 0 do
      44 0 do
        j S>F 44e F/ f-x F!                     \ Create a fp x value
        i S>F 44e F/ f-y F!                     \ Create a fp y value
        j 1+ S>F 44e F/ f-xb F!              \ Create x+0.0227e value
        i 1+ S>F 44e F/ f-yb F!              \ Create y+0.0227e value
        \ Bottom Left texture coordinate
        f-x F@ f-y F@ gl-tex-coord-2f
        j i 0 points-ndx F@
        j i 1 points-ndx F@
        j i 2 points-ndx F@ gl-vertex-3f
        \ Top left texture coordinate
        f-x F@ f-yb F@ gl-tex-coord-2f
        j i 1+ 0 points-ndx F@
        j i 1+ 1 points-ndx F@
        j i 1+ 2 points-ndx F@ gl-vertex-3f
        \ Top Right texture coordinate
        f-xb F@ f-yb F@ gl-tex-coord-2f
        j 1+ i 1+ 0 points-ndx F@
        j 1+ i 1+ 1 points-ndx F@
        j 1+ i 1+ 2 points-ndx F@ gl-vertex-3f
        \ Bottom Right texture coordinate
        f-xb F@ f-y F@ gl-tex-coord-2f
        j 1+ i 0 points-ndx F@
        j 1+ i 1 points-ndx F@
        j 1+ i 2 points-ndx F@ gl-vertex-3f
      loop
    loop
  gl-end

  \ Used to slow down the wave (every 2nd frame only)
  wiggle-count 1 > if
    45 0 do
      0 i 2 points-ndx F@
      44 0 do
        i 1+ j 2 points-ndx F@ i j 2 points-ndx F!
      loop
      44 i 2 points-ndx F!
    loop
    0 to wiggle-count                              \ set back to zero
  then
  wiggle-count 1+ to wiggle-count

  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  x-rot F@ 0.3e F+ x-rot F!                    \ increment x rotation  
  y-rot F@ 0.2e F+ y-rot F!                    \ increment y rotation  
  z-rot F@ 0.4e F+ z-rot F!                    \ increment z rotation  
  
  \ Return a good value
  TRUE
;

