\ ===[ Code Addendum 02 ]============================================
\                 gforth: OpenGL Graphics Lesson 06
\ ===================================================================
\           File: opengllib-1.06.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/24/2010
\    Description: Texture Mapping
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 06
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

require mini-opengl-1.06.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : HandleKeyPress              ( &event -- )
\ : Generate-Texture            { *src -- }
\ : LoadGLTextures              ( -- boolean )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

1 constant NumTextures                    \ number of textures to use

\ Added for Lesson 06
fvariable xrot                                           \ X rotation
fvariable yrot                                           \ Y rotation
fvariable zrot                                           \ Z rotation

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

\ ---[ Variable Initializations ]------------------------------------

0e xrot F!
0e yrot F!
0e zrot F!

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE of TRUE to opengl-exit-flag endof
    SDLK_F1     of screen sdl-wm-togglefullscreen drop endof
  endcase
;

\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as an OpenGL texture

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
  FALSE { _status -- status }         \ create local for return value
  s" data/nehe.bmp0" zstring sdl-loadbmp dup 0<> if
    >R                                    \ image loaded successfully
    TRUE to _status                                \ set return value
    NumTextures texture gl-gen-textures         \ create the textures
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture   \ typical texture
    R@ Generate-Texture                        \ Generate the texture
    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    
    R> sdl-freesurface                   \ release the original image
  else
    drop                                       \ unable to load image
    cr ." Error: texture image could not be loaded!" cr
  then
  _status               \ exit with return value: FALSE=fail; TRUE=ok
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    \ Return a bad value
    FALSE
  else
    GL_TEXTURE_2D gl-enable                  \ Enable texture mapping
    GL_SMOOTH gl-shade-model                  \ Enable smooth shading
    0e 0e 0e 0e gl-clear-color             \ Set the background black
    1e gl-clear-depth                            \ Depth buffer setup
    GL_DEPTH_TEST gl-enable                    \ Enable depth testing
    GL_LEQUAL gl-depth-func                \ Type of depth test to do
    \ Really nice perspective calculations
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
    TRUE                                        \ Return a good value
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

  gl-load-identity                                   \ restore matrix

  0e 0e -5e gl-translate-f             \ Move into the screen 5 units

  xrot F@ 1e 0e 0e gl-rotate-f                 \ rotate on the X axis  
  yrot F@ 0e 1e 0e gl-rotate-f                 \ rotate on the Y axis
  zrot F@ 0e 0e 1e gl-rotate-f                 \ rotate on the Z axis
  
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture  \ Select our texture

\ ---[Note]----------------------------------------------------------
\ The x coordinates of the glTexCoord2f function need to be inverted
\ for SDL because of the way SDL_LoadBmp loads the data. So where
\ in the tutorial it has glTexCoord2f( 1.0f, 0.0f ); it should
\ now read glTexCoord2f( 0.0f, 1.0f );                   - Ti Leggett
\ ------------------------------------------------------[End Note]---
  
  GL_QUADS gl-begin                                     \ draw a quad
  \ Front face of the texture and quad
    0e 1e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f      \ bottom left
    1e 1e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f     \ bottom right
    1e 0e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f        \ top right
    0e 0e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f         \ top left
    
    \ Back face of the texture and quad
    0e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f     \ bottom right
    0e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f        \ top right
    1e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f         \ top left
    1e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f      \ bottom left
  
    \ Top face of the texture and quad
    1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f         \ top left
    1e 0e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f      \ bottom left
    0e 0e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f     \ bottom right
    0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f        \ top right
  
    \ Bottom face of the texture and quad
    0e 1e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f        \ top right
    1e 1e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f         \ top left
    1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f      \ bottom left
    0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f     \ bottom right
  
    \ Right face of the texture and quad
    0e 0e gl-tex-coord-2f  1e -1e -1e gl-vertex-3f     \ bottom right
    0e 1e gl-tex-coord-2f  1e  1e -1e gl-vertex-3f        \ top right
    1e 1e gl-tex-coord-2f  1e  1e  1e gl-vertex-3f         \ top left
    1e 0e gl-tex-coord-2f  1e -1e  1e gl-vertex-3f      \ bottom left
  
    \ Left face of the texture and quad
    1e 0e gl-tex-coord-2f -1e -1e -1e gl-vertex-3f      \ bottom left
    0e 0e gl-tex-coord-2f -1e -1e  1e gl-vertex-3f     \ bottom right
    0e 1e gl-tex-coord-2f -1e  1e  1e gl-vertex-3f        \ top right
    1e 1e gl-tex-coord-2f -1e  1e -1e gl-vertex-3f         \ top left
  gl-end

  sdl-gl-swap-buffers                         \ Draw it to the screen
  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  xrot F@ 0.3e F+ xrot F!                 \ increment x axis rotation
  yrot F@ 0.2e F+ yrot F!                 \ increment y axis rotation
  zrot F@ 0.4e F+ zrot F!                 \ increment z axis rotation

  TRUE                                          \ Return a good value
;

