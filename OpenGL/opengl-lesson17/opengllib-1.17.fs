\ ===================================================================
\           File: opengllib-1.17.fs
\         Author: Giuseppe D'Agata
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/31/2010
\    Description: 2D texture mapped fonts
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 16
\ ===================================================================
\ This code was created by Giuseppe D'Agata
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

require mini-opengl-1.17.fs
require mini-sdl-1.01.fs
require sdlkeysym.fs

[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : KillFont                    ( -- )
\ : Generate-Texture            { *src -- }
\ : LoadGLTextures              ( -- boolean )
\ : BuildFont                   ( -- )
\ : glPrint                     { _x _y *str _len _set -- }  
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

2 constant NumTextures                     \ number of texture to use

variable baselist                \ base display list for the font set
fvariable count1       \ 1st counter used to move text and for coloring
fvariable count2       \ 2nd counter used to move text and for coloring

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

\ ---[ Variable Initializations ]------------------------------------

0 baselist !
0e count1 F!
0e count2 F!

\ ---[ KillFont ]----------------------------------------------------
\ Recover memory from our list of characters by deleting all 256 of
\ the display lists we created.

: KillFont ( -- )
  baselist @ 256 gl-delete-lists
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
\ create local variables for storing surface pointers and return flag
  0 0 FALSE { _tex1 _tex2 _status -- status }
  \ Attempt to load the texture images into SDL surfaces
  s" data/font.bmp0" zstring sdl-loadbmp dup to _tex1
  s" data/bumps.bmp0" zstring sdl-loadbmp dup to _tex2
  \ If either return value is 0 then the image did not load
  _tex1 0<> _tex2 0 <> AND if
    cr ." Texture Images loaded" cr
    TRUE to _status                                \ set return value

    NumTextures texture gl-gen-textures         \ create the textures

  \ Load the font image to texture 0
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture        \ texture[0]

    \ Generate texture 0
    _tex1 Generate-Texture

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

  \ Load the brick-ish image to texture 1
    GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture        \ texture[1]

    \ Linear Filtering
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i    
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i    

    \ Generate texture 1
    _tex2 Generate-Texture

  else
    cr ." Error: texture image could not be loaded!" cr
  then
  _tex1 0<> if _tex1 sdl-freesurface then
  _tex2 0<> if _tex2 sdl-freesurface then
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ BuildFont ]---------------------------------------------------
\ Function to build our OpenGL font list

\ ---[Note]----------------------------------------------------------
\ BMPs are stored with the top-leftmost pixel being the last byte and
\ the bottom-rightmost pixel being the first byte. So an image that
\ is displayed as
\
\               1 0
\               0 0
\
\ is represented data-wise like
\
\               0 0
\               0 1
\
\ And, because SDL_LoadBMP loads the raw data without translating to
\ how it is thought of when viewed, we need to start at the bottom
\ right corner of the data and work backwards to get everything
\ properly. So the below code has been modified to reflect this.
\ Examine how this is done and how the original tutorial is done to
\ grasp the differences.
\
\ As a side note BMPs are also stored as BGR instead of RGB and that
\ is why we load the texture using GL_BGR.
\
\ It's bass-ackwards I know but whattaya gonna do? -- Ti Leggett
\ ------------------------------------------------------[End Note]---

fvariable bf-cx                         \ holds our x character coord
fvariable bf-cy                         \ holds our y character coord

: BuildFont ( -- )
  \ Create 256 display lists
  256 gl-gen-lists baselist !
  \ Select our font texture
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
  \ Loop thru all 256 lists
  256 0 do
    \ X Position of current character
    1e i 16 MOD S>F 16e F/ F- bf-cx F!
    \ Y Position of current character
    1e i 16 / S>F 16e F/ F- bf-cy F!
    \ Start building a list
    baselist @ 255 i - + GL_COMPILE gl-new-list
      GL_QUADS gl-begin               \ use a quad for each character
        \ texture coordinate - bottom left
        bf-cx F@ 0.0625e F- bf-cy F@ gl-tex-coord-2f
        \ vertex coordinate - bottom left
        0 0 gl-vertex-2i
        \ texture coordinate - bottom right
        bf-cx F@ bf-cy F@ gl-tex-coord-2f
        \ vertex coordinate - bottom right
        16 0 gl-vertex-2i
        \ texture coordinate - top right
        bf-cx F@ bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top right
        16 16 gl-vertex-2i
        \ texture coordinate - top left
        bf-cx F@ 0.0625e F- bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top left
        0 16 gl-vertex-2i
      gl-end
      \ Move to the left of the character
      10e 0e 0e gl-translate-d
    gl-end-list
  loop
;

\ ---[ glPrint ]-----------------------------------------------------
\ Prints a string
\ The <set> parameter is 0 for Normal, or 1 for Italic

: glPrint { _x _y *str _len _set -- }  
  _set 1 > if 1 to _set then
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture  \ Select our texture
  GL_DEPTH_TEST gl-disable                    \ Disable depth testing
  GL_PROJECTION gl-matrix-mode         \ Select the projection matrix
  gl-push-matrix                        \ Store the projection matrix
  gl-load-identity                      \ Reset the projection matrix
  0e 640e 0e 480e -1e 1e gl-ortho            \ Set up an ortho screen
  GL_MODELVIEW gl-matrix-mode           \ Select the modelview matrix
  gl-push-matrix                         \ Store the modelview matrix
  gl-load-identity                       \ Reset the modelview matrix
  _x S>F _y S>F 0e gl-translate-d  \ Position text (0,0==bottom left)
  baselist @ 32 - 128 _set * + gl-list-base     \ Choose the font set
  _len GL_BYTE *str gl-call-lists                    \ Write the text
  GL_PROJECTION gl-matrix-mode         \ Select the projection matrix
  gl-pop-matrix                   \ Restore the old projection matrix
  GL_MODELVIEW gl-matrix-mode           \ Select the modelview matrix
  gl-pop-matrix                   \ Restore the old projection matrix
  GL_DEPTH_TEST gl-enable                   \ Re-enable depth testing
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
    \ Build our font lists
    BuildFont
    \ Enable smooth shading
    GL_SMOOTH gl-shade-model
    \ Set the background black
    0e 0e 0e 0e gl-clear-color
    \ Depth buffer setup
    1e gl-clear-depth
    \ Type of depth test to do
    GL_LEQUAL gl-depth-func
    \ Select the type of blending
    GL_SRC_ALPHA GL_ONE gl-blend-func
    \ Enable texture mapping
    GL_TEXTURE_2D gl-enable
    \ Return a good value
    TRUE
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  KillFont                                       \ clean up font list
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

  gl-load-identity

  \ Select our second texture
  GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture
  \ Move into the screen 5 units
  0e 0e -5e gl-translate-f
  \ Rotate on the Z axis 45 degrees - clockwise
  45e 0e 0e 1e gl-rotate-f
  \ Rotate on the x & y axes by count1 - left to right
  count1 F@ 30e F* 1e 1e 0e gl-rotate-f
  \ Disable blending before we draw in 3D
  GL_BLEND gl-disable
  
  1e 1e 1e gl-color-3f                                 \ bright white
  GL_QUADS gl-begin                \ draw the 1st texture mapped quad
    0e 0e gl-tex-coord-2f -1e  1e gl-vertex-2f   \ 1st texture/vertex
    1e 0e gl-tex-coord-2f  1e  1e gl-vertex-2f   \ 2nd texture/vertex
    1e 1e gl-tex-coord-2f  1e -1e gl-vertex-2f   \ 3rd texture/vertex
    0e 1e gl-tex-coord-2f -1e -1e gl-vertex-2f   \ 4th texture/vertex
  gl-end
  \ Rotate on the x & y axes by 90 degrees - left to right
  90e 1e 1e 0e gl-rotate-f
  GL_QUADS gl-begin                \ draw the 2nd texture mapped quad
    0e 0e gl-tex-coord-2f -1e  1e gl-vertex-2f   \ 1st texture/vertex
    1e 0e gl-tex-coord-2f  1e  1e gl-vertex-2f   \ 2nd texture/vertex
    1e 1e gl-tex-coord-2f  1e -1e gl-vertex-2f   \ 3rd texture/vertex
    0e 1e gl-tex-coord-2f -1e -1e gl-vertex-2f   \ 4th texture/vertex
  gl-end
  \ Re-enable blending
  GL_BLEND gl-enable
  \ Reset the view
  gl-load-identity
  \ Pulsing colors based on text position
  \ Print the GL text to the screen

  \ Set color for first text string
  1e count1 F@ FCOS F*
  1e count2 F@ FSIN F*
  1e 0.5e count1 F@ count2 F@ F+ FCOS F* F- gl-color-3f
  \ Print the first text string
  280e 250e count1 F@ FCOS F* F+ F>S
  235e 200e count2 F@ FSIN F* F+ F>S
  s" NeHe"
  0
  glPrint
  
  \ Set color for second text string
  1e count2 F@ FSIN F*
  1e 0.5e count1 F@ count2 F@ F+ FCOS F* F-
  1e count1 F@ FCOS F* gl-color-3f
  \ Print the second text string
  280e 230e count2 F@ FCOS F* F+ F>S
  235e 200e count1 F@ FSIN F* F+ F>S
  s" OpenGL"
  1
  glPrint
  
  \ Set color to red
  0e 0e 1e gl-color-3f
  \ Print the third string
  240e 200e count2 F@ count1 F@ F+ FCOS F* 5e F/ F+ F>S
  2
  s" Giuseppe D'Agata"
  0
  glPrint
  
  \ Set color to white
  1e 1e 1e gl-color-3f
  \ Print offset text to the screen
  242e 200e count2 F@ count1 F@ F+ FCOS F* 5e F/ F+ F>S
  2
  s" Giuseppe D'Agata"
  0
  glPrint
  
  \ Draw it to the screen -- if double buffering is permitted
  sdl-gl-swap-buffers

  count1 F@ 0.01e F+ count1 F!              \ increment first counter
  count2 F@ 0.0081e F+ count2 F!           \ increment second counter
  
  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  \ Return a good value
  TRUE
;

