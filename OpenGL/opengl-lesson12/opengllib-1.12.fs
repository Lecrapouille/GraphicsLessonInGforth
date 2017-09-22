\ ===================================================================
\           File: opengllib-1.12.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/26/2010
\    Description: Display lists
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

require mini-opengl-1.12.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : BuildLists                  ( -- )
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

1 constant NumTextures                    \ number of textures to use

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

variable dl-box                        \ storage for the display list
variable dl-top                 \ storage for the second display list

fvariable x-rot
fvariable y-rot

\ ---[ Array Definitions ]-------------------------------------------
\ These are passed by address to OpenGL, so they must be sfloats

\ Array for the box colors - bright
create box-col[]
1e SF,   0e SF, 0e SF,                                          \ red
1e SF, 0.5e SF, 0e SF,                                       \ orange
1e SF,   1e SF, 0e SF,                                       \ yellow
0e SF,   1e SF, 0e SF,                                        \ green
0e SF,   1e SF, 1e SF,                                         \ blue

\ Array for the top colors - dark
create top-col[]
0.5e SF,    0e SF,   0e SF,                                     \ red
0.5e SF, 0.25e SF,   0e SF,                                  \ orange
0.5e SF,  0.5e SF,   0e SF,                                  \ yellow
  0e SF,  0.5e SF,   0e SF,                                   \ green
  0e SF,  0.5e SF, 0.5e SF,                                    \ blue

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

: color-ndx ( *col n -- *col[n] ) 3 sfloats * + ;

\ ---[ BuildLists ]--------------------------------------------------

: BuildLists ( -- )
  2 gl-gen-lists dl-box !                           \ Build two lists
  dl-box @ GL_COMPILE gl-new-list    \ New compiled display list, box
    GL_QUADS gl-begin
      0e 1e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f    \ Bottom face
      1e 1e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f
      1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f
      0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f

      1e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f     \ Front face
      0e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f
      0e 1e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f
      1e 1e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f

      0e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f      \ Back face
      0e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f
      1e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f
      1e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f

      0e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f     \ Right face
      0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f
      1e 1e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f
      1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f

      1e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f      \ Left face
      0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f
      0e 1e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f
      1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f
    gl-end
  gl-end-list
  
  dl-box @ 1+ dl-top !

  dl-top @ GL_COMPILE gl-new-list    \ New compiled display list, top
    GL_QUADS gl-begin
      1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f       \ Top face
      1e 0e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f
      0e 0e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f
      0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f
    gl-end
  gl-end-list  
;

\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as a GL texture

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
  s" data/cube.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value

    NumTextures texture gl-gen-textures          \ create the texture

    \ texture generation using data from the bitmap
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture

    \ Generate the texture
    R@ Generate-MipMapped-Texture

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR 
    GL_LINEAR_MIPMAP_NEAREST gl-tex-parameter-i    
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
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   Left     make x rotation more negative
\   Right    make x rotation more positive
\   Up       make y rotation more negative
\   Down     make y rotation more positive

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_LEFT     of y-rot F@ 0.5e F- y-rot F! endof
    SDLK_RIGHT    of y-rot F@ 0.5e F+ y-rot F! endof
    SDLK_UP       of x-rot F@ 0.5e F- x-rot F! endof
    SDLK_DOWN     of x-rot F@ 0.5e F+ x-rot F! endof
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
    \ Build our display lists
    BuildLists
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
    \ Enable lighting
    GL_LIGHT0 gl-enable                    \ quick and dirty lighting
    GL_LIGHTING gl-enable                           \ enable lighting
    GL_COLOR_MATERIAL gl-enable            \ enable material coloring
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
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

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  \ Select our texture
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
  \ Start loading in our display lists
  6 1 do
    i 0 do
      gl-load-identity                           \ reset the matrix
      \ Position the cubes on the screen
      1.4e i S>F 2.8e F* F+ j S>F 1.4e F* F-    \ 1st param
      6e j S>F F- 2.4e F* 7e F-                 \ 2nd param
      -20e                                      \ 3rd param
      gl-translate-f
      \ Tilt the cubes up and down
      45e 2e j S>F F* F- x-rot F@ f+ 1e 0e 0e gl-rotate-f
      \ Spin cubes left and right
      45e y-rot F@ F+ 0e 1e 0e gl-rotate-f
      \ Select a color -- box
      box-col[] j 1 - color-ndx gl-color-3fv
      \ Draw the box
      dl-box @ gl-call-list
      \ Select a color -- top
      top-col[] j 1 - color-ndx gl-color-3fv
      \ Draw the top
      dl-top @ gl-call-list
    loop
  loop

  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  \ Return a good value
  TRUE
;

