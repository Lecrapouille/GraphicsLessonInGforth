\ ===================================================================
\           File: opengllib-1.25.fs
\         Author: Piotr Cieslak
\  Linux Version: DarkAlloy
\ gForth Version: Timothy Trussell, 05/25/2011
\    Description: Morphing, loading objects
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.32-31
\   C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 25
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

require mini-opengl-1.25.fs
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

\ ---[ RANDOM NUMBERS IN FORTH ]-------------------------------------
\  D. H. Lehmers Parametric multiplicative linear congruential
\  random number generator is implemented as outlined in the
\  October 1988 Communications of the ACM ( V 31 N 10 page 1192)
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

     16807 =: (A)
2147483647 =: (M)
    127773 =: (Q)   \ m a /
      2836 =: (R)   \ m a mod

CREATE (SEED)  123475689 ,

\ Returns a full cycle random number
: RAND ( -- rand )                        \ 0 <= rand < 4,294,967,295
   (SEED) @ (Q) /MOD ( lo high)
   (R) * SWAP (A) * 2DUP > IF - ELSE - (M) + THEN  DUP (SEED) ! ;

\ Returns single random number less than n
: RND ( n -- rnd ) RAND SWAP MOD ;                     \ 0 <= rnd < n

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

    0 VALUE step                              \ morphing step counter
  200 VALUE steps                  \ maximum number of morphing steps
FALSE VALUE morph                              \ is morphing enabled?
    0 VALUE *sour                          \ pointer to source object
    0 VALUE *dest                     \ pointer to destination object
    0 VALUE maxver                       \ maximum number of vertices

FVARIABLE xrot                                           \ x rotation
FVARIABLE yrot                                           \ y rotation
FVARIABLE zrot                                           \ z rotation
FVARIABLE xspeed                                   \ x rotation speed
FVARIABLE yspeed                                   \ y rotation speed
FVARIABLE zspeed                                   \ z rotation speed
FVARIABLE cx                                             \ x position
FVARIABLE cy                                             \ y position
FVARIABLE cz                                             \ z position

struct
  float% field .xcoord
  float% field .ycoord
  float% field .zcoord
end-struct vertex%

struct
  cell% field .verts             \ number of verrtices for the object
  ptr%  field .points                  \ pointer to array of vertices
end-struct object%

object% %allot =: morph1                 \ the four morphable objects
object% %allot =: morph2
object% %allot =: morph3
object% %allot =: morph4
object% %allot =: helper                            \ a helper object

\ Create and allocate the data spaces for the image data points.
\ This uses the String/Array functions to first create a base entry
\ using the s-new function, and then allocates and zeroes the space
\ for a specified number of elements in the array (486 in this case)
\ using the s-alloc# function.
\
\ A specific array element is then accessed using the s-ndx function
\ by specifying the base entry name and the index number to find:
\
\    486 0 do
\      Sphere[] i s-ndx          ( -- *Sphere[i] )
\      ( do something with the data )
\      dup .xcoord F@
\      dup .ycoord F@
\          .zcoord F@ gl-vertex-3f
\    loop

vertex% nip s-new Sphere[]         \ create a String/Array base entry
Sphere[] 486 s-alloc# drop      \ allocate the data space immediately

vertex% nip s-new Torus[]          Torus[]  486 s-alloc# drop
vertex% nip s-new Tube[]           Tube[]   486 s-alloc# drop
vertex% nip s-new Stars[]          Stars[]  486 s-alloc# drop
vertex% nip s-new Helper[]         Helper[] 486 s-alloc# drop

\ ---[ Variable Initializations ]------------------------------------

-15e0 cz F!

\ ===[ The code ]====================================================

\ ---[ LoadObj ]-----------------------------------------------------
\ Loads the specified data file into a temp buffer above <here>

: LoadObj ( *str len -- *buf )
  0 0 0 0 { *str _len _fh *buf *src #src -- *buf }
  here 65536 MOD 65536 swap - here + to *buf   \ set temp buffer addr
  *buf 65536 255 fill                            \ zero the temp buffer
  *buf to *src
  *str _len r/o open-file throw to _fh             \ open source file
  begin
    *src 4096 _fh read-file throw dup *src + to *src 0=
  until
  *src *buf - to #src               \ calculate length of data loaded
  _fh close-file throw                            \ close source file
  *buf                                        \ return buffer address
;

\ ---[ ProcessObj ]--------------------------------------------------
\ Converts the object data from text to floating point, and then
\ stores the FP numbers to the *dst array.
\ The destination array is a String/Array construct.
  
: ProcessObj ( *buf *dst *obj -- #verts )
  0 { *buf *dst *obj #verts -- #verts }
  *buf $20 s-token + 1+ $0D s-token
  0.0 2OVER >number 2DROP DROP DUP to #verts  \ get #vertices
  ( #verts ) 0 do
    + 2 +                      \ advance to start of next vertice set
    $20 s-token 2DUP >float drop *dst i s-ndx .xcoord F! + 5 +
    $20 s-token 2DUP >float drop *dst i s-ndx .ycoord F! + 5 +
    $0D s-token 2DUP >float drop *dst i s-ndx .zcoord F!
  loop
  2DROP                                 \ drop addr/len of last found
  #verts maxver > if #verts to maxver then            \ update maxver
  #verts *obj .verts !                         \ set #verts in object
  *dst *obj .points !                  \ set vertex address in object
;

s" data/Sphere.txt" LoadObj ( *buf ) Sphere[] morph1 ProcessObj
s" data/Torus.txt"  LoadObj ( *buf ) Torus[]  morph2 ProcessObj
s" data/Tube.txt"   LoadObj ( *buf ) Tube[]   morph3 ProcessObj
s" data/Sphere.txt" LoadObj ( *buf ) Helper[] helper ProcessObj

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 VALUE key-ESC
0 VALUE key-F1
0 VALUE key-Space
0 VALUE key-PgUp
0 VALUE key-PgDn
0 VALUE key-Up
0 VALUE key-Dn
0 VALUE key-Left
0 VALUE key-Right
0 VALUE key-q
0 VALUE key-z
0 VALUE key-w
0 VALUE key-s
0 VALUE key-d
0 VALUE key-a
0 VALUE key-1
0 VALUE key-2
0 VALUE key-3
0 VALUE key-4

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   PgUp     increase z-speed
\   PgDn     decrease z-speed
\   Up       decrease x-speed
\   Down     increase x-speed
\   Left     decrease y-speed
\   Right    increase y-speed
\   d        move object right
\   a        move object left
\   w        move object up
\   s        move object down
\   z        move object towards viewer
\   q        move object away from viewer
\   1..4     morphs to different objects

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_SPACE    of key-Space FALSE = if  \ resets the display fvars
                      TRUE to key-Space        \ set key pressed flag
                      0e0 xrot F!
                      0e0 yrot F!
                      0e0 zrot F!
                      0e0 xspeed F!
                      0e0 yspeed F!
                      0e0 zspeed F!
                      0e0 cx F!
                      0e0 cy F!
                      -15e0 cz F!
                    then
                  endof
    SDLK_PAGEDOWN of 0.01e0 zspeed F-! endof                \ -zspeed
    SDLK_PAGEUP   of 0.01e0 zspeed F+! endof                \ +zspeed
    SDLK_UP       of 0.01e0 xspeed F-! endof                \ -xspeed
    SDLK_DOWN     of 0.01e0 xspeed F+! endof                \ +xspeed
    SDLK_LEFT     of 0.01e0 yspeed F-! endof                \ -yspeed
    SDLK_RIGHT    of 0.01e0 yspeed F+! endof                \ +yspeed

    SDLK_d        of 0.01e0 cx F+! endof                 \ move right
    SDLK_a        of 0.01e0 cx F-! endof                  \ move left
    SDLK_w        of 0.01e0 cy F+! endof                    \ move up
    SDLK_s        of 0.01e0 cy F-! endof                  \ move down
    SDLK_z        of 0.01e0 cz F+! endof                \ move closer
    SDLK_q        of 0.01e0 cz F-! endof                  \ move away

    SDLK_1        of key-1 FALSE = morph FALSE = AND if
                       TRUE to morph         \ start morphing process
                       morph1 to *dest             \ set *dest object
                       TRUE to key-1           \ set key pressed flag
                     then
                  endof
    SDLK_2        of key-2 FALSE = morph FALSE = AND if
                       TRUE to morph         \ start morphing process
                       morph2 to *dest             \ set *dest object
                       TRUE to key-2           \ set key pressed flag
                     then
                  endof
    SDLK_3        of key-3 FALSE = morph FALSE = AND if
                       TRUE to morph         \ start morphing process
                       morph3 to *dest             \ set *dest object
                       TRUE to key-3           \ set key pressed flag
                     then
                  endof
    SDLK_4        of key-4 FALSE = morph FALSE = AND if
                       TRUE to morph         \ start morphing process
                       morph4 to *dest             \ set *dest object
                       TRUE to key-4           \ set key pressed flag
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
    SDLK_SPACE    of FALSE to key-Space endof
    SDLK_PAGEUP   of FALSE to key-PgUp  endof
    SDLK_PAGEDOWN of FALSE to key-PgDn  endof
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
    SDLK_LEFT     of FALSE to key-Left  endof
    SDLK_RIGHT    of FALSE to key-Right endof
    SDLK_q        of FALSE to key-q     endof
    SDLK_z        of FALSE to key-z     endof
    SDLK_w        of FALSE to key-w     endof
    SDLK_s        of FALSE to key-s     endof
    SDLK_d        of FALSE to key-d     endof
    SDLK_a        of FALSE to key-a     endof
    SDLK_1        of FALSE to key-1     endof
    SDLK_2        of FALSE to key-2     endof
    SDLK_3        of FALSE to key-3     endof
    SDLK_4        of FALSE to key-4     endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  GL_SRC_ALPHA GL_ONE gl-blend-func   \ Set blending for translucency
  0e 0e 0e 0e gl-clear-color               \ Set the background black
  1e gl-clear-depth            \ Enables clearing of the depth buffer
  GL_LESS gl-depth-func                    \ Type of depth test to do
  GL_DEPTH_TEST gl-enable                     \ Enables depth testing
  GL_SMOOTH gl-shade-model              \ Enable smooth color shading
  GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint      \ Perspective

  \ Initialize the morph4 object data (stars)
  486 0 do            \ all x/y/z points are a random float in -7..+7
    14000 RND S>F 1000e F/ 7e F- Stars[] i s-ndx .xcoord F!
    14000 RND S>F 1000e F/ 7e F- Stars[] i s-ndx .ycoord F!
    14000 RND S>F 1000e F/ 7e F- Stars[] i s-ndx .zcoord F!
  loop

  486 morph4 .verts ! Stars[] morph4 .points !

  \ Source & Destination are set to the first object (morph1)
  morph1 to *sour
  morph1 to *dest
  
  \ Return a good value
  TRUE
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
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

\ ---[ Calculate ]---------------------------------------------------
\ Calculates movement of points during morphing
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ Changed to add the destination VERTEX* in the call
\ Considering simply putting the x/y/z values on the FP stack
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

: Calculate ( ndx *v -- )
  0 0 { _ndx *v *src *dst -- }
  *sour .points @ _ndx s-ndx to *src             \ calc base pointers
  *dest .points @ _ndx s-ndx to *dst
  *src .xcoord F@ *dst .xcoord F@ F- steps S>F F/ *v .xcoord F!
  *src .ycoord F@ *dst .ycoord F@ F- steps S>F F/ *v .ycoord F!
  *src .zcoord F@ *dst .zcoord F@ F- steps S>F F/ *v .zcoord F!
;

\ ---[ DrawGLScene ]-------------------------------------------------

FVARIABLE tx                              \ temporary x/y/z variables
FVARIABLE ty
FVARIABLE tz
vertex% %allot =: qtemp               \ temporary vertex storage area

: DrawGLScene ( -- boolean )
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear  \ Clear screen
  gl-load-identity                                 \ Reset the matrix
  cx F@ cy F@ cz F@ gl-translate-f       \ translate display position

  xrot F@ 1e 0e 0e gl-rotate-f                 \ rotate on the x axis
  yrot F@ 0e 1e 0e gl-rotate-f                 \ rotate on the y axis
  zrot F@ 0e 0e 1e gl-rotate-f                 \ rotate on the z axis

  xspeed F@ xrot F+!                   \ Increase the rotation values
  yspeed F@ yrot F+!
  zspeed F@ zrot F+!
  
  GL_POINTS gl-begin
    486 0 do
      morph if
        i qtemp Calculate
      else
        0e0 qtemp .xcoord F!
        0e0 qtemp .ycoord F!
        0e0 qtemp .zcoord F!
      then

      helper .points @ i s-ndx >R
      qtemp .xcoord F@ R@ .xcoord F-! R@ .xcoord F@ tx F!
      qtemp .ycoord F@ R@ .ycoord F-! R@ .ycoord F@ ty F!
      qtemp .zcoord F@ R@ .zcoord F-! R> .zcoord F@ tz F!

      0e 1e 1e gl-color-3f                    \ set color to off-blue
      tx F@ ty F@ tz F@ gl-vertex-3f     \ draw a point at temp x/y/z

      0e 0.5e 1e gl-color-3f                 \ darken the color a bit
      2e0 qtemp .xcoord F@ F* tx F-!             \ calc two positions
      2e0 qtemp .ycoord F@ F* ty F-!                          \ ahead
      2e0 qtemp .ycoord F@ F* ty F-!
      tx F@ ty F@ tz F@ gl-vertex-3f            \ draw a second point

      0e 0e 1e gl-color-3f              \ set color to very dark blue
      2e0 qtemp .xcoord F@ F* tx F-!             \ calc two positions
      2e0 qtemp .ycoord F@ F* ty F-!                          \ ahead
      2e0 qtemp .ycoord F@ F* ty F-!                          \ again
      tx F@ ty F@ tz F@ gl-vertex-3f             \ draw a third point
    loop
  gl-end

  \ If we're morphing and we have not gone through all 200 steps
  \ increase our step counter; otherwise set morphing to false, make
  \ Source=Destination and set the step counter back to zero.
  
  step steps < morph AND if
    step 1+ to step
  else
    FALSE to morph
    *dest to *sour
    0 to step
  then

  \ Draw it to the screen -- if double buffering is permitted
  sdl-gl-swap-buffers

  fps-frames 1+ to fps-frames    \ Gather our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window
  
  \ Return a good value
  TRUE
;
