\ ===================================================================
\           File: opengllib-1.20.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 08/01/2010
\    Description: Masking
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 20
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

require mini-opengl-1.20.fs
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

5 constant NumTextures                    \ number of textures to use

fvariable xrot                                           \ X rotation
fvariable yrot                                           \ Y rotation
fvariable zrot                                           \ Z rotation
fvariable rolling                                   \ rolling texture

TRUE value masking                              \ masking toggle flag
FALSE value scene                                 \ scene toggle flag

\ ---[ Variable Initializations ]------------------------------------

0e xrot F!
0e yrot F!
0e zrot F!
0e rolling F!

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;


\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as a GL texture

create TextureImage[] here NumTextures cells dup allot 0 fill

: teximage-ndx ( n -- *TextureImage[n] ) cells TextureImage[] + ;

: Generate-Texture { *src -- }
  GL_TEXTURE_2D 0 3
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0 GL_BGR                                \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  gl-tex-image-2d                               \ finally generate it
;

\ Attempt to load the texture images into SDL surfaces, saving the
\ result into the TextureImage[] array; Return TRUE if result from
\ sdl-loadbmp is <> 0; else return FALSE

: load-image ( str len ndx -- boolean )
  >R zstring sdl-loadbmp dup R> teximage-ndx ! 0<>
;

: LoadGLTextures ( -- status )
\ create local variable for storing return flag and SDL surfaces
  0 { _status -- status }
  s" data/logo.bmp0"   0 load-image 
  s" data/mask1.bmp0"  1 load-image AND
  s" data/image1.bmp0" 2 load-image AND
  s" data/mask2.bmp0"  3 load-image AND
  s" data/image2.bmp0" 4 load-image AND if
    \ All the images loaded, so continue
    TRUE to _status                                \ set return value

    NumTextures texture gl-gen-textures         \ create the textures

    \ Process one image at a time into a texture
    NumTextures 0 do
      GL_TEXTURE_2D i texture-ndx @ gl-bind-texture \ load texture[i]
      i teximage-ndx @ Generate-Texture         \ Generate texture[i]
      \ Nearest Filtering
      GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR
      gl-tex-parameter-i
      GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR
      gl-tex-parameter-i
    loop
    
  else
    \ At least one of the images did not load, so exit
    cr ." Error: texture image could not be loaded!" cr
  then
  \ Free the image surfaces that were created
  NumTextures 0 do
    i teximage-ndx @ dup 0<> if sdl-freesurface else drop then
  loop
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 value key-ESC
0 value key-F1
0 value key-m
0 value key-Space

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   m        toggles masking
\   SPACE    toggles the scene to display

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_m        of key-m FALSE = if       \ skip if being held down
                       masking if 0 else 1 then to masking
                       TRUE to key-m           \ set key pressed flag
                     then
                  endof
    SDLK_SPACE    of key-Space FALSE = if   \ skip if being held down
                       scene if 0 else 1 then to scene
                       TRUE to key-Space       \ set key pressed flag
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
    SDLK_ESCAPE of FALSE to key-ESC   endof
    SDLK_F1     of FALSE to key-F1    endof
    SDLK_m      of FALSE to key-m     endof
    SDLK_SPACE  of FALSE to key-Space endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    \ Return a bad value
    FALSE
  else
    0e 0e 0e 0.5e gl-clear-color           \ Set the background black
    1e gl-clear-depth                            \ Depth buffer setup
    GL_DEPTH_TEST gl-enable                    \ Enable depth testing
    GL_SMOOTH gl-shade-model                  \ Enable smooth shading
    GL_TEXTURE_2D gl-enable                  \ Enable texture mapping
    TRUE                                        \ Return a good value
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

: -rolling rolling F@ FNEGATE ;

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear

  gl-load-identity                                   \ restore matrix

  0e 0e -2e gl-translate-f             \ Move into the screen 2 units

  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture    \ set logo texture

  GL_QUADS gl-begin                            \ draw a textured quad
    0e -rolling 3e F+ gl-tex-coord-2f -1.1e -1.1e 0e gl-vertex-3f
    3e -rolling 3e F+ gl-tex-coord-2f  1.1e -1.1e 0e gl-vertex-3f
    3e -rolling       gl-tex-coord-2f  1.1e  1.1e 0e gl-vertex-3f
    0e -rolling       gl-tex-coord-2f -1.1e  1.1e 0e gl-vertex-3f
  gl-end

  GL_BLEND gl-enable                                \ enable blending
  GL_DEPTH_TEST gl-disable                    \ disable depth testing

  masking if                                    \ is masking enabled?
    GL_DST_COLOR GL_ZERO gl-blend-func
  then
  
  scene if                                   \ draw the second scene?
    0e 0e -1e gl-translate-f       \ Translate into the screen 1 unit
    \ Rotate on the Z axis 360 degrees
    rolling F@ 360e F* 0e 0e 1e gl-rotate-f

    masking if
      \ Select the second mask texture
      GL_TEXTURE_2D 3 texture-ndx @ gl-bind-texture
      GL_QUADS gl-begin                        \ draw a textured quad
        0e 1e gl-tex-coord-2f -1.1e -1.1e 0e gl-vertex-3f
        1e 1e gl-tex-coord-2f  1.1e -1.1e 0e gl-vertex-3f
        1e 0e gl-tex-coord-2f  1.1e  1.1e 0e gl-vertex-3f
        0e 0e gl-tex-coord-2f -1.1e  1.1e 0e gl-vertex-3f
      gl-end
    then

    \ Copy Image 2 Color To The Screen
    GL_ONE GL_ONE gl-blend-func
    \ Select The Second Image Texture
    GL_TEXTURE_2D 4 texture-ndx @ gl-bind-texture
    \ Start Drawing A Textured Quad
    GL_QUADS gl-begin                          \ draw a textured quad
      0e 1e gl-tex-coord-2f -1.1e -1.1e 0e gl-vertex-3f
      1e 1e gl-tex-coord-2f  1.1e -1.1e 0e gl-vertex-3f
      1e 0e gl-tex-coord-2f  1.1e  1.1e 0e gl-vertex-3f
      0e 0e gl-tex-coord-2f -1.1e  1.1e 0e gl-vertex-3f
    gl-end
  else
    masking if
      \ Select the first mask texture
      GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture
      GL_QUADS gl-begin                        \ draw a textured quad
        rolling F@       4e gl-tex-coord-2f
        -1.1e -1.1e 0e gl-vertex-3f
        rolling F@ 4e F+ 4e gl-tex-coord-2f
         1.1e -1.1e 0e gl-vertex-3f
        rolling F@ 4e F+ 0e gl-tex-coord-2f
         1.1e  1.1e 0e gl-vertex-3f
        rolling F@       0e gl-tex-coord-2f
        -1.1e  1.1e 0e gl-vertex-3f
      gl-end
    then
    \ Copy Image 1 Color To The Screen
    GL_ONE GL_ONE gl-blend-func
    \ Select The First Image Texture
    GL_TEXTURE_2D 2 texture-ndx @ gl-bind-texture
    \ Start Drawing A Textured Quad
    GL_QUADS gl-begin                          \ draw a textured quad
      rolling F@       4e gl-tex-coord-2f -1.1e -1.1e 0e gl-vertex-3f
      rolling F@ 4e F+ 4e gl-tex-coord-2f  1.1e -1.1e 0e gl-vertex-3f
      rolling F@ 4e F+ 0e gl-tex-coord-2f  1.1e  1.1e 0e gl-vertex-3f
      rolling F@       0e gl-tex-coord-2f -1.1e  1.1e 0e gl-vertex-3f
    gl-end
  then

  GL_DEPTH_TEST gl-enable                      \ enable depth testing
  GL_BLEND gl-disable                              \ disable blending

  \ Increase our texture rolling variable
  rolling F@ 0.002e F+ FDUP 1e F> if 1e F- then rolling F!

  \ Draw it to the screen
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  xrot F@ 0.3e F+ xrot F!                 \ increment x axis rotation
  yrot F@ 0.2e F+ yrot F!                 \ increment y axis rotation
  zrot F@ 0.4e F+ zrot F!                 \ increment z axis rotation

  \ Return a good value
  TRUE
;

