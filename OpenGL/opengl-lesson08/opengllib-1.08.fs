\ ===================================================================
\           File: opengllib-1.08.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/25/2010
\    Description: Blending
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 08
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

require mini-opengl-1.08.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : Generate-Texture            { *src -- }
\ : Generate-MipMapped-Texture  { *src -- }
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

3 constant NumTextures                    \ number of textures to use

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

fvariable xrot                                           \ X rotation
fvariable yrot                                           \ Y rotation
fvariable xspeed                                   \ x rotation speed
fvariable yspeed                                   \ y rotation speed
fvariable zdepth                              \ depth into the screen

FALSE value light                     \ whether or not lighting is on
FALSE value blend                     \ whether or not blending is on

\ ---[ Light Values ]------------------------------------------------
\ The following three arrays are RGBA color shadings.
\ They can be accessed by either an index function, or by a struct

\ These light tables are passed by address to gl-light-fv, not value,
\ so they must be stored as 32-bit floats, not normal 64-bit floats.

\ Ambient Light Values
create LightAmbient[]   0.5e SF, 0.5e SF, 0.5e SF, 1e SF,

\ Diffuse Light Values
create LightDiffuse[]   1e SF, 1e SF, 1e SF, 1e SF,

\ Light Position
create LightPosition[]  0e SF, 0e SF, 2e SF, 1e SF,

0 value filter                                 \ which filter to use

\ ---[ Variable Initializations ]------------------------------------

0e xrot F!
0e yrot F!
0e xspeed F!
0e yspeed F!
-5e zdepth F!

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

: Generate-MipMapped-Texture { *src -- }
  GL_TEXTURE_2D 3
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  GL_BGR                                  \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  glu-build-2d-mipmaps                          \ finally generate it
;

: LoadGLTextures ( -- status )
  FALSE { _status -- status }                     \ init return value
  s" data/glass.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value
    NumTextures texture gl-gen-textures         \ create the textures

  \ Load in texture 1
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture        \ texture[0]

    \ Generate texture 1
    R@ Generate-Texture

    \ Nearest Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_NEAREST gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_NEAREST gl-tex-parameter-i    

  \ Load in texture 2
    GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture        \ texture[1]

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    \ Generate texture 2
    R@ Generate-Texture

  \ Load in texture 3
    GL_TEXTURE_2D 2 texture-ndx @ gl-bind-texture        \ texture[2]

    \ MipMapped Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR_MIPMAP_NEAREST
    gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    \ Generate MipMapped texture 3
    R@ Generate-MipMapped-Texture

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
0 value key-b
0 value key-f
0 value key-l
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   PageUp   zooms into the scene
\   PageDown zooms out of the scene
\   b        toggles blending on/off
\   f        pages thru the different filters [0..2]
\   l        toggles the light on/off
\   Up       makes x rotation more negative - incremental
\   Down     makes x rotation more positive - incremental
\   Left     makes y rotation more negative - incremental
\   Right    makes y rotation more positive - incremental
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
    SDLK_PAGEUP   of zdepth F@ 0.02e F- zdepth F! endof
    SDLK_PAGEDOWN of zdepth F@ 0.02e F+ zdepth F! endof
    SDLK_b        of key-b FALSE = if       \ skip if being held down
                       1 blend xor dup to blend if
                         GL_BLEND gl-enable
                         GL_DEPTH_TEST gl-disable
                       else
                         GL_BLEND gl-disable
                         GL_DEPTH_TEST gl-enable
                       then
                       TRUE to key-b           \ set key pressed flag
                     then
                  endof
    SDLK_f        of key-f FALSE = if       \ skip if being held down
                       filter 1+ 3 MOD to filter
                       TRUE to key-f           \ set key pressed flag
                     then
                  endof
    SDLK_l        of key-l FALSE = if       \ skip if being held down
                       1 light xor to light
                       light if   GL_LIGHTING gl-enable
                             else GL_LIGHTING gl-disable
                             then
                       TRUE to key-l           \ set key pressed flag
                     then
                  endof
    SDLK_UP       of xspeed F@ 0.01e F- xspeed F! endof
    SDLK_DOWN     of xspeed F@ 0.01e F+ xspeed F! endof
    SDLK_RIGHT    of yspeed F@ 0.01e F+ yspeed F! endof
    SDLK_LEFT     of yspeed F@ 0.01e F- yspeed F! endof
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
    SDLK_b        of FALSE to key-b     endof
    SDLK_f        of FALSE to key-f     endof
    SDLK_l        of FALSE to key-l     endof
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
    SDLK_RIGHT    of FALSE to key-Right endof
    SDLK_LEFT     of FALSE to key-Left  endof
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
    \ Enable depth testing
    GL_DEPTH_TEST gl-enable
    \ Type of depth test to do
    GL_LEQUAL gl-depth-func
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
    \ Set up the ambient light
    GL_LIGHT1 GL_AMBIENT LightAmbient[] gl-light-fv
    \ Set up the diffuse light
    GL_LIGHT1 GL_DIFFUSE LightDiffuse[] gl-light-fv
    \ Position the light
    GL_LIGHT1 GL_POSITION LightPosition[] gl-light-fv
    \ Enable Light One
    GL_LIGHT1 gl-enable
    \ Full brightness, 50% Alpha
    1e 1e 1e 0.5e gl-color-4f
    \ Blending translucency based on source alpha value
    GL_SRC_ALPHA GL_ONE gl-blend-func
    \ Return a good value
    TRUE
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag               \ reset these for next time
  0e xrot F!
  0e yrot F!
  0e xspeed F!
  0e yspeed F!
  -5e zdepth F!
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

  gl-load-identity                                   \ restore matrix

  \ Translate into/out of the screen by zdepth
  0e 0e zdepth F@ gl-translate-f

  xrot F@ 1e 0e 0e gl-rotate-f                 \ rotate on the X axis  
  yrot F@ 0e 1e 0e gl-rotate-f                 \ rotate on the Y axis
  
  \ Select a texture based on filter
  GL_TEXTURE_2D filter texture-ndx @ gl-bind-texture

  GL_QUADS gl-begin                                     \ draw a quad
  \ Front face
    0e 0e 1e gl-normal-3f            \ normal pointing towards viewer
    1e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f  \ Point 1 - front
    0e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f  \ Point 2 - front
    0e 1e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f  \ Point 3 - front
    1e 1e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f  \ Point 4 - front
    
    \ Back face
    0e 0e -1e gl-normal-3f         \ Normal pointing away from viewer
    0e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f   \ Point 1 - back
    0e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f   \ Point 2 - back
    1e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f   \ Point 3 - back
    1e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f   \ Point 4 - back
  
    \ Top face
    0e 1e 0e gl-normal-3f                        \ Normal pointing up
    1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f    \ Point 1 - top
    1e 0e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f    \ Point 2 - top
    0e 0e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f    \ Point 3 - top
    0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f    \ Point 4 - top
  
    \ Bottom face
    0e -1e 0e gl-normal-3f                     \ Normal pointing down
    0e 1e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f \ Point 1 - bottom
    1e 1e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f \ Point 2 - bottom
    1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f \ Point 3 - bottom
    0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f \ Point 4 - bottom
  
    \ Right face
    1e 0e 0e gl-normal-3f                     \ Normal pointing right
    0e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f  \ Point 1 - right
    0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f  \ Point 1 - right
    1e 1e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f  \ Point 1 - right
    1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f  \ Point 1 - right
  
    \ Left face
    -1e 0e 0e gl-normal-3f                     \ Normal pointing left
    1e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f   \ Point 1 - left
    0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f   \ Point 2 - left
    0e 1e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f   \ Point 3 - left
    1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f   \ Point 4 - left
  gl-end

  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  xrot F@ xspeed F@ F+ xrot F!                   \ add xspeed to xrot
  yrot F@ yspeed F@ F+ yrot F!                   \ add yspeed to yrot

  TRUE
;

