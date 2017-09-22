\ ===================================================================
\           File: opengllib-1.16.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/31/2010
\    Description: Cool looking fog
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 16
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

require mini-opengl-1.16.fs
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

3 constant NumTextures                     \ number of texture to use

fvariable xrot                                           \ X rotation
fvariable yrot                                           \ Y rotation
fvariable xspeed                                   \ x rotation speed
fvariable yspeed                                   \ y rotation speed
fvariable zdepth                              \ depth into the screen

0 value FogFilter                                  \ which fog to use
0 value filter                                  \ which filter to use
FALSE value light                         \ lighting is initially off

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

create FogMode[] GL_EXP , GL_EXP2 , GL_LINEAR ,      \ 3 types of fog

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;
: fogmode-ndx ( n -- *texture[n] ) 3 mod cell * FogMode[] + ;

\ ---[ Light Values ]------------------------------------------------
\ The following three arrays are RGBA color shadings.
\ These light tables are passed by address to gl-light-fv, not value
\ Therefore, OpenGL expects them to be 32-bit floats (gforth sfloats)

\ Ambient Light Values
create LightAmbient[]   0.5e SF, 0.5e SF, 0.5e SF, 1e SF,

\ Diffuse Light Values
create LightDiffuse[]   1e SF, 1e SF, 1e SF, 1e SF,

\ Light Position
create LightPosition[]  0e SF, 0e SF, 2e SF, 1e SF,

\ Fog Color
create FogColor[]       0.5e SF, 0.5e SF, 0.5e SF, 1e SF,

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
  s" data/crate.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
   cr ." Texture Image loaded" cr
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

0 value key-ESC
0 value key-F1
0 value key-f
0 value key-g
0 value key-l
0 value key-PgUp
0 value key-PgDn
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   f        cycle thru the different filters
\   g        cycle thru the different fogs
\   l        toggles the light on/off
\   PageUp   zooms into the scene
\   PageDown zooms out of the scene
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
    SDLK_f        of key-f FALSE = if       \ skip if being held down
                       filter 1+ 3 MOD to filter
                       TRUE to key-f           \ set key pressed flag
                     then
                  endof
    SDLK_g        of key-g FALSE = if       \ skip if being held down
                       FogFilter 1+ 3 MOD to FogFilter
                       GL_FOG_MODE FogFilter fogmode-ndx @ gl-fog-i
                       TRUE to key-g           \ set key pressed flag
                     then
                  endof
    SDLK_l        of key-l FALSE = if       \ skip if being held down
                       light if 0 else 1 then to light
                       light if   GL_LIGHTING gl-enable
                             else GL_LIGHTING gl-disable
                             then
                       TRUE to key-l           \ set key pressed flag
                     then
                  endof
    SDLK_PAGEUP   of zdepth F@ 0.02e F- zdepth F! endof
    SDLK_PAGEDOWN of zdepth F@ 0.02e F+ zdepth F! endof
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
    SDLK_f        of FALSE to key-f     endof
    SDLK_g        of FALSE to key-g     endof
    SDLK_l        of FALSE to key-l     endof
    SDLK_PAGEUP   of FALSE to key-PgUp  endof
    SDLK_PAGEDOWN of FALSE to key-PgDn  endof
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
    \ Set the background color
    0.5e 0.5e 0.5e 1e gl-clear-color
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

    \ ---[ Set up the Fog ]---

    GL_FOG_MODE FogFilter fogmode-ndx @ gl-fog-i       \ Set Fog mode
    GL_FOG_COLOR FogColor[] gl-fog-fv                 \ Set Fog color
    GL_FOG_DENSITY 0.35e gl-fog-f            \ Set density of the Fog
    GL_FOG_HINT GL_DONT_CARE gl-hint                 \ Fog Hint value
    GL_FOG_START 1e gl-fog-f                        \ Fog start depth
    GL_FOG_END 5e gl-fog-f                            \ Fog end depth
    GL_FOG gl-enable                                  \ Enable GL_FOG
    
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

  \ Return a good value
  TRUE
;

