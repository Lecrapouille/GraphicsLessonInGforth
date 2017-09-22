\ ===[ Code Addendum 02 ]============================================
\                 gforth: OpenGL Graphics Lesson 10
\ ===================================================================
\           File: opengllib-1.10.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/25/2010
\    Description: Moving in a 3D world
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 10
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

require mini-opengl-1.10.fs
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

\ ---[ Structs ]-----------------------------------------------------

\ Build Our Vertex Structure
struct
  FLOAT% field vertex-x                              \ 3D Coordinates
  FLOAT% field vertex-y
  FLOAT% field vertex-z
  FLOAT% field vertex-u                         \ Texture Coordinates
  FLOAT% field vertex-v
end-struct vertex%

\ Build Our Triangle Structure
struct
  vertex% 3 * field vertex[]                \ Array Of Three Vertices
end-struct triangle%

\ Build Our Sector Structure
struct
  cell% field sector-#tris                 \ # of triangles in sector
  cell% field sector-*tris            \ pointer to array of triangles
end-struct sector%

\ ---[ sector1 ]-----------------------------------------------------
\ Contains the number of polygons in the image, and the address of
\ the array in the dictionary.

sector% %allot value sector1                             \ our sector

\ ---[ sector-ndx ]--------------------------------------------------
\ Passed the number of the triangle to access, and the specific
\ vertex required, the base address of that vertex set is returned,
\ from which the vertex -x/-y/-z/-u/-v field can be used to get/set
\ the required vertex address.

\ x_m = sector1.triangle[i].vertex[0].x

: sector-ndx { _#tri _#ver -- *tri[#tri].vertex[#ver] }
  sector1 sector-*tris @        \ *tri[0]
  triangle% nip _#tri * +       \ *tri[#tri]
  vertex% nip _#ver * +         \ *(tri[#tri].vertex[#ver])
;

3 constant NumTextures                    \ number of textures to use

fvariable y-rot                             \ Camera rotation variable
fvariable x-pos                                 \ Camera pos variables
fvariable z-pos
fvariable walkbias                            \ head-bobbin variables
fvariable walkbiasangle
fvariable lookupdown

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

\ ---[ Light Values ]------------------------------------------------
\ The following three arrays are RGBA color shadings.

\ Ambient Light Values
create LightAmbient[]   0.5e SF, 0.5e SF, 0.5e SF, 1e SF,

\ Diffuse Light Values
create LightDiffuse[]   1e SF, 1e SF, 1e SF, 1e SF,

\ Light Position
create LightPosition[]  0e SF, 0e SF, 2e SF, 1e SF,

0 value filter                                  \ which filter to use

pi 180e F/ fconstant PiOver180            \ for converting to radians

\ ---[ Polygon Definitions ]-----------------------------------------
\ This data definition area holds the contents of the world.txt file
\ that is included in the Lesson 10 source archive.

create [sector-data]

\ Store all of the polygon info into the dictionary.
\ Each group of three lines is a single triangle% struct

0                                              \ init polygon counter

\  x      y       z        u     v
  -3e F,  0e F,  -3e F,    0e F, 6e F,                      \ Floor 1
  -3e F,  0e F,   3e F,    0e F, 0e F,
   3e F,  0e F,   3e F,    6e F, 0e F, 1+      \ increment poly count

  -3e F,  0e F,  -3e F,    0e F, 6e F,
   3e F,  0e F,  -3e F,    6e F, 6e F,
   3e F,  0e F,   3e F,    6e F, 0e F, 1+         \ after each struct

  -3e F,  1e F,  -3e F,    0e F, 6e F,                    \ Ceiling 1
  -3e F,  1e F,   3e F,    0e F, 0e F,
   3e F,  1e F,   3e F,    6e F, 0e F, 1+

  -3e F,  1e F,  -3e F,    0e F, 6e F,
   3e F,  1e F,  -3e F,    6e F, 6e F,
   3e F,  1e F,   3e F,    6e F, 0e F, 1+

  -2e F,  1e F,   -2e F,   0e F, 1e F,                           \ A1
  -2e F,  0e F,   -2e F,   0e F, 0e F,
-0.5e F,  0e F,   -2e F, 1.5e F, 0e F, 1+

  -2e F,  1e F,   -2e F,   0e F, 1e F,
-0.5e F,  1e F,   -2e F, 1.5e F, 1e F,
-0.5e F,  0e F,   -2e F, 1.5e F, 0e F, 1+

   2e F,  1e F,   -2e F,   2e F, 1e F,                           \ A2
   2e F,  0e F,   -2e F,   2e F, 0e F,
 0.5e F,  0e F,   -2e F, 0.5e F, 0e F, 1+

   2e F,  1e F,   -2e F,   2e F, 1e F,
 0.5e F,  1e F,   -2e F, 0.5e F, 1e F,
 0.5e F,  0e F,   -2e F, 0.5e F, 0e F, 1+

  -2e F,  1e F,    2e F,   2e F, 1e F,                           \ B1
  -2e F,  0e F,    2e F,   2e F, 0e F,
-0.5e F,  0e F,    2e F, 0.5e F, 0e F, 1+

  -2e F,  1e F,    2e F,   2e F, 1e F,
-0.5e F,  1e F,    2e F, 0.5e F, 1e F,
-0.5e F,  0e F,    2e F, 0.5e F, 0e F, 1+

   2e F,  1e F,    2e F,   2e F, 1e F,                           \ B2
   2e F,  0e F,    2e F,   2e F, 0e F,
 0.5e F,  0e F,    2e F, 0.5e F, 0e F, 1+

   2e F,  1e F,    2e F,   2e F, 1e F,
 0.5e F,  1e F,    2e F, 0.5e F, 1e F,
 0.5e F,  0e F,    2e F, 0.5e F, 0e F, 1+

  -2e F,  1e F,   -2e F,   0e F, 1e F,                           \ C1
  -2e F,  0e F,   -2e F,   0e F, 0e F,
  -2e F,  0e F, -0.5e F, 1.5e F, 0e F, 1+

  -2e F,  1e F,   -2e F,   0e F, 1e F,
  -2e F,  1e F, -0.5e F, 1.5e F, 1e F,
  -2e F,  0e F, -0.5e F, 1.5e F, 0e F, 1+

  -2e F,  1e F,    2e F,   2e F, 1e F,                           \ C2
  -2e F,  0e F,    2e F,   2e F, 0e F,
  -2e F,  0e F,  0.5e F, 0.5e F, 0e F, 1+

  -2e F,  1e F,    2e F,   2e F, 1e F,
  -2e F,  1e F,  0.5e F, 0.5e F, 1e F,
  -2e F,  0e F,  0.5e F, 0.5e F, 0e F, 1+

   2e F,  1e F,   -2e F,   0e F, 1e F,                           \ D1
   2e F,  0e F,   -2e F,   0e F, 0e F,
   2e F,  0e F, -0.5e F, 1.5e F, 0e F, 1+

   2e F,  1e F,   -2e F,   0e F, 1e F,
   2e F,  1e F, -0.5e F, 1.5e F, 1e F,
   2e F,  0e F, -0.5e F, 1.5e F, 0e F, 1+

   2e F,  1e F,    2e F,   2e F, 1e F,                           \ D2
   2e F,  0e F,    2e F,   2e F, 0e F,
   2e F,  0e F,  0.5e F, 0.5e F, 0e F, 1+

   2e F,  1e F,    2e F,   2e F, 1e F,
   2e F,  1e F,  0.5e F, 0.5e F, 1e F,
   2e F,  0e F,  0.5e F, 0.5e F, 0e F, 1+

-0.5e F,  1e F,   -3e F,   0e F, 1e F,            \ Upper hallway - L
-0.5e F,  0e F,   -3e F,   0e F, 0e F,
-0.5e F,  0e F,   -2e F,   1e F, 0e F, 1+

-0.5e F,  1e F,   -3e F,   0e F, 1e F,
-0.5e F,  1e F,   -2e F,   1e F, 1e F,
-0.5e F,  0e F,   -2e F,   1e F, 0e F, 1+

 0.5e F,  1e F,   -3e F,   0e F, 1e F,            \ Upper hallway - R
 0.5e F,  0e F,   -3e F,   0e F, 0e F,
 0.5e F,  0e F,   -2e F,   1e F, 0e F, 1+

 0.5e F,  1e F,   -3e F,   0e F, 1e F,
 0.5e F,  1e F,   -2e F,   1e F, 1e F,
 0.5e F,  0e F,   -2e F,   1e F, 0e F, 1+

-0.5e F,  1e F,    3e F,   0e F, 1e F,            \ Lower hallway - L
-0.5e F,  0e F,    3e F,   0e F, 0e F,
-0.5e F,  0e F,    2e F,   1e F, 0e F, 1+

-0.5e F,  1e F,    3e F,   0e F, 1e F,
-0.5e F,  1e F,    2e F,   1e F, 1e F,
-0.5e F,  0e F,    2e F,   1e F, 0e F, 1+

 0.5e F,  1e F,    3e F,   0e F, 1e F,            \ Lower hallway - R
 0.5e F,  0e F,    3e F,   0e F, 0e F,
 0.5e F,  0e F,    2e F,   1e F, 0e F, 1+

 0.5e F,  1e F,    3e F,   0e F, 1e F,
 0.5e F,  1e F,    2e F,   1e F, 1e F,
 0.5e F,  0e F,    2e F,   1e F, 0e F, 1+

  -3e F,  1e F,  0.5e F,   1e F, 1e F,            \ Left hallway - Lw
  -3e F,  0e F,  0.5e F,   1e F, 0e F,
  -2e F,  0e F,  0.5e F,   0e F, 0e F, 1+

  -3e F,  1e F,  0.5e F,   1e F, 1e F,
  -2e F,  1e F,  0.5e F,   0e F, 1e F,
  -2e F,  0e F,  0.5e F,   0e F, 0e F, 1+

  -3e F,  1e F, -0.5e F,   1e F, 1e F,            \ Left hallway - Hi
  -3e F,  0e F, -0.5e F,   1e F, 0e F,
  -2e F,  0e F, -0.5e F,   0e F, 0e F, 1+

  -3e F,  1e F, -0.5e F,   1e F, 1e F,
  -2e F,  1e F, -0.5e F,   0e F, 1e F,
  -2e F,  0e F, -0.5e F,   0e F, 0e F, 1+

   3e F,  1e F,  0.5e F,   1e F, 1e F,           \ Right hallway - Lw
   3e F,  0e F,  0.5e F,   1e F, 0e F,
   2e F,  0e F,  0.5e F,   0e F, 0e F, 1+

   3e F,  1e F,  0.5e F,   1e F, 1e F,
   2e F,  1e F,  0.5e F,   0e F, 1e F,
   2e F,  0e F,  0.5e F,   0e F, 0e F, 1+

   3e F,  1e F, -0.5e F,   1e F, 1e F,           \ Right hallway - Hi
   3e F,  0e F, -0.5e F,   1e F, 0e F,
   2e F,  0e F, -0.5e F,   0e F, 0e F, 1+

   3e F,  1e F, -0.5e F,   1e F, 1e F,
   2e F,  1e F, -0.5e F,   0e F, 1e F,
   2e F,  0e F, -0.5e F,   0e F, 0e F, 1+

value #Polygons

[sector-data] sector1 sector-*tris !      \ Initialize sector1 fields
#Polygons     sector1 sector-#tris !

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
  s" data/mud.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value
    3 texture gl-gen-textures                    \ create the texture

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
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   Up       move forward
\   Down     move backward
\   Right    turns camera to the right
\   Left     turns camera to the left


: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_UP       of
                     x-pos F@ 
                     y-rot F@ PiOver180 F* FSIN 0.05e F*
                     F- x-pos F!
                     z-pos F@ 
                     y-rot F@ PiOver180 F* FCOS 0.05e F*
                     F- z-pos F!
                     walkbiasangle F@ 359e F>= if
                       0e
                     else
                       walkbiasangle F@ 10e F+
                     then
                     FDUP walkbiasangle F!
                     \ Cause the 'player' to bounce
                     PiOver180 F* FSIN 20e F/ walkbias F!
                  endof
    SDLK_DOWN     of
                     x-pos F@ 
                     y-rot F@ PiOver180 F* FSIN 0.05e F*
                     F+ x-pos F!
                     z-pos F@ 
                     y-rot F@ PiOver180 F* FCOS 0.05e F*
                     F+ z-pos F!
                     walkbiasangle F@ 1e F<= if
                       359e
                     else
                       walkbiasangle F@ 10e F-
                     then
                     FDUP walkbiasangle F!
                     \ Cause the 'player' to bounce
                     PiOver180 F* FSIN 20e F/ walkbias F!
                  endof
    SDLK_RIGHT    of y-rot F@ 1.5e F- y-rot F! endof
    SDLK_LEFT     of y-rot F@ 1.5e F+ y-rot F! endof
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

    0e lookupdown F!
    0e walkbias F!
    0e walkbiasangle F!

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

fvariable x-m
fvariable y-m
fvariable z-m
fvariable u-m
fvariable v-m
fvariable x-trans
fvariable z-trans
fvariable y-trans
fvariable SceneRotY

: DrawGLScene ( -- boolean )
  x-pos F@ fnegate x-trans F!
  z-pos F@ fnegate z-trans F!
  walkbias F@ 0.25e F- y-trans F!
  360e y-rot F@ F- SceneRotY F!
  
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  gl-load-identity                                   \ restore matrix
  \ Rotate up and down to look up and down
  lookupdown F@ 1e 0e 0e gl-rotate-f
  \ Rotate depending on direction 'player' is facing
  SceneRotY F@ 0e 1e 0e gl-rotate-f
  \ Translate the scene based on 'player' position
  x-trans F@ y-trans F@ z-trans F@ gl-translate-f
  \ Select a texture based on filter
  GL_TEXTURE_2D filter texture-ndx @ gl-bind-texture
  
  \ Process each triangle
  sector1 sector-#tris @ 0 do
    GL_TRIANGLES gl-begin
      \ Normal pointing forward
      0e 0e 1e gl-normal-3f

      \ Vertices of first point
      i 0 sector-ndx vertex-u F@
      i 0 sector-ndx vertex-v F@
      gl-tex-coord-2f                        \ set texture coordinate
      i 0 sector-ndx vertex-x F@
      i 0 sector-ndx vertex-y F@
      i 0 sector-ndx vertex-z F@
      gl-vertex-3f                                  \ set the vertice

      \ Vertices of second point
      i 1 sector-ndx vertex-u F@
      i 1 sector-ndx vertex-v F@
      gl-tex-coord-2f                        \ set texture coordinate
      i 1 sector-ndx vertex-x F@
      i 1 sector-ndx vertex-y F@
      i 1 sector-ndx vertex-z F@
      gl-vertex-3f                                  \ set the vertice

      \ Vertices of third point
      i 2 sector-ndx vertex-u F@
      i 2 sector-ndx vertex-v F@
      gl-tex-coord-2f                        \ set texture coordinate
      i 2 sector-ndx vertex-x F@
      i 2 sector-ndx vertex-y F@
      i 2 sector-ndx vertex-z F@
      gl-vertex-3f                                  \ set the vertice
    gl-end
  loop
  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  \ Return a good value
  TRUE
;

