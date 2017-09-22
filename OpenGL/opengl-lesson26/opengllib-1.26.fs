\ ===================================================================
\           File: opengllib-1.26.fs
\         Author: Banu Cosmin
\  Linux Version: Gray Fox
\ gForth Version: Timothy Trussell, 05/27/2011
\    Description: Clipping & Reflections Using The Stencil Buffer
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.32-31
\   C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 26
\ ===================================================================
\ This code was created by Jeff Molofee 2000
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ UseLibUtil ]--------------------------------------------------
\ Conditional compilation of the libcc interfaces:
\ -- Set to 1 if you use the LibUtil script to copy the files to
\    the ~/.gforth directory.
\ -- Set to 0 to use the files from the Lesson directory (PWD).

0 =: UseLibUtil

UseLibUtil [if]

require ~/.gforth/opengl-libs/mini-opengl-current.fs
require ~/.gforth/opengl-libs/mini-sdl-current.fs
require ~/.gforth/opengl-libs/sdlkeysym.fs

[else]

require mini-opengl-1.26.fs
require mini-sdl-1.02.fs
require sdlkeysym.fs

[then]

\ ============[ Additional Ancilliary Support Routines ]=============

\ ---[ String/Array Words ]------------------------------------------

\ s-new creates the base name, storing the element size in next cell
: s-new    ( size -- ) create , does> ;

\ s-alloc allots/clears a single element instance; returns address
: s-alloc  ( *base -- *t ) @ here dup rot dup allot 0 fill ;

\ s-alloc# allots/clears an array of elements; returns 1st address
: s-alloc# ( *base n -- *t ) swap @ * here dup rot dup allot 0 fill ;

\ s-ndx returns the n-th element address of an array
: s-ndx    ( *base n -- *str[n] ) over @ * CELL + + ;

\ s-token scans the source string for the first occurrence of the
\         character /c/ returning *str/len of the string
\         Succeeding calls using NULL as the source address will
\         continue from one byte past the last found address.

0 VALUE (token)                            \ addr of last token found

: s-token { *src _c -- *str len }
  *src 0= if (token) 1+ to *src then   \ if NULL passed use last addr
  0                               \ initial count value for this pass
  begin *src over + C@ dup _c = swap 0= OR if 1 else 1+ 0 then until
  *src over + to (token)                          \ set for next time
  *src swap
;

\ ---[ Floating Point +!/-! ]----------------------------------------
\ Floating point versions of the integer +! function

: F+! ( f: fval *fvar -- ) dup F@ FSWAP F+ F! ;
: F-! ( f: fval *fvar -- ) FNEGATE F+! ;

\ ---[ Prototype Listing ]-------------------------------------------
\ : LoadObj                     ( *str len -- *buf )
\ : ProcessObj                  ( *buf *dst *obj -- #verts )
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : Calculate                   ( ndx *v -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

3 =: NumImages                       \ number of images in the Lesson
3 =: NumTextures                   \ number of textures being created

0 VALUE quadratic                    \ Quadratic For Drawing A Sphere

FVARIABLE xrot                                           \ x rotation
FVARIABLE yrot                                           \ y rotation
FVARIABLE xspeed                                   \ x rotation speed
FVARIABLE yspeed                                   \ y rotation speed
FVARIABLE zoom                                \ depth into the screen
FVARIABLE ballheight                      \ height of ball from floor

\ Store the OpenGL Texture handles in the Texture[] array
CELL s-new Texture[]  Texture[]  NumImages   s-alloc# drop

\ Store the SDL Surface pointers in the TexImage[] array
CELL s-new TexImage[] TexImage[] NumTextures s-alloc# drop

\ Light Parameters - passed by address so they have to be 32-bit

create LightAmb[] 0.7e SF, 0.7e SF, 0.7e SF, 1.0e SF,       \ Ambient
create LightDif[] 1.0e SF, 1.0e SF, 1.0e SF, 1.0e SF,       \ Diffuse
create LightPos[] 4.0e SF, 4.0e SF, 6.0e SF, 1.0e SF,      \ Position

\ ---[ Variable Initializations ]------------------------------------

0e0 xrot F!
0e0 yrot F!
0e0 xspeed F!
0e0 yspeed F!
-7.0e zoom F!
2e0 ballheight F!

\ ===[ The code ]====================================================

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

\ ---[ Load-Image ]--------------------------------------------------
\ Attempt to load the texture images into SDL surfaces, saving the
\ result into the teximage[] array; Return TRUE if result from
\ sdl-loadbmp is <> 0; else return FALSE

: Load-Image ( str len ndx -- boolean )
  >R zstring sdl-loadbmp dup TexImage[] R> s-ndx ! 0<>
;

: LoadGLTextures ( -- status )
\ create local variable for storing return flag
  0 { _status -- status }
  \ Attempt to load the texture images into SDL surfaces
  s" data/Envwall.bmp0" 0 Load-Image 
  s" data/Ball.bmp0"    1 Load-Image AND
  s" data/Envroll.bmp0" 2 Load-Image AND if
    TRUE to _status                                \ set return value
    NumTextures Texture[] 0 s-ndx gl-gen-textures   \ create textures
    NumTextures 0 do
      \ Create Nearest Filtered Texture
      GL_TEXTURE_2D Texture[] i s-ndx @ gl-bind-texture
      TexImage[] i s-ndx @ Generate-Texture
     GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
     GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    loop
  else
    \ At least one of the images did not load, so exit
    cr ." Error: texture image could not be loaded!" cr
  then
  \ Free the image surfaces that were created
  NumImages 0 do
    TexImage[] i s-ndx @ dup 0<> if sdl-freesurface else drop then
  loop
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 VALUE key-ESC
0 VALUE key-F1
0 VALUE key-PgUp
0 VALUE key-PgDn
0 VALUE key-Up
0 VALUE key-Dn
0 VALUE key-Left
0 VALUE key-Right
0 VALUE key-a
0 VALUE key-z

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   PgUp     increase ball height
\   PgDn     decrease ball height
\   Up       increase x-speed
\   Down     decrease x-speed
\   Left     decrease y-speed
\   Right    increase y-speed
\   a        move object towards viewer
\   z        move object away from viewer

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_UP       of 0.08e0 xspeed F+! endof                \ +xspeed
    SDLK_DOWN     of 0.08e0 xspeed F-! endof                \ -xspeed
    SDLK_LEFT     of 0.08e0 yspeed F-! endof                \ -yspeed
    SDLK_RIGHT    of 0.08e0 yspeed F+! endof                \ +yspeed
    SDLK_PAGEUP   of 0.03e0 ballheight F+! endof            \ ball up
    SDLK_PAGEDOWN of 0.03e0 ballheight F-! endof          \ ball down
    SDLK_a        of 0.05e0 zoom F+! endof                  \ zoom in
    SDLK_z        of 0.05e0 zoom F-! endof                 \ zoom out
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
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
    SDLK_LEFT     of FALSE to key-Left  endof
    SDLK_RIGHT    of FALSE to key-Right endof
    SDLK_a        of FALSE to key-a     endof
    SDLK_z        of FALSE to key-z     endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    FALSE
  else
    GL_SMOOTH gl-shade-model            \ Enable smooth color shading
    0.2e 0.5e 1.0e 1.0e gl-clear-color     \ Set the background color
    1.0e gl-clear-depth        \ Enables clearing of the depth buffer
    0 gl-clear-stencil                \ clear the stencil buffer to 0
    GL_DEPTH_TEST gl-enable                   \ Enables depth testing
    GL_LEQUAL gl-depth-func                \ Type of depth test to do
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint    \ Perspective
    GL_TEXTURE_2D gl-enable               \ enable 2D texture mapping

    GL_LIGHT0 GL_AMBIENT LightAmb[] gl-light-fv   \ Set ambient light
    GL_LIGHT0 GL_DIFFUSE LightDif[] gl-light-fv   \ Set diffuse light
    GL_LIGHT0 GL_POSITION LightPos[] gl-light-fv \ Position the light
    GL_LIGHT0 gl-enable                           \ Enable Light Zero
    GL_LIGHTING gl-enable                           \ Enable Lighting

    glu-new-quadric to quadratic             \ Create a new quadratic
    quadratic GL_SMOOTH glu-quadric-normals \ generate smooth normals
    quadratic GL_TRUE glu-quadric-texture     \ enable texture coords
    
    GL_S GL_TEXTURE_GEN_MODE GL_SPHERE_MAP gl-tex-gen-i      \ set up
    GL_T GL_TEXTURE_GEN_MODE GL_SPHERE_MAP gl-tex-gen-i  \ sphere map
    
    TRUE                                        \ Return a good value
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  quadratic glu-delete-quadric              \ clean up our quadratics
  NumTextures Texture[] 0 s-ndx gl-delete-textures \ clean up texture
  sdl-quit                               \ close down the SDL systems
;

FVARIABLE fps-seconds
FVARIABLE fps-count
0 VALUE   fps-ticks
0 VALUE   fps-t0
0 VALUE   fps-frames
0 VALUE   fps-line

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

: DrawObject ( -- )
  1.0e 1.0e 1.0e gl-color-3f                     \ set color to white
  GL_TEXTURE_2D Texture[] 1 s-ndx @ gl-bind-texture
  quadratic 0.35e 32 16 glu-sphere                \ draw first sphere

  GL_TEXTURE_2D Texture[] 2 s-ndx @ gl-bind-texture
  1.0e 1.0e 1.0e 0.4e gl-color-4f     \ color is white with 40% alpha
  GL_BLEND gl-enable                                \ enable blending
  GL_SRC_ALPHA GL_ONE gl-blend-func               \ set blending mode
  GL_TEXTURE_GEN_S gl-enable                  \ enable sphere mapping
  GL_TEXTURE_GEN_T gl-enable                  \ enable sphere mapping
  
  quadratic 0.35e 32 16 glu-sphere              \ draw another sphere
  
  GL_TEXTURE_GEN_S gl-disable                \ disable sphere mapping
  GL_TEXTURE_GEN_T gl-disable                \ disable sphere mapping
  GL_BLEND gl-disable                              \ disable blending
;

: DrawFloor ( -- )
  GL_TEXTURE_2D Texture[] 0 s-ndx @ gl-bind-texture
  GL_QUADS gl-begin                            \ begin drawing a quad
    0.0e 1.0e 0.0e gl-normal-3f                  \ normal pointing up
    0.0e 1.0e gl-tex-coord-2f                \ bottom left of texture
    -2.0e 0.0e 2.0e gl-vertex-3f        \ bottom left corner of floor

    0.0e 0.0e gl-tex-coord-2f                   \ top left of texture
    -2.0e 0.0e -2.0e gl-vertex-3f          \ top left corner of floor

    1.0e 0.0e gl-tex-coord-2f                  \ top right of texture
    2.0e 0.0e -2.0e gl-vertex-3f          \ top right corner of floor

    1.0e 1.0e gl-tex-coord-2f               \ bottom right of texture
    2.0e 0.0e 2.0e gl-vertex-3f        \ bottom right corner of floor
  gl-end                                      \ done drawing the quad
;

\ Plane Equation for the reflected objects - C DOUBLEs specifically
create EQR[] 0.0e F, -1.0e F, 0.0e F, 0.0e F,

: DrawGLScene ( -- boolean )
  GL_COLOR_BUFFER_BIT 
  GL_DEPTH_BUFFER_BIT OR 
  GL_STENCIL_BUFFER_BIT OR gl-clear                    \ Clear screen
  gl-load-identity                                 \ Reset the matrix
  0.0e -0.6e zoom F@ gl-translate-f   \ zoom/raise camera above floor
  0 0 0 0 gl-color-mask                              \ set color mask
  GL_STENCIL_TEST gl-enable                   \ enable stencil buffer
  GL_ALWAYS 1 1 gl-stencil-func
  GL_KEEP GL_KEEP GL_REPLACE gl-stencil-op
  GL_DEPTH_TEST gl-disable                    \ disable depth testing
  DrawFloor                 \ draws the floor (to the stencil buffer)
  GL_DEPTH_TEST gl-enable                      \ enable depth testing
  1 1 1 1 gl-color-mask                     \ set color mask to trues
  GL_EQUAL 1 1 gl-stencil-func
  GL_KEEP GL_KEEP GL_KEEP gl-stencil-op \ no change to stencil buffer
  GL_CLIP_PLANE0 gl-enable    \ enable clip plane to remove artifacts
  GL_CLIP_PLANE0 EQR[] gl-clip-plane \ equation for reflected obejcts
  gl-push-matrix                      \ push the current matrix stack
    1.0e -1.0e 1.0e gl-scale-f                        \ mirror Y axis
    GL_LIGHT0 GL_POSITION LightPos[] gl-light-fv         \ set light0
    0.0e ballheight F@ 0.0e gl-translate-f      \ position the object
    xrot F@ 1.0e 0.0e 0.0e gl-rotate-f             \ rotate on x axis
    yrot F@ 0.0e 1.0e 0.0e gl-rotate-f             \ rotate on y axis
    DrawObject                         \ draw the sphere (reflection)
  gl-pop-matrix                         \ restore the previous matrix
  GL_CLIP_PLANE0 gl-disable        \ disable clip plane for the floor
  GL_STENCIL_TEST gl-disable             \ disable the stencil buffer
  GL_LIGHT0 GL_POSITION LightPos[] gl-light-fv           \ set Light0
  GL_BLEND gl-enable      \ enable blending so reflected object shows
  GL_LIGHTING gl-disable                    \ because we are blending
  1.0e 1.0e 1.0e 0.8e gl-color-4f \ set color to white with 80% alpha
  GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA gl-blend-func
  DrawFloor                                           \ to the screen
  GL_LIGHTING gl-enable                             \ enable lighting
  GL_BLEND gl-disable                \ position ball at proper height
  0.0e ballheight F@ 0.0e gl-translate-f        \ position the object
  xrot F@ 1.0e 0.0e 0.0e gl-rotate-f               \ rotate on x axis
  yrot F@ 0.0e 1.0e 0.0e gl-rotate-f               \ rotate on y axis
  DrawObject                           \ draw the sphere (reflection)
  xspeed F@ xrot F+!                        \ update x rotation angle
  yspeed F@ yrot F+!                        \ update y rotation angle
  gl-flush                                    \ flush the GL pipeline

  \ Draw it to the screen -- if double buffering is permitted
  \ The Lesson code actually calls glutSwapBuffers which I do not
  \ have coded in my system - as yet...
  
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window
  
  \ Return a good value
  TRUE
;
