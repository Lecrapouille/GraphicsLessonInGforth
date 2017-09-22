\ ===================================================================
\           File: opengllib-1.19.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 08/01/2010
\    Description: Particle Engine Using Triangle Strips
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 19
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

require mini-opengl-1.19.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

require random.fs

\ ---[ Prototype Listing ]-------------------------------------------
\ : Generate-Texture            { *src -- }
\ : LoadGLTextures              ( -- boolean )
\ : ResetParticle               { _num _color _xdir _ydir _zdir -- }
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

1000 constant MaxParticles              \ maximum number of particles

fvariable slowdown                              \ slow down particles
fvariable xspeed                                       \ base x speed
fvariable yspeed                                       \ base y speed
fvariable zoom                                     \ used to zoom out

TRUE value rainbow                            \ toggle rainbow effect
0 value rainbow-delay                          \ rainbow effect delay
0 value rainbow-color                                 \ rainbow color

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

\ ---[ Variable Initializations ]------------------------------------

2e slowdown F!
-40e zoom F!

struct
  cell%  field p-active                    \ active particle (yes/no)
  float% field p-life                                 \ particle life
  float% field p-fade                                    \ fade speed
  float% field p-r                                        \ red value
  float% field p-g                                      \ green value
  float% field p-b                                       \ blue value
  float% field p-x                                       \ x position
  float% field p-y                                       \ y position
  float% field p-z                                       \ z position
  float% field p-xi                                     \ x direction
  float% field p-yi                                     \ y direction
  float% field p-zi                                     \ z direction
  float% field p-xg                                       \ x gravity
  float% field p-yg                                       \ y gravity
  float% field p-zg                                       \ z gravity
end-struct particle%

\ Rainbow of colors

struct
  float% field c-red
  float% field c-green
  float% field c-blue
end-struct color%

create colors[]
1e    F, 0.5e  F, 0.5e  F,
1e    F, 0.75e F, 0.5e  F,
1e    F, 1e    F, 0.5e  F,
0.75e F, 1e    F, 0.5e  F,
0.5e  F, 1e    F, 0.5e  F,
0.5e  F, 1e    F, 0.75e F,
0.5e  F, 1e    F, 1e    F,
0.5e  F, 0.75e F, 1e    F,
0.5e  F, 0.5e  F, 1e    F,
0.75e F, 0.5e  F, 1e    F,
1e    F, 0.5e  F, 1e    F,
1e    F, 0.5e  F, 0.75e F,

\ Index function to access colors[] array
: color-ndx ( n -- *color[n] ) color% nip * colors[] + ;

\ Pointer to array of particle structs.
\ This is allocated at run-time when InitGL is executed.

0 value particle[]

\ Index function to access particle[] array
: particle-ndx ( n -- *particle[n] ) particle% nip * particle[] + ;

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
\ create local variable for storing return flag
  0 { _status -- status }
  \ Attempt to load the texture images into SDL surfaces
  s" data/particle.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                         \ save surface pointer
    cr ." Texture Image loaded" cr
    TRUE to _status                                \ set return value

    NumTextures texture gl-gen-textures         \ create the textures

  \ Load texture
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture        \ texture[0]

    \ Generate texture
    R@ Generate-Texture

    \ Nearest Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    R> sdl-freesurface
  else
    cr ." Error: texture image could not be loaded!" cr
  then
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ ResetParticle ]-----------------------------------------------
\ Resets a particle to its initial state

0 value rp-ptr
0 value rp-color

: ResetParticle { _num _color f: _xdir f: _ydir f: _zdir -- }
  \ Initialize the index pointers once
  _num particle-ndx to rp-ptr
  _color color-ndx to rp-color
  \ Reset all of the struct fields
  TRUE rp-ptr p-active !                   \ Make the particle active
  1e rp-ptr p-life F!                  \ Give the particle life, Igor
  100 random S>F 1000e F/ 0.003e F+ rp-ptr p-fade F!    \ Random fade 
  rp-color c-red   F@ rp-ptr p-r F!                         \ set red
  rp-color c-green F@ rp-ptr p-g F!                       \ set green
  rp-color c-blue  F@ rp-ptr p-b F!                        \ set blue
  0e    rp-ptr p-x  F!                             \ reset x position
  0e    rp-ptr p-y  F!                             \ reset y position
  0e    rp-ptr p-z  F!                             \ reset z position
  _xdir rp-ptr p-xi F!                       \ Random speed on x axis
  _ydir rp-ptr p-yi F!                       \ Random speed on y axis
  _zdir rp-ptr p-zi F!                       \ Random speed on z axis
  0e    rp-ptr p-xg F!                        \ reset horizontal pull
  -0.8e rp-ptr p-yg F!                  \ Set vertical pull downwards
  0e    rp-ptr p-zg F!                         \ reset pull on z axis
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 value key-ESC
0 value key-F1
0 value key-PgUp
0 value key-PgDn
0 value key-Tab
0 value key-Return
0 value key-Space
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left
0 value key-KP+
0 value key-KP-
0 value key-KP2
0 value key-KP4
0 value key-KP6
0 value key-KP8

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   PageUp   zooms into the scene
\   PageDown zooms out of the scene
\   TAB      resets all particles; makes them explode
\   RETURN   toggles the rainbow color effect
\   SPACE    turns off rainbowing; cycles thru the colors
\ Arrow Keys:
\   Up       increase particles y movement
\   Down     decrease particles y movement
\   Right    increase particles x movement
\   Left     decrease particles x movement
\ Numeric Keypad:
\   KP-Plus  speeds up the particles
\   KP-Minus slows down the particles
\   KP-8     increase particles y gravity
\   KP-2     decrease particles y gravity
\   KP-6     increase particles x gravity
\   KP-4     decrease particles x gravity

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_TAB      of key-Tab FALSE = if     \ skip if being held down
                       MaxParticles 0 do
                         i                                      \ num
                         i 1 + MaxParticles 12 / /            \ color
                         50 random S>F 26e F- 10e F*             \ xi
                         50 random S>F 25e F- 10e F*             \ yi
                         FDUP                                    \ zi
                         ResetParticle
                       loop
                       TRUE to key-Tab         \ set key pressed flag
                     then
                  endof
    SDLK_RETURN   of key-Return FALSE = if  \ skip if being held down
                       rainbow if FALSE else TRUE then to rainbow
                       25 to rainbow-delay
                       TRUE to key-Return      \ set key pressed flag
                     then
                  endof
    SDLK_SPACE    of key-Space FALSE = if   \ skip if being held down
                       FALSE to rainbow
                       0 to rainbow-delay
                       rainbow-color 1+ 12 MOD to rainbow-color                    
                       TRUE to key-Space       \ set key pressed flag
                     then
                  endof
    SDLK_PAGEUP   of zoom F@ 0.01e F+ zoom F! endof
    SDLK_PAGEDOWN of zoom F@ 0.01e F- zoom F! endof
    SDLK_UP       of yspeed F@ 200e F< if
                       yspeed F@ 1e F+ yspeed F!
                     then
                  endof
    SDLK_DOWN     of yspeed F@ -200e F> if
                       yspeed F@ 1e F- yspeed F!
                     then
                  endof
    SDLK_RIGHT    of xspeed F@ 200e F< if
                       xspeed F@ 1e F+ xspeed F!
                     then
                  endof
    SDLK_LEFT     of xspeed F@ -200e F> if
                       xspeed F@ 1e F- xspeed F!
                     then
                  endof
    SDLK_KP_PLUS  of slowdown F@ 1e F> if
                       slowdown F@ 0.01e F- slowdown F!
                      then
                  endof
    SDLK_KP_MINUS of slowdown F@ 4e F< if
                       slowdown F@ 0.01e F+ slowdown F!
                     then
                  endof
    SDLK_KP8      of MaxParticles 0 do
                       i particle-ndx p-yg F@ FDUP 1.5e F< if
                         0.01e F+ i particle-ndx p-yg F!
                       else
                         FDROP
                       then
                     loop
                  endof
    SDLK_KP2      of MaxParticles 0 do
                       i particle-ndx p-yg F@ FDUP -1.5e F> if
                         0.01e F- i particle-ndx p-yg F!
                       else
                         FDROP
                       then
                     loop
                  endof
    SDLK_KP6      of MaxParticles 0 do
                       i particle-ndx p-xg F@ FDUP 1.5e F< if
                         0.01e F+ i particle-ndx p-xg F!
                       else
                         FDROP
                       then
                     loop
                  endof
    SDLK_KP4      of MaxParticles 0 do
                       i particle-ndx p-xg F@ FDUP -1.5e F> if
                         0.01e F- i particle-ndx p-xg F!
                       else
                         FDROP
                       then
                     loop
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
    SDLK_ESCAPE   of FALSE to key-ESC    endof
    SDLK_F1       of FALSE to key-F1     endof
    SDLK_TAB      of FALSE to key-Tab    endof
    SDLK_RETURN   of FALSE to key-Return endof
    SDLK_SPACE    of FALSE to key-Space  endof
    SDLK_PAGEUP   of FALSE to key-PgUp   endof
    SDLK_PAGEDOWN of FALSE to key-PgDn   endof
    SDLK_UP       of FALSE to key-Up     endof
    SDLK_DOWN     of FALSE to key-Dn     endof
    SDLK_RIGHT    of FALSE to key-Right  endof
    SDLK_LEFT     of FALSE to key-Left   endof
    SDLK_KP_PLUS  of FALSE to key-KP+    endof
    SDLK_KP_MINUS of FALSE to key-KP-    endof
    SDLK_KP2      of FALSE to key-KP2    endof
    SDLK_KP4      of FALSE to key-KP4    endof
    SDLK_KP6      of FALSE to key-KP6    endof
    SDLK_KP8      of FALSE to key-KP8    endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    false
  else
    \ Allocate space for the particle[] array - 116,000 bytes
    particle[] 0= if      \ if &particle[]!=0 skip - already allotted
      \ assign the address
      here to particle[]
      \ Allot the space and then clear it to zeroes - 116,000 bytes
      particle[] particle% nip MaxParticles * dup allot 0 fill
    then
    \ Enable smooth shading
    GL_SMOOTH gl-shade-model
    \ Set the background black
    0e 0e 0e 0e gl-clear-color
    \ Depth buffer setup
    1e gl-clear-depth
    \ Disable depth testing
    GL_DEPTH_TEST gl-disable
    \ Enable blending
    GL_BLEND gl-enable
    \ Type of blending to perform
    GL_SRC_ALPHA GL_ONE gl-blend-func
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
    \ Really nice point smoothing
    GL_POINT_SMOOTH_HINT GL_NICEST gl-hint
    \ Enable texture mapping
    GL_TEXTURE_2D gl-enable
    \ Select our texture
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
    \ Reset all the particles
    MaxParticles 0 do
      i                                                         \ num
      i 1 + MaxParticles 12 / /                               \ color
      50 random S>F 26e F- 10e F*                                \ xi
      50 random S>F 25e F- 10e F*                                \ yi
      FDUP                                                       \ zi
      ResetParticle
    loop
    \ Enable rainbow coloring at start
    TRUE to rainbow
    \ Return a good value
    TRUE
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  NumTextures texture gl-delete-textures          \ clean up textures
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

\ Float variables used in DrawGLScene

fvariable ds-x
fvariable ds-y
fvariable ds-z
0 value ds-ptr                 \ storage for pointer <i particle-ndx>

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  gl-load-identity
  \ Modify each of the particles
  MaxParticles 0 do
    i particle-ndx to ds-ptr
    ds-ptr p-active @ if                    \ is the particle active?
      ds-ptr p-x F@ ds-x F!            \ Grab our particle x position
      ds-ptr p-y F@ ds-y F!            \ Grab our particle y position
      ds-ptr p-z F@ zoom F@ F+ ds-z F! \ Grab our particle z position

      \ Draw particle using our RGB values, fading based on its life
      ds-ptr dup p-r F@ dup p-g F@ dup p-b F@ p-life F@ gl-color-4f
      
      GL_TRIANGLE_STRIP gl-begin   \ Build quad from a triangle strip
        1e 1e gl-tex-coord-2f                             \ top right
        ds-x F@ 0.5e F+ ds-y F@ 0.5e F+ ds-z F@ gl-vertex-3f
        0e 1e gl-tex-coord-2f                              \ top left
        ds-x F@ 0.5e F- ds-y F@ 0.5e F+ ds-z F@ gl-vertex-3f
        1e 0e gl-tex-coord-2f                          \ bottom right
        ds-x F@ 0.5e F+ ds-y F@ 0.5e F- ds-z F@ gl-vertex-3f
        0e 0e gl-tex-coord-2f                           \ bottom left
        ds-x F@ 0.5e F- ds-y F@ 0.5e F- ds-z F@ gl-vertex-3f
      gl-end

      \ gforth uses two stacks - one for integers and one for floats.
      \ ds-ptr puts an address onto the int stack, while the F@ F* F+
      \ and F! words work with data on the float stack.
      \ The <dup> word makes a copy of the top of the integer stack.
      \ Keeping this in mind, the flow here is pretty easy to follow.

      \ Move on the x axis by x speed
      ds-ptr dup p-x F@ dup p-xi F@ slowdown F@ 1000e F* F/ F+ p-x F!
      \ Move on the y axis by y speed
      ds-ptr dup p-y F@ dup p-yi F@ slowdown F@ 1000e F* F/ F+ p-y F!
      \ Move on the z axis by y speed
      ds-ptr dup p-z F@ dup p-zi F@ slowdown F@ 1000e F* F/ F+ p-z F!
      \ Take pull on x axis into account
      ds-ptr dup p-xi F@ dup p-xg F@ F+ p-xi F!
      \ Take pull on y axis into account
      ds-ptr dup p-yi F@ dup p-yg F@ F+ p-yi F!
      \ Take pull on z axis into account
      ds-ptr dup p-zi F@ dup p-zg F@ F+ p-zi F!
      \ Reduce the particles' life by 'fade' value
      ds-ptr p-life F@ ds-ptr p-fade F@ F- ds-ptr p-life F!
      \ If the particle dies, revive it
      ds-ptr p-life F@ F0< if
        i                                                       \ num
        rainbow-color                                         \ color
        xspeed F@ 60 random S>F 32e F- F+                        \ xi
        yspeed F@ 60 random S>F 30e F- F+                        \ yi
        60 random S>F 32e F-                                     \ zi
        ResetParticle
      then
    then
  loop
  
  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window
  
  \ Return a good value
  TRUE
;

