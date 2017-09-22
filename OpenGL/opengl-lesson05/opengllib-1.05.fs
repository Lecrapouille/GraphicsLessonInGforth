\ ===[ Code Addendum 02 ]============================================
\                 gforth: OpenGL Graphics Lesson 05
\ ===================================================================
\           File: opengllib-1.05.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/07/2010
\    Description: 3D Shapes
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 05
\ ===================================================================
\ This code was created by Jeff Molofee '99 
\ (ported to Linux/SDL by Ti Leggett '01)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

require ~/.gforth/opengl-libs/mini-opengl-current.fs
require ~/.gforth/opengl-libs/mini-sdl-1.00.fs
require ~/.gforth/opengl-libs/sdlkeysym.fs

\ ---[ Prototype Listing ]-------------------------------------------
\ : HandleKeyPress              ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Rotation Variables ]------------------------------------------

fvariable r-tri
fvariable r-quad

0e r-tri F!
0e r-quad F!

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

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
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
  \ Return a good value
  TRUE
;
  
\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset these flag for next time
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

  \ Move left 1.5 units, and into the screen 6.0
  -1.5e 0e -6e gl-translate-f

  \ Rotate The Triangle On The Y axis ( NEW )
  r-tri F@ 0e 1e 0e gl-rotate-f

  GL_TRIANGLES gl-begin                     \ drawing using triangles
     1e  0e  0e gl-color-3f                                     \ red
     0e  1e  0e gl-vertex-3f                  \ top of triangle front
     0e  1e  0e gl-color-3f                                   \ green
    -1e -1e  1e gl-vertex-3f                 \ left of triangle front
     0e  0e  1e gl-color-3f                                    \ blue
     1e -1e  1e gl-vertex-3f                \ right of triangle front

     1e  0e  0e gl-color-3f                                     \ red
     0e  1e  0e gl-vertex-3f                  \ top of triangle right
     0e  0e  1e gl-color-3f                                   \ green
     1e -1e  1e gl-vertex-3f                 \ left of triangle right
     0e  1e  0e gl-color-3f                                    \ blue
     1e -1e -1e gl-vertex-3f                \ right of triangle right

     1e  0e  0e gl-color-3f                                     \ red
     0e  1e  0e gl-vertex-3f                   \ top of triangle back
     0e  1e  0e gl-color-3f                                   \ green
     1e -1e -1e gl-vertex-3f                  \ left of triangle back
     0e  0e  1e gl-color-3f                                    \ blue
    -1e -1e -1e gl-vertex-3f                 \ right of triangle back

     1e  0e  0e gl-color-3f                                     \ red
     0e  1e  0e gl-vertex-3f                   \ top of triangle left
     0e  0e  1e gl-color-3f                                   \ green
    -1e -1e -1e gl-vertex-3f                  \ left of triangle left
     0e  1e  0e gl-color-3f                                    \ blue
    -1e -1e  1e gl-vertex-3f                 \ right of triangle left
  gl-end                              \ finished drawing the triangle

  gl-load-identity                                   \ restore matrix

  \ Move right 3 units
  1.5e 0e -6e gl-translate-f

  \ Rotate The Quad On The X axis ( NEW )
  r-quad F@ 1e 0e 0e gl-rotate-f

  0.5e 0.5e 1e gl-color-3f      \ Set The Color To Blue One Time Only

  GL_QUADS gl-begin                                     \ draw a quad
     0e   1e  0e gl-color-3f                                  \ green
     1e   1e -1e gl-vertex-3f                             \ top right
    -1e   1e -1e gl-vertex-3f                              \ top left
    -1e   1e  1e gl-vertex-3f                           \ bottom left
     1e   1e  1e gl-vertex-3f                          \ bottom right

     1e 0.5e  0e gl-color-3f                                 \ orange
     1e  -1e  1e gl-vertex-3f                             \ top right
    -1e  -1e  1e gl-vertex-3f                              \ top left
    -1e  -1e -1e gl-vertex-3f                           \ bottom left
     1e  -1e -1e gl-vertex-3f                          \ bottom right

     1e  0e  0e gl-color-3f                                    \ red
     1e  1e  1e gl-vertex-3f                              \ top right
    -1e  1e  1e gl-vertex-3f                               \ top left
    -1e -1e  1e gl-vertex-3f                            \ bottom left
     1e -1e  1e gl-vertex-3f                           \ bottom right

     1e  1e  0e gl-color-3f                                  \ yellow
     1e -1e -1e gl-vertex-3f                              \ top right
    -1e -1e -1e gl-vertex-3f                               \ top left
    -1e  1e -1e gl-vertex-3f                            \ bottom left
     1e  1e -1e gl-vertex-3f                           \ bottom right

     0e  0e  1e gl-color-3f                                    \ blue
    -1e  1e  1e gl-vertex-3f                              \ top right
    -1e  1e -1e gl-vertex-3f                               \ top left
    -1e -1e -1e gl-vertex-3f                            \ bottom left
    -1e -1e  1e gl-vertex-3f                           \ bottom right

     1e  0e  1e gl-color-3f                                  \ violet
     1e  1e -1e gl-vertex-3f                              \ top right
     1e  1e  1e gl-vertex-3f                               \ top left
     1e -1e  1e gl-vertex-3f                            \ bottom left
     1e -1e -1e gl-vertex-3f                           \ bottom right
  gl-end

  \ Draw it to the screen
  sdl-gl-swap-buffers

  \ Gather  our frames per second count
  fps-frames 1+ to fps-frames

  \ Display the FPS count to the terminal window
  Display-FPS

  \ Increase The Rotation Variable For The Triangle
  r-tri F@ 0.2e F+ r-tri F!
  \ Decrease The Rotation Variable For The Quad
  r-quad F@ 0.15e F- r-quad F!

  \ Return a good value
  TRUE
;
