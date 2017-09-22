\ ===================================================================
\           File: opengllib-1.21.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 08/01/2010
\    Description: Lines, timing, ortho, sound
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 21
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
  require ~/.gforth/opengl-libs/mini-mixer-current.fs
[else]
  require mini-opengl-1.21.fs
  require mini-sdl-1.01.fs
  require sdlkeysym.fs
  require mini-mixer-1.00.fs
[then]

require random.fs

\ ---[ Prototype Listing ]-------------------------------------------
\ : Generate-Texture            { *src -- }
\ : LoadGLTextures              ( -- boolean )
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : BuildFont                   ( -- )
\ : glPrint                     { _x _y *str _len _set -- }  
\ : InitGL                      ( -- boolean )
\ : PlaySound                   { *sound _len _repeat -- }
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variables ]---------------------------------------------------

2 constant NumTextures                    \ number of textures to use

0 value grid-filled                       \ done filling in the grid?
TRUE value gameover                               \ is the game over?
TRUE value anti                  \ use anti-aliasing to smooth lines?

0 value enemy-delay
3 value speed-adjust                    \ for really slow video cards
5 value player-lives
1 value internal-level                          \ internal game level
1 value displayed-level                        \ displayed game level
1 value game-stage

variable baselist                    \ base display list for the font

0 value Chunk                                                 \ audio
0 value Music                                                 \ audio

\ Create a structure for our player

struct
  cell%  field .fx                           \ fine movement position
  cell%  field .fy
  cell%  field .x                           \ current player position
  cell%  field .y
  float% field .spin                                 \ spin direction
end-struct object%

\ Allocate memory space for the following structures
object% %allot constant player                   \ player information
object% 9 * %allot constant enemies[]             \ enemy information
object% %allot constant hourglass

\ stepping values for slow video adjustment
create steps[] 1 , 2 , 4 , 5 , 10 , 20 ,

create hline[] here 11 cells 11 * dup allot 0 fill
create vline[] here 11 cells 11 * dup allot 0 fill

\ allocate space for <NumTextures> texture pointers
create texture here NumTextures cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: enemies-ndx ( n -- *enemies[n] ) 9 MOD object% nip * enemies[] + ;
: steps-ndx ( n -- *steps[n] )     6 MOD cells steps[] + ;
: hline-ndx ( x y -- *hline[n] )  11 cells * swap cells + hline[] + ;
: vline-ndx ( x y -- *vline[n] )  11 cells * swap cells + vline[] + ;
: texture-ndx ( n -- *texture[n] ) NumTextures MOD cells texture + ;

: ResetObjects ( -- )
  0 player .x !            \ reset player x to far left of the screen
  0 player .y !                     \ reset player y to top of screen
  0 player .fx !                            \ set fine x pos to match
  0 player .fy !                            \ set fine y pos to match
  \ loop thru all the enemies
  game-stage internal-level * 0 do
    \ Set random X position             Set fine x to match
    6 random 5 + dup i enemies-ndx .x ! 60 * i enemies-ndx .fx !
    \ Set random y position             Set fine y to match
    11 random    dup i enemies-ndx .y ! 40 * i enemies-ndx .fy !
  loop
;

\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as a GL texture

\ TextureImage[] - an array to store SDL surface pointers to
create TextureImage[] here NumTextures cells dup allot 0 fill

: teximage-ndx ( n -- *TextureImage[n] ) cells TextureImage[] + ;

: Generate-Texture { *src -- }
  GL_TEXTURE_2D 0 3
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0 GL_RGB                                \ pixel mapping orientation
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
  FALSE { _status -- status }
  TextureImage[] NumTextures cells 0 fill       \ erase data pointers
  s" data/font.bmp0"   0 load-image 
  s" data/image.bmp0"  1 load-image AND if
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
0 value key-a
0 value key-Space
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   a        toggles anti-aliasing
\   SPACE    starts the game
\   Right    moves player right
\   Left     moves player left
\   Up       moves player up
\   Down     moves player down

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE of TRUE to opengl-exit-flag endof
    SDLK_F1     of key-F1 FALSE = if        \ skip if being held down
                     screen sdl-wm-togglefullscreen drop
                     TRUE to key-F1            \ set key pressed flag
                   then
                endof
    SDLK_a      of key-a FALSE = if         \ skip if being held down
                     anti if 0 else 1 then to anti
                     TRUE to key-a             \ set key pressed flag
                   then
                endof
    SDLK_SPACE  of key-Space FALSE = if     \ skip if being held down
                     gameover if
                       FALSE to gameover
                       TRUE to grid-filled
                       1 to internal-level
                       1 to displayed-level
                       1 to game-stage
                       5 to player-lives
                     then
                     TRUE to key-Space
                   then
                endof
    SDLK_RIGHT  of player .x @ 10 < 
                   player .fx @ player .x @ 60 * = AND
                   player .fy @ player .y @ 40 * = AND if
                     \ Mark The Current Horizontal Border As Filled
                     TRUE player .x @ player .y @ hline-ndx !
                     1 player .x +!               \ move player right
                   then
                endof
    SDLK_LEFT   of player .x @ 0>
                   player .fx @ player .x @ 60 * = AND
                   player .fy @ player .y @ 40 * = AND if
                     -1 player .x +!               \ move player left
                     \ Mark The Current Horizontal Border As Filled
                     TRUE player .x @ player .y @ hline-ndx !
                   then
                endof
    SDLK_UP     of player .y @ 0>
                   player .fx @ player .x @ 60 * = AND
                   player .fy @ player .y @ 40 * = AND if
                     -1 player .y +!                 \ move player up
		    \ Mark The Current Vertical Border As Filled
                     TRUE player .x @ player .y @ vline-ndx !

                   then
                endof
    SDLK_DOWN   of player .y @ 10 <
                   player .fx @ player .x @ 60 * = AND
                   player .fy @ player .y @ 40 * = AND if
		    \ Mark The Current Vertical Border As Filled
                     TRUE player .x @ player .y @ vline-ndx !
                     1 player .y +!                \ move player down
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
    SDLK_a        of FALSE to key-a     endof
    SDLK_SPACE    of FALSE to key-Space endof
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
    SDLK_RIGHT    of FALSE to key-Right endof
    SDLK_LEFT     of FALSE to key-Left  endof
  endcase
;

\ ---[ BuildFont ]---------------------------------------------------
\ Function to build our OpenGL font list (from Lesson 17)

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
        0 16 gl-vertex-2i
        \ texture coordinate - bottom right
        bf-cx F@ bf-cy F@ gl-tex-coord-2f
        \ vertex coordinate - bottom right
        16 16 gl-vertex-2i
        \ texture coordinate - top right
        bf-cx F@ bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top right
        16 0 gl-vertex-2i
        \ texture coordinate - top left
        bf-cx F@ 0.0625e F- bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top left
        0 0 gl-vertex-2i
      gl-end
      \ Move to the left of the character
      15e 0e 0e gl-translate-d
    gl-end-list
  loop
;

\ ---[ glPrint ]-----------------------------------------------------
\ Prints a string
\ <set> selects Normal <0>, or Italic <1> from the font.bmp image.

: glPrint { _x _y *str _len _set -- }  
  _set 1 > if 1 to _set then
  GL_TEXTURE_2D gl-enable                    \ Enable texture mapping
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture  \ Select our texture
  GL_DEPTH_TEST gl-disable                    \ Disable depth testing
  gl-load-identity                       \ Reset the modelview matrix
  _x S>F _y S>F 0e gl-translate-d  \ Position text (0,0==bottom left)
  baselist @ 32 - 128 _set * + gl-list-base     \ Choose the font set
  _set 0= if                     \ if set 0 is used, enlarge the font
    1.5e 2e 1e gl-scale-f            \ scale width and height of font
  then
  _len GL_BYTE *str gl-call-lists                    \ Write the text
  GL_TEXTURE_2D gl-disable                  \ Disable texture mapping
  GL_DEPTH_TEST gl-enable                   \ Re-enable depth testing
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    FALSE                                        \ Return a bad value
  else
    BuildFont                                        \ build the font
    GL_SMOOTH gl-shade-model                  \ Enable smooth shading
    0e 0e 0e 0.5e gl-clear-color           \ Set the background black
    1e gl-clear-depth                            \ Depth buffer setup
    GL_LINE_SMOOTH_HINT GL_NICEST gl-hint     \ set line antialiasing
    GL_BLEND gl-enable                              \ Enable blending
    GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA gl-blend-func    \ blend type
    TRUE                                        \ Return a good value
  then
;

\ ---[ PlaySound ]---------------------------------------------------
\ Starts (or stops) a sound via the SDL_mixer package.
\ To stop a sound, call PlaySound with 0 0 0 as parameters.

: PlaySound { *sound _len _repeat -- }
  *sound 0= if
    1 mix-halt-channel drop
    Chunk mix-free-chunk
    0 to Chunk
  else
    Chunk 0<> if
      1 mix-halt-channel drop
      Chunk mix-free-chunk
      0 to Chunk
    then
    *sound _len zstring mix-load-wav dup to Chunk 0= if
      cr ." Failed to load sound: " *sound zprint cr
    then
    -1 Chunk _repeat mix-play-channel
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  baselist 256 gl-delete-lists               \ clean up the font list
  NumTextures texture gl-delete-textures          \ clean up textures
  FALSE to opengl-exit-flag          \ reset these flag for next time
  Sound if
    mix-halt-music drop                      \ stop playing the music
    Music mix-free-music           \ free the music memory allocation
    Chunk mix-free-chunk                 \ free sfx memory allocation
    mix-close-audio                          \ close the audio device
    SDL_INIT_AUDIO sdl-quit-sub-system
  then
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

\ ---[ temp$ ]-------------------------------------------------------
\ A work buffer for string manipulations. Should always be considered
\ to be temporary.

create temp$ here 256 dup allot 0 fill           \ temp string buffer

\ ---[ IntToStr ]----------------------------------------------------
\ Converts an integer value to a string; returns addr/len

: IntToStr ( n -- str len ) 0 <# #S #> ;

\ ---[ concat-string ]-----------------------------------------------
\ A basic string concatenation function, to add one string to another

: concat-string { *str _len *dst -- }
  *str *dst 1+ *dst C@ + _len cmove
  *dst C@ _len + *dst C!                \ use *dst[0] as length byte
;

\ ---[ set-string ]--------------------------------------------------
\ Copies the *src string to the *dst address. *dst[0] is the length.

: set-string ( *str len *dst -- )
  0 over C!                                         \ set length to 0
  concat-string                                   \ copy *str to *dst
;

\ ---[ DrawGLScene ]-------------------------------------------------
\ Here goes our drawing code 

: Show-Game-Name ( -- )
  1e 0.5e 1e gl-color-3f                        \ set color to purple
  207 24 s" GRID CRAZY" 0 glPrint
;

: Show-Level ( -- )
  1e 1e 0e gl-color-3f                          \ set color to yellow
  s" Level: " temp$ set-string
  displayed-level IntToStr temp$ concat-string
  20 20 temp$ dup 1+ swap C@ 1 glPrint

;

: Show-Stage ( -- )
  s" Stage: " temp$ set-string
  game-stage IntToStr temp$ concat-string
  20 40 temp$ dup 1+ swap C@ 1 glPrint
;

: Show-GameOver ( -- )
    255 random 255 random 255 random gl-color-3ub \ pick random color
    472 20 s" GAME OVER" 1 glPrint
    456 40 s" PRESS SPACE" 1 glPrint
;

: Draw-Player ( -- )
  \ Start Drawing Our Player Using Lines
  player-lives 1- 0 do
    gl-load-identity                                 \ reset the view
    490e i 40 * S>F F+ 40e 0e gl-translate-f \ move to right of title
    player .spin F@ FNEGATE 0e 0e 1e gl-rotate-f \ spin counter clock
    0e 1e 0e gl-color-3f            \ set player color to light green
    GL_LINES gl-begin
      -5e -5e gl-vertex-2d                       \ top left of player
       5e  5e gl-vertex-2d                   \ bottom right of player
       5e -5e gl-vertex-2d                      \ top right of player
      -5e  5e gl-vertex-2d                    \ bottom left of player
    gl-end
    
    \ Rotate counter-clockwise
    player .spin F@ FNEGATE 0.5e F* 0e 0e 1e gl-rotate-f
    \ set player color to dark green
    0e 0.75e 0e gl-color-3f

    GL_LINES gl-begin
      -7e  0e gl-vertex-2d                    \ left center of player
       7e  0e gl-vertex-2d                   \ right center of player
       0e -7e gl-vertex-2d                     \ top center of player
       0e  7e gl-vertex-2d                  \ bottom center of player
    gl-end
  loop
;
  
: Draw-Hourglass ( -- )
  gl-load-identity                       \ reset the modelview matrix
  \ move to the fine hourglass position
  20 hourglass .x @ 60 * + S>F
  70 hourglass .y @ 40 * + S>F 0e gl-translate-f
  hourglass .spin F@ 0e 0e 1e gl-rotate-f          \ rotate clockwise
  255 random 255 random 255 random gl-color-3ub        \ random color
  GL_LINES gl-begin         \ Start drawing the hourglass using lines
    -5e -5e gl-vertex-2d                      \ top left of hourglass
     5e  5e gl-vertex-2d                  \ bottom right of hourglass
     5e -5e gl-vertex-2d                     \ top right of hourglass
    -5e  5e gl-vertex-2d                   \ bottom left of hourglass
    -5e  5e gl-vertex-2d                   \ bottom left of hourglass
     5e  5e gl-vertex-2d                  \ bottom right of hourglass
    -5e -5e gl-vertex-2d                      \ top left of hourglass
     5e -5e gl-vertex-2d                     \ top right of hourglass
  gl-end
;

: Draw-Enemies ( -- )
  game-stage internal-level * 0 do
    gl-load-identity                     \ reset the modelview matrix
    i enemies-ndx .fx @ S>F 20e F+
    i enemies-ndx .fy @ S>F 70e F+ 0e gl-translate-f
    1e 0.5e 05e gl-color-3f                    \ make enemy body pink
    GL_LINES gl-begin                                    \ draw enemy
       0e -7e gl-vertex-2d                        \ top point of body
      -7e  0e gl-vertex-2d                       \ left point of body
      -7e  0e gl-vertex-2d                       \ left point of body
       0e  7e gl-vertex-2d                     \ bottom point of body
       0e  7e gl-vertex-2d                     \ bottom point of body
       7e  0e gl-vertex-2d                      \ right point of body
       7e  0e gl-vertex-2d                      \ right point of body
       0e -7e gl-vertex-2d                        \ top point of body
    gl-end

    i enemies-ndx .spin F@ 0e 0e 1e gl-rotate-f  \ rotate enemy blade
    1e 0e 0e gl-color-3f                       \ make enemy blade red
    
    GL_LINES gl-begin
      -7e -7e gl-vertex-2d                        \ top left of enemy
       7e  7e gl-vertex-2d                    \ bottom right of enemy
      -7e  7e gl-vertex-2d                     \ bottom left of enemy
       7e -7e gl-vertex-2d                       \ top right of enemy
    gl-end
  loop
;

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear

  gl-load-identity                                   \ restore matrix

  Show-Game-Name
  Show-Level
  Show-Stage

  gameover if
    Show-GameOver
  then

  Draw-Player  

  TRUE to grid-filled                    \ set to TRUE before testing
  2e gl-line-width                  \ set line width for cells to 2.0
  GL_LINE_SMOOTH gl-disable                   \ disable anti-aliasing
  gl-load-identity               \ reset the current modelview matrix
  11 0 do
    11 0 do
      0e 0.5e 1e gl-color-3f
      j i hline-ndx @ if       \ has the horizontal line been traced?
        1e 1e 1e gl-color-3f
      then
      j 10 < if                    \ do not draw too far to the right
        j i hline-ndx @ 0= if
          FALSE to grid-filled    \ the horizontal line is not filled
        then
        GL_LINES gl-begin
          \ Left side of horizontal line
          20e j 60 * S>F F+ 70e i 40 * S>F F+ gl-vertex-2d
          \ Right side of horizontal line
          80e j 60 * S>F F+ 70e i 40 * S>F F+ gl-vertex-2d
        gl-end
      then
      0e 0.5e 1e gl-color-3f                 \ set line color to blue
      j i vline-ndx @ if         \ has the vertical line been traced?
        1e 1e 1e gl-color-3f                \ set line color to white
      then
      i 10 < if                            \ do not draw too far down
        j i vline-ndx @ 0= if      \ if a vertical line is not filled
          FALSE to grid-filled
        then
        GL_LINES gl-begin
          \ Left side of horizontal line
          20e j 60 * S>F F+ 70e i 40 * S>F F+ gl-vertex-2d
          \ Right side of horizontal line
          20e j 60 * S>F F+ 110e i 40 * S>F F+ gl-vertex-2d
        gl-end
      then
      GL_TEXTURE_2D gl-enable                \ Enable texture mapping
      1e 1e 1e gl-color-3f                  \ Set color to full white
      GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture
      j 10 < i 10 < AND if       \ If in bounds, fill in traced boxes
        \ Are all sides in the box traced?
        j i hline-ndx @
        j i 1+ hline-ndx @ AND
        j i vline-ndx @ AND
        j 1+ i vline-ndx @ AND if
          GL_QUADS gl-begin                    \ Draw a textured quad
            j S>F 10e F/ 0.1e F+
            1e i S>F 10e F/ F- gl-tex-coord-2f            \ top right
            20 j 60 * + 59 + S>F 70 i 40 * + 1 + S>F gl-vertex-2d
            
            j S>F 10e F/ 
            1e i S>F 10e F/ F- gl-tex-coord-2f             \ top left
            20 j 60 * + 1 + S>F 70 i 40 * + 1 + S>F gl-vertex-2d
            
            j S>F 10e F/ 
            1e i S>F 10e F/ 0.1e F+ F- gl-tex-coord-2f  \ bottom left
            20 j 60 * + 1 + S>F 70 i 40 * + 39 + S>F gl-vertex-2d
            
            j S>F 10e F/ 0.1e F+
            1e i S>F 10e F/ 0.1e F+ F- gl-tex-coord-2f \ bottom right
            20 j 60 * + 59 + S>F 70 i 40 * + 39 + S>F gl-vertex-2d
          gl-end
        then
      then
      GL_TEXTURE_2D gl-disable              \ Disable texture mapping
    loop
  loop

  1e gl-line-width                        \ set the line width to 1.0
  
  anti if
    GL_LINE_SMOOTH gl-enable                   \ enable anti-aliasing
  then
  
  hourglass .fx @ 1 = if
    Draw-Hourglass
  then

  gl-load-identity                       \ reset the modelview matrix
  \ Move to the fine player position
  player .fx @ S>F 20e F+ player .fy @ S>F 70E F+ 0e gl-translate-f
  player .spin F@ 0e 0e 1e gl-rotate-f             \ rotate clockwise
  0e 1e 0e gl-color-3f              \ set player color to light green

  \ Draw the player using lines
  GL_LINES gl-begin
    -5e -5e gl-vertex-2d                         \ top left of player
     5e  5e gl-vertex-2d                     \ bottom right of player
     5e -5e gl-vertex-2d                        \ top right of player
    -5e  5e gl-vertex-2d                      \ bottom left of player
  gl-end

  player .spin F@ 0.5e F* 0e 0e 1e gl-rotate-f     \ rotate clockwise
  0e 0.75e 0e gl-color-3f            \ set player color to dark green

  GL_LINES gl-begin
    -7e  0e gl-vertex-2d                      \ left center of player
     7e  0e gl-vertex-2d                     \ right center of player
     0e -7e gl-vertex-2d                       \ top center of player
     0e  7e gl-vertex-2d                    \ bottom center of player
  gl-end
  
  Draw-Enemies                                     \ Draw the enemies

  sdl-gl-swap-buffers                         \ Draw it to the screen

  fps-frames 1+ to fps-frames   \ Gather  our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  TRUE                                          \ Return a good value
;

